using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class ArticleListQueryMessage : AsyncRequestMessage<ArticleListQueryResponse>
    {
        public string ID { get; private set; } = Guid.NewGuid().ToString();
        public ArticleListQueryRequest Query { get; private set; }



        public ArticleListQueryMessage(ArticleListQueryRequest query) : base()
        {
            Query = query;
        }
    }
}
