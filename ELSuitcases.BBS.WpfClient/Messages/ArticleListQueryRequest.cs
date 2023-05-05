using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient.Messages
{
    internal sealed class ArticleListQueryRequest
    {
        public Uri RequestURL { get; private set; }
        public string BoardID { get; private set; }
        public int PageSize { get; private set; }
        public int PageNo { get; private set; }
        public string KeywordTitle { get; private set; }



        public ArticleListQueryRequest(Uri urlApiRequest, string bbsId, int pageSize, int pageNo = 1, string keywordTitle = "")
        {
            if (pageNo < 1) pageNo = Constants.DEFAULT_VALUE_PAGER_PAGE_NO;
            if (pageSize < 1) pageSize = Constants.DEFAULT_VALUE_PAGER_PAGE_SIZE;
            
            RequestURL = urlApiRequest;
            BoardID = bbsId;
            PageSize = pageSize;
            PageNo = pageNo;
            KeywordTitle = keywordTitle;
        }
    }
}
