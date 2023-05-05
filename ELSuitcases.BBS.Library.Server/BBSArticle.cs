using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace ELSuitcases.BBS.Library.Server
{
    public class BBSArticle
    {
        #region [SELECT]
        public static Task<DataTable> List(string? bbsId)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = (string.IsNullOrEmpty(bbsId)) ? string.Empty : bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                }
            };

            return OracleHelper.Select(OracleHelper.GetConnection(), "PKG_BBS_Article.List_All", CommandType.StoredProcedure, parameters);
        }

        public static Task<DataTable> List_By_Page(string bbsId, int pageNo = 1, int pageSize = 10, string keywordTitle = "")
        {
            OracleParameter paraTotalCount = new OracleParameter()
            {
                ParameterName = "p_total_count",
                Value = -1,
                Direction = ParameterDirection.Output,
                OracleDbType = OracleDbType.Int32
            };
            OracleParameter paraPageCount = new OracleParameter()
            {
                ParameterName = "p_page_count",
                Value = -1,
                Direction = ParameterDirection.Output,
                OracleDbType = OracleDbType.Int32
            };

            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = (string.IsNullOrEmpty(bbsId)) ? string.Empty : bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_keyword_t",
                    Value = keywordTitle ?? string.Empty,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_page_no",
                    Value = pageNo,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_page_size",
                    Value = pageSize,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                },
                paraTotalCount,
                paraPageCount
            };

            return OracleHelper.Select(OracleHelper.GetConnection(), "PKG_BBS_Article.List_By_Page", CommandType.StoredProcedure, parameters);
        }

        public static async Task<ArticleDTO?> Read_Article(string bbsId, string articleId, int replyId, bool isIncreaseReadCount = false)
        {
            ArticleDTO? article = null;
            
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_article_id",
                    Value = articleId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_reply_id",
                    Value = replyId,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_is_increase_count",
                    Value = isIncreaseReadCount ? 'Y' : 'N',
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Char
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                }
            };

            await OracleHelper.Select(
                        OracleHelper.GetConnection(),
                        "PKG_BBS_Article.Read_Article",
                        CommandType.StoredProcedure,
                        parameters)
                    .ContinueWith((t) =>
                    {
                        if ((t.Result is DataTable dt) && (dt.Rows.Count > 0))
                            article = Common.ConvertFromDataTableToArticleDTOList(dt).First();
                    });

            return article;
        }

        public static Task<DataTable> Search_Article_By_Title(string bbsId, string title)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_title",
                    Value = title,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.RefCursor
                }
            };

            return OracleHelper.Select(OracleHelper.GetConnection(), "PKG_BBS_Article.Search_Article_By_Title", CommandType.StoredProcedure, parameters);
        }
        #endregion

        #region [WRITE]
        public static Task<int> Delete_Article(string bbsId, string articleId, int replyId)
        {
            List<OracleParameter> parameters = new List<OracleParameter>
            {
                new OracleParameter()
                {
                    ParameterName = "p_bbs_id",
                    Value = bbsId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_article_id",
                    Value = articleId,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_reply_id",
                    Value = replyId,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            return OracleHelper.Write(OracleHelper.GetConnection(), "PKG_BBS_Article.Delete_Article", CommandType.StoredProcedure, parameters, "p_result");
        }

        public static async Task<Tuple<int, string, string, int>> Reply_Article(
            string bbsId, string articleId, DateTime writingTime, Constants.BBSArticleType articleType, string title, string contents, string writer, string articlePassword)
        {
            Tuple<int, string, string, int> result = new Tuple<int, string, string, int>(-1, string.Empty, string.Empty, -1);

            OracleParameter paraBBSID = new OracleParameter()
            {
                ParameterName = "p_bbs_id",
                Value = bbsId,
                Direction = ParameterDirection.Input
            };
            OracleParameter paraArticleID = new OracleParameter()
            {
                ParameterName = "p_article_id",
                Value = articleId,
                Direction = ParameterDirection.Input
            };
            OracleParameter paraReplyID = new OracleParameter()
            {
                ParameterName = "p_reply_id_new",
                Value = -1,
                Direction = ParameterDirection.Output,
                OracleDbType = OracleDbType.Int32
            };

            List<OracleParameter> parameters = new List<OracleParameter>
            {
                paraBBSID,
                paraArticleID,
                paraReplyID,
                new OracleParameter()
                {
                    ParameterName = "p_article_type",
                    Value = Convert.ToInt32(articleType),
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_title",
                    Value = title,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_writer",
                    Value = writer,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_written_time",
                    Value = writingTime,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_article_password",
                    Value = articlePassword,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_contents",
                    Value = contents,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.NClob
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            int dbResult = await OracleHelper.Write(
                                    OracleHelper.GetConnection(),
                                    "PKG_BBS_Article.Reply_Article",
                                    CommandType.StoredProcedure,
                                    parameters,
                                    "p_result");

            result = new Tuple<int, string, string, int>(
                            dbResult,
                            paraBBSID.Value.ToString() ?? string.Empty,
                            paraArticleID.Value.ToString() ?? string.Empty,
                            Convert.ToInt32(paraReplyID.Value.ToString()));

            return result;
        }

        public static async Task<Tuple<int, string, string, int>> Update_Article(
            string bbsId, string articleId, int replyId, Constants.BBSArticleType articleType, string title, string contents, string articlePassword, bool isDeleted)
        {
            Tuple<int, string, string, int> result = new Tuple<int, string, string, int>(-1, string.Empty, string.Empty, -1);

            OracleParameter paraBBSID = new OracleParameter()
            {
                ParameterName = "p_bbs_id",
                Value = bbsId,
                Direction = ParameterDirection.Input
            };
            OracleParameter paraArticleID = new OracleParameter()
            {
                ParameterName = "p_article_id",
                Value = articleId,
                Direction = ParameterDirection.Input
            };
            OracleParameter paraReplyID = new OracleParameter()
            {
                ParameterName = "p_reply_id",
                Value = replyId,
                Direction = ParameterDirection.Input,
                OracleDbType = OracleDbType.Int32
            };

            List<OracleParameter> parameters = new List<OracleParameter>
            {
                paraBBSID,
                paraArticleID,
                paraReplyID,
                new OracleParameter()
                {
                    ParameterName = "p_article_type",
                    Value = Convert.ToInt32(articleType),
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_title",
                    Value = title,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_article_password",
                    Value = articlePassword,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_is_deleted",
                    Value = isDeleted ? "1" : "0",
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_contents",
                    Value = contents,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.NClob
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                }
            };

            int dbResult = await OracleHelper.Write(
                                    OracleHelper.GetConnection(),
                                    "PKG_BBS_Article.Update_Article",
                                    CommandType.StoredProcedure,
                                    parameters,
                                    "p_result");

            result = new Tuple<int, string, string, int>(
                            dbResult,
                            paraBBSID.Value.ToString() ?? string.Empty,
                            paraArticleID.Value.ToString() ?? string.Empty,
                            Convert.ToInt32(paraReplyID.Value.ToString()));

            return result;
        }

        public static async Task<Tuple<int, string, string, int>> Write_Article(
            string bbsId, DateTime writingTime, Constants.BBSArticleType articleType, string title, string contents, string writer, string articlePassword, Random? randomNo = null)
        {
            Tuple<int, string, string, int> result = new Tuple<int, string, string, int>(-1, string.Empty, string.Empty, -1);

            OracleParameter paraBBSID = new OracleParameter()
            {
                ParameterName = "p_bbs_id",
                Value = bbsId,
                Direction = ParameterDirection.Input
            };
            OracleParameter paraArticleID = new OracleParameter()
            {
                ParameterName = "p_article_id",
                Value = Common.Generate16IdentityCode(writingTime, randomNo ?? new Random(1)),
                Direction = ParameterDirection.Input
            };
            OracleParameter paraReplyID = new OracleParameter()
            {
                ParameterName = "p_reply_id_new",
                Value = -1,
                Direction = ParameterDirection.Output,
                OracleDbType = OracleDbType.Int32
            };

            List<OracleParameter> parameters = new List<OracleParameter>
            {
                paraBBSID,
                paraArticleID,
                new OracleParameter()
                {
                    ParameterName = "p_article_type",
                    Value = Convert.ToInt32(articleType),
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.Int32
                },
                new OracleParameter()
                {
                    ParameterName = "p_title",
                    Value = title,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_writer",
                    Value = writer,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_written_time",
                    Value = writingTime,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_article_password",
                    Value = articlePassword,
                    Direction = ParameterDirection.Input
                },
                new OracleParameter()
                {
                    ParameterName = "p_contents",
                    Value = contents,
                    Direction = ParameterDirection.Input,
                    OracleDbType = OracleDbType.NClob
                },
                new OracleParameter()
                {
                    ParameterName = "p_result",
                    Value = -1,
                    Direction = ParameterDirection.Output,
                    OracleDbType = OracleDbType.Int32
                },
                paraReplyID
            };

            int dbResult = await OracleHelper.Write(
                                    OracleHelper.GetConnection(),
                                    "PKG_BBS_Article.Write_Article",
                                    CommandType.StoredProcedure,
                                    parameters,
                                    "p_result");

            if (dbResult > 0)
            {
                result = new Tuple<int, string, string, int>(
                                dbResult,
                                paraBBSID.Value.ToString() ?? string.Empty,
                                paraArticleID.Value.ToString() ?? string.Empty,
                                Convert.ToInt32(paraReplyID.Value.ToString()));
            }
            else
            {
                result = new Tuple<int, string, string, int>(
                                dbResult,
                                string.Empty,
                                string.Empty,
                                -1);
            }

            return result;
        }
        #endregion
    }
}
