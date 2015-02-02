using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedDog.Search;
using RedDog.Search.Http;
using RedDog.Search.Model;

namespace WordPressAzureSearchParser
{
    public class AzureSearchIndexer
    {
        public string ServiceName {get; set;}
        public string ServiceKey { get; set; }
        public string Index { get; set; }
        public WordPressPosts WordPressPosts { get; set; }
        public string LastGoodPostID { get; set; }

        private int maximumNumberOfDocumentsPerBatch = 100;
        private ApiConnection connection = null;
        private IndexManagementClient managementClient = null;
        private IndexQueryClient queryClient = null;
        bool connected = false;
        
        public AzureSearchIndexer(string ServiceName, string ServiceKey, string Index, WordPressPosts Posts)
        {
            this.ServiceKey = ServiceKey;
            this.ServiceName = ServiceName;
            this.Index = Index;
            this.WordPressPosts = Posts;
        }

        public async Task GetLastGoodPostID()
        {
            string id = "";
            if (!connected)
                Connect();

            await CreateIndex();
            var queryClient = new IndexQueryClient(connection);
            var query = new SearchQuery("")
                .Count(true)
                .Select("Id")
                .OrderBy("postedOn")
                .Highlight("title")
                .Filter("year gt 2002 and make eq 'Volvo'");
            var searchResults = await queryClient.SearchAsync("cars", query);
            foreach (var result in searchResults.Body.Records)
            {
                // Do something with the properties: result.Properties["title"], result.Properties["description"]
            }

        }

        public void Connect()
        {
            connection = ApiConnection.Create(ServiceName, ServiceKey);
            managementClient = new IndexManagementClient(connection);
            queryClient = new IndexQueryClient(connection);
            connected = true;
        }

        public async Task AddPosts()
        {

            if (!connected)
                    Connect();

            await CreateIndex();

            // run index population in batches.  The Reddog.Search client maxes out at 1000 operations or about 16 MB of data transfer, so we have set the maximum to 100 posts in a batch to be conservative.
            int batchCount = 0;
            List<IndexOperation> IndexOperationList = new List<IndexOperation>(maximumNumberOfDocumentsPerBatch);
            
            foreach (WordPressPost post in WordPressPosts.Posts)
            {
                batchCount++;
                IndexOperation indexOperation = new IndexOperation(IndexOperationType.MergeOrUpload, "Id", post.Id.ToString())
                    .WithProperty("Title", post.Title)
                    .WithProperty("Content", post.Content)
                    .WithProperty("Excerpt", post.Excerpt)
                    .WithProperty("CreateDate", post.CreateDate.ToUniversalTime())
                    .WithProperty("ModifiedDate", post.ModifiedDate.ToUniversalTime())
                    .WithProperty("CreateDateAsString", post.CreateDate.ToLongDateString())
                    .WithProperty("ModifiedDateAsString", post.ModifiedDate.ToLongDateString())
                    .WithProperty("Author", post.Author)
                    .WithProperty("Categories", post.Categories)
                    .WithProperty("Tags", post.Tags)
                    .WithProperty("Slug", post.Slug)
                    .WithProperty("CommentCount", post.CommentCount)
                    .WithProperty("CommentContent", post.CommentContent);
                IndexOperationList.Add(indexOperation);
                if (batchCount >= maximumNumberOfDocumentsPerBatch)
                {
                    var result = await managementClient.PopulateAsync(Index, IndexOperationList.ToArray());
                    if (!result.IsSuccess)
                        Console.Out.WriteLine(result.Error.Message);
                    batchCount = 0;
                    IndexOperationList = new List<IndexOperation>(maximumNumberOfDocumentsPerBatch);
                }
                    
            }
            var remainingResult = await managementClient.PopulateAsync(Index, IndexOperationList.ToArray() );
            if (!remainingResult.IsSuccess)
                Console.Out.WriteLine(remainingResult.Error.Message);
        }

        public async Task DeleteIndex()
        {
            var result = await managementClient.DeleteIndexAsync(Index);
        }

        /// <summary>
        /// Creates an Index in Azure Search
        /// </summary>
        /// <returns></returns>
        public async Task CreateIndex()
        {
            // check to see if index exists.  If not, then create it.
            var result = await managementClient.GetIndexAsync(Index);
            if (!result.IsSuccess)
            {
                result = await managementClient.CreateIndexAsync(new Index(Index)
                    .WithStringField("Id", f => f.IsKey().IsRetrievable())
                    .WithStringField("Title", f => f.IsRetrievable().IsSearchable())
                    .WithStringField("Content", f => f.IsSearchable().IsRetrievable())
                    .WithStringField("Excerpt", f => f.IsRetrievable())
                    .WithDateTimeField("CreateDate", f => f.IsRetrievable().IsSortable().IsFilterable().IsFacetable())
                    .WithDateTimeField("ModifiedDate", f => f.IsRetrievable().IsSortable().IsFilterable().IsFacetable())
                    .WithStringField("CreateDateAsString", f => f.IsSearchable().IsRetrievable().IsFilterable())
                    .WithStringField("ModifiedDateAsString", f => f.IsSearchable().IsRetrievable().IsFilterable())
                    .WithStringField("Author", f=>f.IsSearchable().IsRetrievable().IsFilterable())
                    .WithStringField("Categories", f => f.IsSearchable().IsRetrievable())
                    .WithStringField("Tags", f => f.IsSearchable().IsRetrievable())
                    .WithStringField("Slug", f => f.IsRetrievable())
                    .WithIntegerField("CommentCount", f => f.IsRetrievable())
                    .WithStringField("CommentContent", f=>f.IsSearchable().IsRetrievable())


                );
                if (!result.IsSuccess)
                {
                    Console.Out.WriteLine(result.Error.Message);
                }
            }
        }

        public void Disconnect()
        {
            if (connected)
            {
                queryClient.Dispose();
                managementClient.Dispose();
                connection.Dispose();
            }

        }

         
    }
}
