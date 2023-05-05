using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class ArticleListQueryResponse
    {
        public ArticleListQueryRequest Request { get; private set; }
        public bool IsCompletedSuccessfully { get; private set; }
        public Exception? Error { get; private set; }
        public List<ArticleDTO>? ResultData { get; private set; }
        public int TotalQueriedCount { get; private set; } = 0;
        


        public ArticleListQueryResponse(ArticleListQueryRequest request, int totalQueriedCount, bool isSuccess, List<ArticleDTO>? resultData = null, Exception? error = null)
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
