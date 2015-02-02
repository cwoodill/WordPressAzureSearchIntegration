angular.module('app', ['ui.bootstrap']);

angular.module('app').controller("SearchController", function ($scope, $http) {
    var vm = this;
    vm.keywords = "";
    vm.APIVersion = "2014-07-31-Preview";
    vm.APIKey = "728C22304D806095B2B2431C9DCA6EBC";
    vm.URL = "https://wordpresssearch.search.windows.net/indexes/wordpresssearchintegration/docs";
    vm.preparedURL = "";
    vm.showSearchResults = false;
    vm.title = "Azure Search Example";
    vm.orderby = "Relevance";
    vm.results = [];
    vm.dataObject = {};
    var config = {
        headers: {
            'api-key': '728C22304D806095B2B2431C9DCA6EBC',
            'Accept': 'application/json',
        }
    };
    vm.submit = function (item, event) {
        if (vm.orderby == "Relevance")
            var URLstring = vm.URL + "?search=" + vm.keywords + "&api-version=" + vm.APIVersion;
        else
            var URLstring = vm.URL + "?search=" + vm.keywords + "&$orderby=CreateDate desc" + "&api-version=" + vm.APIVersion;

        if (!isEmpty(vm.keywords))
        {
            var responsePromise = $http.get(URLstring, config, {});
            responsePromise.success(function (dataFromServer, status, headers, config) {
                vm.results = dataFromServer.value;
                vm.showSearchResults = true;
            });
            responsePromise.error(function (data, status, headers, config) {
                alert("Submitting form failed!");
            });
        }
        else
        {
            vm.showSearchResults = false;
            vm.results = [];
        }
            
    }


});

angular.module('app').filter('unsafe', function ($sce) {
    return function (val) {
        return $sce.trustAsHtml(val);
    };
});

function isEmpty(str)
{
    var isEmpty = false;
    if (!str)
        isEmpty = true;
    else
    {
        if (str.length == 0)
            isEmpty = true;
        else if (str.trim().length == 0)
            isEmpty = true;
    }
    return isEmpty;

}
