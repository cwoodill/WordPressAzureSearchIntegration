using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace WordPressAzureSearchParser
{
    public class WordPressJSONLoader
    {
        
        public static WordPressPosts LoadAllPosts(string URL)
        {
            try
            {
                WordPressPosts wordPressPosts = new WordPressPosts();
                string query = "?json=get_posts";
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(URL + query);
                StreamReader reader = new StreamReader(stream);
                var results = JObject.Parse(reader.ReadLine());
                var JsonPosts = results["posts"];

                foreach (var JsonPost in JsonPosts)
                {
                    wordPressPosts.Posts.Add(loadPostFromJToken(JsonPost));
                }

                int pages = (int) results["pages"];
                if (pages > 1)
                {
                    for(int i = 2; i<=pages; i++)
                    {
                        query = "?json=get_posts&page=" + i;
                        stream = client.OpenRead(URL + query);
                        reader = new StreamReader(stream);
                        results = JObject.Parse(reader.ReadLine());
                        JsonPosts = results["posts"];

                        foreach (var JsonPost in JsonPosts)
                        {
                            wordPressPosts.Posts.Add(loadPostFromJToken(JsonPost));
                        }
                    }
                }

                return wordPressPosts;

            }
            catch (Exception e)
            {
                throw;
            }
                
        
        }

        private static WordPressPost loadPostFromJToken(JToken JsonPost)
        {
            WordPressPost post = new WordPressPost();
            post.Id = (int)JsonPost["id"];
            post.Title = (string)JsonPost["title"];
            post.Slug = (string)JsonPost["slug"];
            post.Content = (string)JsonPost["content"];
            post.Excerpt = (string)JsonPost["excerpt"];
            post.CreateDate = (DateTime)JsonPost["date"];
            post.ModifiedDate = (DateTime)JsonPost["modified"];
            var Author = JsonPost["author"];
            if (Author != null)
                post.Author = (string) Author["first_name"] + " " + (string) Author["last_name"] + " " + (string) Author["slug"];
            
            // grab the content count and the text of all the comments for this post.
            post.CommentCount = (int) JsonPost["comment_count"];
            string commentContent = "";
            if (post.CommentCount > 0)
            {
                foreach (var Comment in JsonPost["comments"])
                {
                    string content = (string) Comment["content"];
                    commentContent += content;
                }
            }
            post.CommentContent = commentContent;


            // grab all the tags
            var tags = JsonPost["tags"];
            if (tags != null)
            {
                string tagContent = "";
                foreach (var tag in tags)
                {
                    string tagTitle = (string)tag["title"];
                    tagContent += tagTitle + ",";
                }
                post.Tags = tagContent;
            }

            // grab all the categories
            var categories = JsonPost["categories"];
            if (categories != null)
            {
                string categoryContent = "";
                foreach (var category in categories)
                {
                    string categoryTitle = (string)category["title"];
                    categoryContent += categoryTitle + ",";
                }
                post.Categories = categoryContent;
            }

            return post;
        }
    }
}
