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
    /// <summary>
    /// Service for adding WordPress posts to Azure Search.
    /// </summary>
    public class AzureSearchIndexer
    {
        // Service Name
        public string ServiceName {get; set;}
        // Service Key - needs to be the admin key in order to add index and documents
        public string ServiceKey { get; set; }
        // Name of the index to add and/or post documents
        public string Index { get; set; }
        // Collection of WordPress posts to add to the index
        public WordPressPosts WordPressPosts { get; set; }

        // maximum number of documents to add per batch.  RedDog.Search has a maximum value of 1000 posts and 16 MB per batch, so 100 is conservative.
        private int maximumNumberOfDocumentsPerBatch = 100;

        // RedDog.Search API objects
        private ApiConnection connection = null;
        private IndexManagementClient managementClient = null;
        private IndexQueryClient queryClient = null;

        // Tracking of whether connection Azure Search has been previously made
        bool connected = false;
        
        // basic constructor to set properties for the service
        public AzureSearchIndexer(string ServiceName, string ServiceKey, string Index, WordPressPosts Posts)
        {
            this.ServiceKey = ServiceKey;
            this.ServiceName = ServiceName;
            this.Index = Index;
            this.WordPressPosts = Posts;
        }

        // make a connection to Azure Search using RedDog.Search client
        public void Connect()
        {
            connection = ApiConnection.Create(ServiceName, ServiceKey);
            managementClient = new IndexManagementClient(connection);
            queryClient = new IndexQueryClient(connection);
            connected = true;
        }

        // main method for adding posts.  Adds all the posts in the WordPressPosts collection.
        public async Task AddPosts()
        {
            // if not previously connected, make a connection
            if (!connected)
                    Connect();

            // create the index if it hasn't already been created.
            await CreateIndex();

            // run index population in batches.  The Reddog.Search client maxes out at 1000 operations or about 16 MB of data transfer, so we have set the maximum to 100 posts in a batch to be conservative.
            int batchCount = 0;
            List<IndexOperation> IndexOperationList = new List<IndexOperation>(maximumNumberOfDocumentsPerBatch);
            
            foreach (WordPressPost post in WordPressPosts.Posts)
            {
                batchCount++;
                // create an indexoperation with the appropriate metadata and supply it with the incoming WordPress post
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

                // add the index operation to the collection
                IndexOperationList.Add(indexOperation);

                // if we have added maximum number of documents per batch, add the collection of operations to the index and then reset the collection to add a new batch.
                if (batchCount >= maximumNumberOfDocumentsPerBatch)
                {
                    
                    var result = await managementClient.PopulateAsync(Index, IndexOperationList.ToArray());
                    if (!result.IsSuccess)
                        Console.Out.WriteLine(result.Error.Message);
                    batchCount = 0;
                    IndexOperationList = new List<IndexOperation>(maximumNumberOfDocumentsPerBatch);
                }
                    
            }
            // look for any remaining items that have not yet been added to the index.
            var remainingResult = await managementClient.PopulateAsync(Index, IndexOperationList.ToArray() );
            if (!remainingResult.IsSuccess)
                Console.Out.WriteLine(remainingResult.Error.Message);
        }

        // delete the index.
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

        /// <summary>
        /// Disconnect drops the connections and disposes of the objects.
        /// </summary>
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
