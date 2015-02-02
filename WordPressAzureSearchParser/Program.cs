using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure;

namespace WordPressAzureSearchParser
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class WordPressAzureSearchParser
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            string webSiteURL = "";
            string searchServiceName = "";
            string searchServiceKey = "";
            string indexName = "";

            // load configuration attributes            
            webSiteURL = CloudConfigurationManager.GetSetting("WebSiteURL");
            searchServiceName = CloudConfigurationManager.GetSetting("ServiceName");
            searchServiceKey = CloudConfigurationManager.GetSetting("ServiceKey");
            indexName = CloudConfigurationManager.GetSetting("IndexName");

            if (webSiteURL == "" || searchServiceName == "" || searchServiceKey == "" || indexName == "")
                throw new Exception("Missing configuration attributes.  Expected values for WebSiteUrl, ServiceName, ServiceKey and IndexName");

            Console.Out.WriteLine("Found WebSiteURL = " + webSiteURL);
            Console.Out.WriteLine("Found SearchServiceName = " + searchServiceName);
            Console.Out.WriteLine("Found SearchServiceKey = " + searchServiceKey);
            Console.Out.WriteLine("Found IndexName = " + indexName);

            MainAsync(webSiteURL, searchServiceName, searchServiceKey, indexName).Wait();
            
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();
        }

        static async Task MainAsync(string WebSiteURL, string SearchServiceName, string SearchServiceKey, string IndexName)
        {
            AzureSearchIndexer indexer = new AzureSearchIndexer(SearchServiceName, SearchServiceKey, IndexName, WordPressJSONLoader.LoadAllPosts(WebSiteURL));
            Task task = indexer.AddPosts();
            task.Wait();
            Console.Out.WriteLine("End of Program.");
        }
    }

}
