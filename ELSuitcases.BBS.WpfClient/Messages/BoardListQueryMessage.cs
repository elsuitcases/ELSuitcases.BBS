using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class BoardListQueryMessage : AsyncRequestMessage<BoardListQueryResponse>
    {
        public string ID { get; private set; } = Guid.NewGuid().ToString();
        public BoardListQueryRequest Query { get; private set; }



        public BoardListQueryMessage(BoardListQueryRequest query) : base()
        {
            Query = query;
        }
    }
}
