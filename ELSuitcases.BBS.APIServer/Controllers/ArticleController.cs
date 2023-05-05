using System.Collections.Concurrent;
using System.Data;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.Library.Server;

namespace ELSuitcases.BBS.APIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : BBSControllerBase
    {
        public static readonly ConcurrentDictionary<string, UploadTask> Uploads = new ConcurrentDictionary<string, UploadTask>();
        


        private readonly ILogger<ArticleController> _logger;



        public ArticleController(ILogger<ArticleController> logger)
        {
            _logger = logger;
        }



        private void DoPrepareToUploadTask(object? uploadTask)
        {
            if (uploadTask == null) return;

            UploadTask ut = (UploadTask)uploadTask;
            
            if ((string.IsNullOrEmpty(ut.Request.UserState)) || (ut.Request.UserState.Split(Constants.SEPARATOR_COMMA).Length != 3))
            {
                ProblemFormattedDetailMessage(MethodBase.GetCurrentMethod()?.Name ?? "DoUploadTask",
                                              string.Format("[{0}] {1}", ut.Request.TransferID, Constants.MESSAGE_ERROR_ARGUMENTS_NULL),
                                              new ArgumentNullException());
                return;
            }

            if ((ut.Request.Files == null) || (ut.Request.Files.Count < 1))
                return;

            string apiFuncName = "API_ARTICLE_POST_QUEUE_PREPARE";
            string transID = ut.Request.TransferID;
            string bbsID = ut.Request.UserState.Split(Constants.SEPARATOR_COMMA)[0];
            string articleID = ut.Request.UserState.Split(Constants.SEPARATOR_COMMA)[1];
            string replyID = ut.Request.UserState.Split(Constants.SEPARATOR_COMMA)[2];
            string pathZipFile = Path.Combine(PATH_ATTACHED_FILE_FOLDER, string.Format("{0}_{1}_{2}.zip", bbsID, articleID, replyID));
            DirectoryInfo folderUpload = new DirectoryInfo(Path.Combine(PATH_ATTACHED_FILE_FOLDER, transID));

            if (!Directory.Exists(folderUpload.FullName))
                Directory.CreateDirectory(folderUpload.FullName);

            if (System.IO.File.Exists(pathZipFile))
            {
                ZipFile.ExtractToDirectory(pathZipFile, folderUpload.FullName, Encoding.UTF8, true);
            }

            if (ut.Request.Files.Any(f => f.IsPendingDelete))
            {
                foreach (var f in ut.Request.Files.Where(f => f.IsPendingDelete))
                {
                    System.IO.File.Delete(Path.Combine(folderUpload.FullName, f.FileName));
                }
            }

            OkFormattedDetailMessage(
                    apiFuncName,
                    string.Format("업로드 큐 준비 완료 : TRANS_ID={0}", transID));
        }



        [HttpGet]
        public async Task<List<ArticleDTO>> Get()
        {
            string apiFuncName = "API_ARTICLE_GET";
            OkFormattedDetailMessage(apiFuncName, "게시물 목록 조회 요청 : 전체");
            return Common.ConvertFromDataTableToArticleDTOList(await BBSArticle.List(null));
        }

        [HttpGet("{bbsId}")]
        public async Task<List<ArticleDTO>> Get(string bbsId)
        {
            string apiFuncName = "API_ARTICLE_GET";

            OkFormattedDetailMessage(apiFuncName, string.Format("게시물 목록 조회 요청 : BBS_ID={0}", bbsId));

            var dt = await BBSArticle.List(bbsId);
            var list = dt?.AsEnumerable()
                          .ToDictionary<DataRow, string, ArticleDTO>(
                                new Func<DataRow, string>((r) =>
                                {
                                    return r[Constants.PROPERTY_KEY_NAME_ROW_NUMBER].ToString() ?? "-1";
                                }),
                                new Func<DataRow, ArticleDTO>((r) =>
                                {
                                    ArticleDTO dto = new ArticleDTO();
                                    foreach (DataColumn c in r.Table.Columns)
                                    {
                                        dto.SetValue(c.ColumnName, r[c.ColumnName]);
                                    }
                                    return dto;
                                }))
                          .Values
                          .ToList()
                       ??
                       new List<ArticleDTO>();

            return list;
        }

        [HttpGet("{bbsId}/{pageSize}/{pageNo}")]
        public async Task<List<ArticleDTO>> Get(string bbsId, int pageSize, int pageNo)
        {
            return await Get(bbsId, pageSize, pageNo, string.Empty);
        }

        [HttpGet("{bbsId}/{pageSize}/{pageNo}/{keywordTitle}")]
        public async Task<List<ArticleDTO>> Get(string bbsId, int pageSize, int pageNo, string keywordTitle = "")
        {
            string apiFuncName = "API_ARTICLE_GET";

            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 1;
            if (!string.IsNullOrEmpty(keywordTitle))
                keywordTitle = System.Web.HttpUtility.UrlDecode(keywordTitle);

            OkFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 목록 조회 요청 : BBS_ID={0} / PAGE_SIZE={1} / PAGE_NO={2} / KEYWORD_TITLE={3}", bbsId, pageSize, pageNo, keywordTitle));

            return Common.ConvertFromDataTableToArticleDTOList(await BBSArticle.List_By_Page(bbsId, pageNo, pageSize, keywordTitle));
        }

        [HttpGet("read/{bbsId}/{articleId}/{replyId}")]
        public async Task<ArticleDTO?> Get_Read(string bbsId, string articleId, int replyId)
        {
            string apiFuncName = "API_ARTICLE_GET_READ";

            OkFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 조회 요청 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}", bbsId, articleId, replyId));

            ArticleDTO? article = await BBSArticle.Read_Article(
                                                        bbsId, 
                                                        articleId, 
                                                        replyId, 
                                                        true);
            if (article != null)
            {
                string fileName = string.Format("{0}_{1}_{2}.zip", 
                                            article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID), 
                                            article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID), 
                                            article.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID));

                if (System.IO.File.Exists(Path.Combine(PATH_ATTACHED_FILE_FOLDER, fileName)))
                {
                    try
                    {
                        using (ZipArchive zip = ZipFile.OpenRead(Path.Combine(PATH_ATTACHED_FILE_FOLDER, fileName)))
                        {
                            article.SetValue(Constants.PROPERTY_KEY_NAME_ATTACHED_FILES_COUNT, zip.Entries.Count);

                            if (zip.Entries.Count > 0)
                            {
                                StringBuilder sb = new StringBuilder();

                                for (int i = 0; i < zip.Entries.Count; i++)
                                {
                                    sb.Append(zip.Entries[i].Name);
                                    if (!(i >= zip.Entries.Count - 1))
                                        sb.Append(Constants.DEFAULT_VALUE_SEPARATOR);
                                }

                                article.SetValue(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME, sb.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProblemFormattedDetailMessage(
                            apiFuncName,
                            string.Format("게시물 조회 첨부파일 패키지 읽기 오류 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                    article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                    article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                    article.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                            ex);
                    }
                }   
                else
                    article.SetValue(Constants.PROPERTY_KEY_NAME_ATTACHED_FILES_COUNT, 0);

                OkFormattedDetailMessage(
                                apiFuncName,
                                string.Format("게시물 조회 성공 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}", bbsId, articleId, replyId));
            }

            return article;
        }

        [HttpGet("create_test/{bbsId}/{maxCount}")]
        public async Task Get_GenerateTestData(string bbsId, int maxCount = 10)
        {
            string apiFuncName = "API_ARTICLE_GET_GENERATE_TEST_ARTICLES";

            if (string.IsNullOrEmpty(bbsId))
            {
                ProblemFormattedDetailMessage(apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL, null);
                return;
            }

            if (maxCount < 0) maxCount = 10;
            int okCount = 0;
            Random rnd = new Random(1);
            
            for (int i = 1; i < (maxCount + 1); i++)
            {
                string body = "&lt;Section xmlns=&quot;http://schemas.microsoft.com/winfx/2006/xaml/presentation&quot; xml:space=&quot;preserve&quot; TextAlignment=&quot;Left&quot; LineHeight=&quot;Auto&quot; IsHyphenationEnabled=&quot;False&quot; xml:lang=&quot;en-us&quot; FlowDirection=&quot;LeftToRight&quot; NumberSubstitution.CultureSource=&quot;User&quot; NumberSubstitution.Substitution=&quot;AsCulture&quot; FontFamily=&quot;맑은 고딕&quot; FontStyle=&quot;Normal&quot; FontWeight=&quot;Normal&quot; FontStretch=&quot;Normal&quot; FontSize=&quot;12&quot; Foreground=&quot;#FF000000&quot; Typography.StandardLigatures=&quot;True&quot; Typography.ContextualLigatures=&quot;True&quot; Typography.DiscretionaryLigatures=&quot;False&quot; Typography.HistoricalLigatures=&quot;False&quot; Typography.AnnotationAlternates=&quot;0&quot; Typography.ContextualAlternates=&quot;True&quot; Typography.HistoricalForms=&quot;False&quot; Typography.Kerning=&quot;True&quot; Typography.CapitalSpacing=&quot;False&quot; Typography.CaseSensitiveForms=&quot;False&quot; Typography.StylisticSet1=&quot;False&quot; Typography.StylisticSet2=&quot;False&quot; Typography.StylisticSet3=&quot;False&quot; Typography.StylisticSet4=&quot;False&quot; Typography.StylisticSet5=&quot;False&quot; Typography.StylisticSet6=&quot;False&quot; Typography.StylisticSet7=&quot;False&quot; Typography.StylisticSet8=&quot;False&quot; Typography.StylisticSet9=&quot;False&quot; Typography.StylisticSet10=&quot;False&quot; Typography.StylisticSet11=&quot;False&quot; Typography.StylisticSet12=&quot;False&quot; Typography.StylisticSet13=&quot;False&quot; Typography.StylisticSet14=&quot;False&quot; Typography.StylisticSet15=&quot;False&quot; Typography.StylisticSet16=&quot;False&quot; Typography.StylisticSet17=&quot;False&quot; Typography.StylisticSet18=&quot;False&quot; Typography.StylisticSet19=&quot;False&quot; Typography.StylisticSet20=&quot;False&quot; Typography.Fraction=&quot;Normal&quot; Typography.SlashedZero=&quot;False&quot; Typography.MathematicalGreek=&quot;False&quot; Typography.EastAsianExpertForms=&quot;False&quot; Typography.Variants=&quot;Normal&quot; Typography.Capitals=&quot;Normal&quot; Typography.NumeralStyle=&quot;Normal&quot; Typography.NumeralAlignment=&quot;Normal&quot; Typography.EastAsianWidths=&quot;Normal&quot; Typography.EastAsianLanguage=&quot;Normal&quot; Typography.StandardSwashes=&quot;0&quot; Typography.ContextualSwashes=&quot;0&quot; Typography.StylisticAlternates=&quot;0&quot;&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Arial&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Arial&quot; FontStyle=&quot;Italic&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Arial&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;핵심&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;영역&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;다음&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;핵심&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;영역에&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;걸쳐&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;광범위한&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;제공합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;Infrastructure &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;그리드&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Oracle&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Infrastructure &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;그리드&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기술을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하면&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;저비용&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;서버&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;저장&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;영역을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;도입하여&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;효율성&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고가용성&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;성능&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;측면에서&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;최고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;품질의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;서비스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;제공&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;하는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;시스템을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;구축할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;그리드&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;컴퓨팅의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;이점을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;통합&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;확장합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;그리드&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;컴퓨팅을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;완전히&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;이용하는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;것과&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;별도로&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;고유한&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;변경&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;보증&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;통해&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;통제되고&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;비용&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;효율적인&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;방법으로&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;변경&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;내용을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;관리합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기존의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;컨텐트&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;통합&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;주기&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;영역으로&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;확장합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;. Oracle&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;XML(Extensible Markup Language),&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;텍스트&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;공간&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;멀티미디어&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;의료&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;이미지&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;시맨틱&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기술과&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;같은&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고급&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;유형의&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;컨텐트를&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;관리합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;응용&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;프로그램&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;개발&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;PL/SQL, Java/JDBC, .NET&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;과&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;Windows, PHP,&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;SQL Developer &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Application Express&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;와&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;같은&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;모든&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;주요&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;응용&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;프로그램&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;개발&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;환경을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하고&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;관리할&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontStyle=&quot;Italic&quot; FontWeight=&quot;Bold&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontStyle=&quot;Normal&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;조직에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;업무&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;응용&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;프로그램에&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;대한&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;신속하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;안전한&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;액세스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;끊임없이&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;요구하는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;유저를&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;위해&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;테라바이트&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;단위의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;지원해야&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터베이스&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;시스템은&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;신뢰할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있어야&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;어떤&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;형태의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;failure&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;에서도&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;신속하게&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;복구할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있어야&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;조직에서&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Infrastructure &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;그리드를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;쉽게&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고품질의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;서비스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;제공할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있도록&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;다음&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;영역에&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;중점을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;두고&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;설계되었습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;효율성&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;변경&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;보증&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;자동화&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;오류&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;진단&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;중&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;몇&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;가지를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하여&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;DBA(&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;베이스&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리자&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;)&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;가&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;생산성을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;향상하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;비용을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;줄이고&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;오류를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;최소화하며&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;서비스&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;품질을&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;최대화할&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;효율성을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;높일&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;있는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;몇&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;가지&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;유용한&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;기능으로는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;Database&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;Replay &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;기능&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;, SQL Performance Analyzer &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;Automatic SQL Tuning &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;기능이&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고가용성&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고가용성&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하여&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;다운타임&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;손실을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;줄일&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;이러한&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;기능으로&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;온라인&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;작업을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;개선하고&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;데이터베이스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;더&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;빠르게&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;업그레이드할&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontWeight=&quot;Bold&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;(&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontWeight=&quot;Normal&quot;&gt;&lt;Run&gt;계속&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;)&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;성능&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;SecureFiles, OLTP(Online Transaction Processing) &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;압축&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;, RAC(Real Application Clusters)&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;최적화&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Result Cache &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;등의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하여&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터베이스의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;성능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;크게&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;개선할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;조직에서는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;빠른&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;액세스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;제공하며&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;확장&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;가능한&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;대형&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;트랜잭션&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;웨어하우징&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;시스템을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;저비용&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;모듈식&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;저장&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;영역을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하여&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;관리할&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;보안&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;조직에서&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고유&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;보안&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;구성&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;, &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;암호화&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;(encryption)&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;와&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;마스킹&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;및&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정교한&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;감사&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;(auditing) &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능으로&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;보호할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있도록&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;모든&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;유형의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보에&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;대한&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;빠르고&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;신뢰할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;액세스가&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;가능하도록&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;산업&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;표준&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;인터페이스를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;사용하여&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;안전하고&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;확장성&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;있는&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;플랫폼을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot;&gt;&lt;Run&gt;제공합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;16.666666666666664&quot;&gt;&lt;Run&gt;• &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;통합&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontWeight=&quot;Bold&quot;&gt;&lt;Run&gt;: &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;Oracle Database 11&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontStyle=&quot;Italic&quot;&gt;&lt;Run&gt;g&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기업&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;전체에서&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터를&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;더&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;잘&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;통합할&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;있도록&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;하는&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;많은&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span&gt;&lt;Run&gt;기능을&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;제공하며&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;고급&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;정보&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;수명&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;주기&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;관리&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;기능도&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;지원합니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot;&gt;&lt;Run&gt;. &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;따라서&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이터베이스의&lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span&gt;&lt;Run&gt;데이&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;터&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;변경을&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;쉽게&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;관리할&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;수&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt; &lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Batang&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;있습니다&lt;/Run&gt;&lt;/Span&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;.&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;Paragraph FontFamily=&quot;Arial&quot; FontSize=&quot;13.333333333333332&quot; Margin=&quot;0,0,0,0&quot;&gt;&lt;Span FontFamily=&quot;Times New Roman&quot; FontSize=&quot;15.333333333333332&quot;&gt;&lt;Run&gt;&lt;/Run&gt;&lt;/Span&gt;&lt;/Paragraph&gt;&lt;/Section&gt;";

                ArticleDTO dto = new ArticleDTO();
                dto.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, bbsId);
                dto.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_TYPE, Constants.BBSArticleType.General);
                dto.SetValue(Constants.PROPERTY_KEY_NAME_TITLE, "테스트 게시물 #" + i.ToString());
                dto.SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, body);
                dto.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD, "1234");
                dto.SetValue(Constants.PROPERTY_KEY_NAME_WRITER, "SYSTEM");

                var result = await BBSArticle.Write_Article(
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                                DateTime.Now,
                                                Constants.BBSArticleType.General,
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_TITLE),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_WRITER),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD),
                                                rnd);

                if (result.Item1 > 0)
                {
                    okCount++;
                }

                await Task.Delay(500);
            }

            OkFormattedDetailMessage(
                                apiFuncName,
                                string.Format("테스트 게시물 데이터 생성 완료 : BBS_ID={0} / REQUESTED_COUNT={1} / OK_COUNT={2}", bbsId, maxCount, okCount));
        }

        [HttpPost]
        public async Task Post([FromBody] ArticleDTO dto)
        {
            string apiFuncName = "API_ARTICLE_POST_NEW";

            if (dto == null)
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시물 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            try
            {
                var result = await BBSArticle.Write_Article(
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                                DateTime.Now,
                                                Constants.BBSArticleType.General,
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_TITLE),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_WRITER),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD), 
                                                new Random());

                if (result.Item1 > 0)
                {
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, result.Item2);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_ID, result.Item3);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_REPLY_ID, result.Item4);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, string.Empty);

                    await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(dto.ToString()));

                    OkFormattedDetailMessage(
                        apiFuncName, 
                        string.Format("게시물 추가 성공 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}", 
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID), 
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID), 
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)));
                }
                else
                {
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 추가 실패 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)), 
                        null);
                }
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 추가 오류 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                        ex);
            }
        }

        [HttpPost("reply/{bbsId}/{articleId}")]
        public async Task Post(string bbsId, string articleId, [FromBody] ArticleDTO dto)
        {
            string apiFuncName = "API_ARTICLE_POST_REPLY";
            
            if (string.IsNullOrEmpty(bbsId) || string.IsNullOrEmpty(articleId) || (dto == null))
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시물 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            try
            {
                var result = await BBSArticle.Reply_Article(
                                                bbsId,
                                                articleId,
                                                DateTime.Now,
                                                Constants.BBSArticleType.General,
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_TITLE),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_WRITER),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD));

                if (result.Item1 > 0)
                {
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, result.Item2);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_ID, result.Item3);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_REPLY_ID, result.Item4);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, string.Empty);
                    await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(dto.ToString()));

                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 리플라이 추가 성공 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)));
                }
                else
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 리플라이 추가 실패 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                        null);

            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 리플라이 추가 오류 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                        ex);
            }
        }

        // PUT api/<BoardController>/5
        [HttpPut()]
        public async Task Put([FromBody] ArticleDTO dto)
        {
            string apiFuncName = "API_ARTICLE_PUT";

            if (dto == null)
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시물 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            string bbsId = dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);
            string articleId = dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID);
            int replyId = dto.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID);

            if (string.IsNullOrEmpty(bbsId) || string.IsNullOrEmpty(articleId))
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시물 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            bool isDeleted = (dto.GetString(Constants.PROPERTY_KEY_NAME_IS_DELETED) == "1");

            try
            {
                var result = await BBSArticle.Update_Article(
                                                bbsId,
                                                articleId,
                                                replyId,
                                                (Constants.BBSArticleType)dto.GetInt32(Constants.PROPERTY_KEY_NAME_ARTICLE_TYPE),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_TITLE),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD),
                                                isDeleted);

                if (result.Item1 > 0)
                {
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, result.Item2);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_ID, result.Item3);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_REPLY_ID, result.Item4);
                    dto.SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, string.Empty);

                    await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(dto.ToString()));

                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 업데이트 성공 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)));
                }
                else
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 업데이트 실패 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                        null);
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 업데이트 오류 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                dto.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID)),
                        ex);
            }
        }

        [HttpDelete("{bbsId}/{articleId}/{replyId}")]
        public async Task Delete(string bbsId, string articleId, int replyId)
        {
            string apiFuncName = "API_ARTICLE_DELETE";

            try
            {
                int result = await BBSArticle.Delete_Article(
                                                bbsId, 
                                                articleId, 
                                                replyId);

                if (result > 0)
                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 삭제 성공 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                bbsId,
                                articleId,
                                replyId));
                else
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 삭제 실패 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                bbsId,
                                articleId,
                                replyId),
                        null);
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시물 삭제 오류 : BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                bbsId,
                                articleId,
                                replyId),
                        ex);
            }
        }

        #region [첨부파일]
        [HttpGet(Constants.API_ARTICLE_SUBPATH_DOWNLOAD_ENTRY_FILE + "/{bbsId}/{articleId}/{replyId}/{fileName}")]
        public async Task<Stream?> Get_EntryFile(string bbsId, string articleId, int replyId, string fileName)
        {
            string apiFuncName = "API_ARTICLE_GET_FILE_ENTRY";
            string pkgFileName = string.Format("{0}_{1}_{2}.zip", bbsId, articleId, replyId);
            string entrySize = "-1";
            FileInfo fiPackage = new FileInfo(Path.Combine(PATH_ATTACHED_FILE_FOLDER, pkgFileName));

            if ((!System.IO.File.Exists(fiPackage.FullName)) || (!fiPackage.Exists))
            {
                ProblemFormattedDetailMessage(apiFuncName, "서버에 패키지 파일이 존재하지 않습니다.", null);
                return null;
            }

            fileName = HttpUtility.UrlDecode(fileName);

            using (FileStream fs = new FileStream(fiPackage.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (ZipArchive archive = new ZipArchive(fs))
                    entrySize = archive.Entries.SingleOrDefault(e => e.Name == fileName)?.Length.ToString() ?? "0";
            }

            Response.Headers.Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME, new Microsoft.Extensions.Primitives.StringValues(HttpUtility.UrlEncode(fileName)));
            Response.Headers.Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_SIZE, new Microsoft.Extensions.Primitives.StringValues(entrySize));

            if (fiPackage.Directory != null)
                return await FileHelper.GetEntryFromZipArchive(fiPackage.FullName, new DirectoryInfo(fiPackage.Directory.FullName + "\\_Entry_Temp"), fileName);
            else
                return null;
        }

        [HttpGet(Constants.API_ARTICLE_SUBPATH_DOWNLOAD_PACKAGE_FILE + "/{bbsId}/{articleId}/{replyId}")]
        public async Task<Stream?> Get_PackageFile(string bbsId, string articleId, int replyId)
        {
            string apiFuncName = "API_ARTICLE_GET_FILE_PACKAGE";
            string fileName = string.Format("{0}_{1}_{2}.zip", bbsId, articleId, replyId);
            FileInfo fi = new FileInfo(Path.Combine(PATH_ATTACHED_FILE_FOLDER, fileName));

            if ((!System.IO.File.Exists(fi.FullName)) || (!fi.Exists))
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시물 컨텐츠가 제공되지 않았습니다.", null);
                return null;
            }

            Response.Headers.Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME, new Microsoft.Extensions.Primitives.StringValues(HttpUtility.UrlEncode(fileName)));
            Response.Headers.Add(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_SIZE, new Microsoft.Extensions.Primitives.StringValues(fi.Length.ToString()));

            Stream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
            Common.PrintDebugInfo(string.Format("[{5}] 게시물 첨부파일 패키지 읽기 성공 : FILE_NAME={3} / FILE_SIZE = {4} bytes / BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                            bbsId,
                                            articleId,
                                            replyId,
                                            fi.Name,
                                            fi.Length.ToString("N0"),
                                            apiFuncName));
            await Task.Delay(10);

            return fs;
        }

        [HttpPost(Constants.API_ARTICLE_SUBPATH_UPLOAD_QUEUE)]
        public async Task<bool> Post_Upload_Queue([FromBody] UploadRequest request)
        {
            string apiFuncName = "API_ARTICLE_POST_UPLOAD_QUEUE";

            if ((request == null) || (string.IsNullOrEmpty(request.TransferID)))
            {
                ProblemFormattedDetailMessage(apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL, new ArgumentNullException());
                return false;
            }

            bool result = false;
            string tid = request.TransferID;

            if (Uploads.Any(u => u.Key.Equals(tid)))
                return true;

            UploadTask task = new UploadTask(request.TransferID, request, new Action<object?>(DoPrepareToUploadTask));

            result = Uploads.TryAdd(tid, task);

            if (result)
            {
                await task.Prepare();
                await Task.Delay(10);
                Common.PrintDebugInfo(string.Format("[{0}] 업로드 큐 등록 완료 : TRANS_ID={1}", apiFuncName, tid));
            }

            return result;
        }

        [HttpPost(Constants.API_ARTICLE_SUBPATH_UPLOAD_DEQUEUE)]
        public async Task<bool> Post_Upload_Dequeue([FromBody] UploadRequest request)
        {
            string apiFuncName = "API_ARTICLE_POST_UPLOAD_DEQUEUE";

            if ((string.IsNullOrEmpty(request.UserState)) || (request.UserState.Split(Constants.SEPARATOR_COMMA).Length != 3))
            {
                ProblemFormattedDetailMessage(apiFuncName,
                                              string.Format("[{0}] {1}", request.TransferID, Constants.MESSAGE_ERROR_ARGUMENTS_NULL),
                                              new ArgumentNullException());
                return false;
            }

            string transID = request.TransferID;
            string bbsID = request.UserState.Split(Constants.SEPARATOR_COMMA)[0];
            string articleID = request.UserState.Split(Constants.SEPARATOR_COMMA)[1];
            string replyID = request.UserState.Split(Constants.SEPARATOR_COMMA)[2];
            string pathFolderTemp = Path.Combine(PATH_ATTACHED_FILE_FOLDER, transID);
            DirectoryInfo folderUploading = (Directory.Exists(pathFolderTemp)) ?
                                                new DirectoryInfo(pathFolderTemp) :
                                                Directory.CreateDirectory(pathFolderTemp);
            FileInfo fileZip = new FileInfo(Path.Combine(
                                                    PATH_ATTACHED_FILE_FOLDER, 
                                                    string.Format("{0}_{1}_{2}.zip", bbsID, articleID, replyID)));

            if (System.IO.File.Exists(fileZip.FullName))
                System.IO.File.Delete(fileZip.FullName);

            ZipFile.CreateFromDirectory(
                            folderUploading.FullName,
                            fileZip.FullName,
                            CompressionLevel.SmallestSize,
                            false,
                            Encoding.UTF8);

            Directory.Delete(folderUploading.FullName, true);
            await Task.Delay(250);

            if (System.IO.File.Exists(fileZip.FullName))
            {
                if (Uploads.ContainsKey(transID))
                {
                    UploadTask? ut;
                    if (Uploads.TryRemove(transID, out ut))
                        Common.PrintDebugInfo(string.Format("[{0}] 업로드 큐 삭제 완료 : TRANS_ID={1}", apiFuncName, transID));
                    else
                        Common.PrintDebugInfo(string.Format("[{0}] 업로드 큐 삭제 실패 : TRANS_ID={1}", apiFuncName, transID));
                }

                OkFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 첨부파일 패키지 쓰기 성공 : PID={0} / ZIP_FILE={1} / BBS_ID={2} / ARTICLE_ID={3} / REPLY_ID={4}",
                            transID,
                            fileZip.FullName,
                            bbsID,
                            articleID,
                            replyID));

                return true;
            }
            else
            {
                ProblemFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 첨부파일 패키지 쓰기 실패 : PID={0} / ZIP_FILE={1} / BBS_ID={2} / ARTICLE_ID={3} / REPLY_ID={4}",
                            transID,
                            fileZip.FullName,
                            bbsID,
                            articleID,
                            replyID),
                    null);

                return false;
            }
        }

        [HttpPost(Constants.API_ARTICLE_SUBPATH_UPLOAD_FILE + "/{bbsId}/{articleId}/{replyId}")]
        public async Task<long> Post_Upload_File(string bbsId, string articleId, int replyId, [FromBody] FilePacket fileItem)
        {
            string apiFuncName = "API_ARTICLE_POST_UPLOAD_FILE";

            if ((fileItem == null) ||
                (fileItem.IsPendingDelete) ||
                (string.IsNullOrEmpty(fileItem.FileSliceAsBase64String)))
                return 0;

            bool result = false;
            string pathFolderTemp = Path.Combine(PATH_ATTACHED_FILE_FOLDER, fileItem.PackageID);
            byte[] buffer = new byte[BUFFER_SIZE];
            int readBytes = 0;
            long readTotalBytes = 0;
            DirectoryInfo folderUploading = (Directory.Exists(pathFolderTemp)) ?
                                                new DirectoryInfo(pathFolderTemp) :
                                                Directory.CreateDirectory(pathFolderTemp);

            using (FileStream fs = new FileStream(Path.Combine(folderUploading.FullName, fileItem.UserState), FileMode.Append))
            {
                using (MemoryStream ms = new MemoryStream(fileItem.ReadFileSlice()))
                {
                    try
                    {
                        ms.Position = 0;
                        while ((readBytes = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fs.WriteAsync(buffer, 0, readBytes);
                            await fs.FlushAsync();

                            readTotalBytes += readBytes;
                            Common.PrintDebugInfo(string.Format("[{8}] 게시물 첨부파일 슬라이스 버퍼 쓰기 성공 : PID={3} / FILE_ID={4} / FILE_NAME={5} / WRITE = {6}  / TOTAL_WRITE = {7} bytes / BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                                            bbsId,
                                                            articleId,
                                                            replyId,
                                                            fileItem.PackageID,
                                                            fileItem.FileID,
                                                            fileItem.FileName,
                                                            readBytes,
                                                            readTotalBytes,
                                                            apiFuncName));
                            readBytes = 0;

                            await Task.Delay(1);
                        }

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        ProblemFormattedDetailMessage(
                            apiFuncName,
                            string.Format("게시물 첨부파일 슬라이스 버퍼 쓰기 오류 : PID={3} / FILE_ID={4} / FILE_NAME={5} / WRITE = {6}  / TOTAL_WRITE = {7} bytes / BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                                    bbsId,
                                    articleId,
                                    replyId,
                                    fileItem.PackageID,
                                    fileItem.FileID,
                                    fileItem.FileName,
                                    readBytes,
                                    readTotalBytes),
                            ex);
                    }
                    finally
                    {
                        ms.Close();
                        fs.Close();
                    }
                }
            }

            if (!result)
            {
                folderUploading.Delete(true);

                ProblemFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 첨부파일 슬라이스 쓰기 실패 : PID={3} / FILE_ID={4} / BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                            bbsId,
                            articleId,
                            replyId,
                            fileItem.PackageID,
                            fileItem.FileID),
                    null);

                return -1;
            }

            if (!((fileItem.IsLastFileInPackage) && (fileItem.IsEOF)))
            {
                OkFormattedDetailMessage(
                    apiFuncName,
                    string.Format("게시물 첨부파일 슬라이스 쓰기 성공 : PID={3} / FILE_ID={4} / BBS_ID={0} / ARTICLE_ID={1} / REPLY_ID={2}",
                            bbsId,
                            articleId,
                            replyId,
                            fileItem.PackageID,
                            fileItem.FileID));
            }

            return readTotalBytes;
        }
        #endregion
    }
}
