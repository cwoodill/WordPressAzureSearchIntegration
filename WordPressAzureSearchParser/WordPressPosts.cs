using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace WordPressAzureSearchParser
{
    public class WordPressPosts
    {
        public List<WordPressPost> Posts { get; set; }

        public WordPressPosts()
        {
            Posts = new List<WordPressPost>();
        }

    }
}
