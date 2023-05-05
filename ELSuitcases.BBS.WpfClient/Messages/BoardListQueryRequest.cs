using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class BoardListQueryRequest
    {
        public Uri RequestURL { get; private set; }
        public string BoardId { get; private set; } = string.Empty;



        public BoardListQueryRequest(Uri urlApiRequest, string boardId = "")
        {
            RequestURL = urlApiRequest;
            BoardId = boardId;
        }
    }
}
