using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressAzureSearchParser
{
    class AzureSearchIndexerException : Exception
    {
        public AzureSearchIndexerException() : base ()
        {

        }

        public AzureSearchIndexerException(string Message) : base (Message)
        {

        }

        public AzureSearchIndexerException(string Message, Exception InnerException) : base (Message, InnerException)
        {

        }

    }
}
