using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressAzureSearchParser
{
    /// <summary>
    /// Value object representing a single WordPress post.
    /// </summary>
    public class WordPressPost
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Author { get; set; }
        public string Categories { get; set; }
        public string Slug { get; set; }
        public string Tags { get; set; }
        public int CommentCount { get; set; }
        public string CommentContent { get; set; }
    }
}
