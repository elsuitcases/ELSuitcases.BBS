using ELSuitcases.BBS.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class BoardListQueryResponse
    {
        public BoardListQueryRequest Request { get; private set; }
        public bool IsCompletedSuccessfully { get; private set; }
        public Exception? Error { get; private set; }
        public List<BoardDTO>? ResultData { get; private set; }
        public int TotalQueriedCount { get; private set; } = 0;



        public BoardListQueryResponse(BoardListQueryRequest request, int totalQueriedCount, bool isSuccess, List<BoardDTO>? resultData = null, Exception? error = null)
        {
            ResultData = null;
            Request = request;
            TotalQueriedCount = (totalQueriedCount < 0) ? 0 : totalQueriedCount;
            IsCompletedSuccessfully = isSuccess;
            ResultData = resultData;
            Error = error;
        }
    }
}
