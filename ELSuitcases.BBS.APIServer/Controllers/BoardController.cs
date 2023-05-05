using Microsoft.AspNetCore.Mvc;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.Library.Server;
using System.Text;

namespace ELSuitcases.BBS.APIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : BBSControllerBase
    {
        private readonly ILogger<BoardController> _logger;



        public BoardController(ILogger<BoardController> logger)
        {
            _logger = logger;
        }



        [HttpGet]
        public async Task<List<BoardDTO>> Get()
        {
            string apiFuncName = "API_BOARD_GET";
            OkFormattedDetailMessage(apiFuncName, "게시판 목록 조회 요청 : 전체"); 
            
            List<BoardDTO> result = new List<BoardDTO>();

            return Common.ConvertFromDataTableToBoardDTOList(await BBSBoard.List());
        }
        
        [HttpGet("{bbsId}")]
        public async Task<BoardDTO?> Get(string bbsId)
        {
            string apiFuncName = "API_BOARD_GET";
            OkFormattedDetailMessage(apiFuncName, "게시판 목록 조회 요청 : BBS_ID = " + bbsId);

            return Common.ConvertFromDataTableToBoardDTOList(await BBSBoard.List()).SingleOrDefault(e => (e.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID) == bbsId));
        }

        [HttpPost]
        public async Task Post([FromBody] BoardDTO? dto)
        {
            string apiFuncName = "API_BOARD_POST";

            if (dto == null)
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시판 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            string bbsId = string.Empty;

            try
            {
                bbsId = dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);
                int? result = await BBSBoard.Create_Board(
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_NAME),
                                                (Constants.BBSBoardType)dto.GetInt32(Constants.PROPERTY_KEY_NAME_BBS_TYPE));

                if (result == 1)
                {
                    await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(dto.ToString()));

                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 추가 성공 : BBS_ID={0}", bbsId));
                }
                else
                {
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 추가 실패 : BBS_ID={0}", bbsId),
                        null);
                }
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 추가 오류 : BBS_ID={0}", bbsId),
                        ex);
            }
        }

        [HttpPut]
        public async Task Put([FromBody] BoardDTO? dto)
        {
            string apiFuncName = "API_BOARD_PUT";
            
            if (dto == null)
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시판 컨텐츠가 제공되지 않았습니다.", null);
                return;
            }

            string bbsId = dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);

            if (string.IsNullOrEmpty(bbsId))
            {
                ProblemFormattedDetailMessage(apiFuncName, "게시판 컨텐츠 (게시판 ID)가 제공되지 않았습니다.", null);
                return;
            }

            try
            {
                bool isEnabled = (dto.GetString(Constants.PROPERTY_KEY_NAME_IS_ENABLED) == "1");
                int? result = await BBSBoard.Update_Board(
                                                bbsId,
                                                dto.GetString(Constants.PROPERTY_KEY_NAME_BBS_NAME),
                                                (Constants.BBSBoardType)dto.GetInt32(Constants.PROPERTY_KEY_NAME_BBS_TYPE),
                                                isEnabled);

                if (result == 1)
                {
                    await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(dto.ToString()));

                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 업데이트 성공 : BBS_ID={0}", bbsId));
                }
                else
                {
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 업데이트 실패 : BBS_ID={0}", bbsId),
                        null);
                }
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 업데이트 오류 : BBS_ID={0}", bbsId),
                        ex);
            }
        }

        [HttpDelete("{bbsId}")]
        public async Task Delete(string bbsId)
        {
            string apiFuncName = "API_BOARD_DELETE";

            try
            {
                int result = await BBSBoard.Delete_Board(bbsId);

                if (result == 1)
                {
                    OkFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 삭제 성공 : BBS_ID={0}", bbsId));
                }
                else
                {
                    ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 삭제 실패 : BBS_ID={0}", bbsId),
                        null);
                }   
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(
                        apiFuncName,
                        string.Format("게시판 삭제 오류 : BBS_ID={0}", bbsId),
                        ex);
            }
        }
    }
}
