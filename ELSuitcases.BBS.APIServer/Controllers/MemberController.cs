using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.Library.Server;
using System.Text;

namespace ELSuitcases.BBS.APIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : BBSControllerBase
    {
        private readonly ILogger<MemberController> _logger;



        public MemberController(ILogger<MemberController> logger)
        {
            _logger = logger;
        }
        


        [HttpGet("login")]
        public async Task<UserDTO?> Login()
        {
            string apiFuncName = "API_MEMBER_LOGIN";
            string userID = Request.Headers[Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID].ToString() ?? string.Empty;
            string userPW = Request.Headers[Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_PW].ToString() ?? string.Empty;
            UserDTO? dtoUser = null;

            try
            {
                userPW = Encoding.UTF8.GetString(await Common.AESDecrypt(
                                                                Convert.FromBase64String(userPW), 
                                                                Encoding.UTF8.GetBytes(Constants.ENCRYPT_KEY), 
                                                                Encoding.UTF8.GetBytes(Constants.ENCRYPT_IV)));
                
                using (var dt = await BBSMember.GetByAccountIDAndPassword(userID, userPW))
                {
                    await Task.Delay(1);

                    if ((dt != null) && (dt.Rows.Count > 0))
                    {
                        dtoUser = new UserDTO(Convert.ToString(dt.Rows[0]["ACCOUNT_ID"]), Convert.ToString(dt.Rows[0]["FULLNAME"]));
                        dtoUser.SetValue("MEMBER_ID", Convert.ToString(dt.Rows[0]["MEMBER_ID"]));
                        dtoUser.SetValue(Constants.PROPERTY_KEY_NAME_CURRENT_USER_EMAIL, Convert.ToString(dt.Rows[0]["EMAIL"]));
                        dtoUser.SetValue("IS_ENABLED", Convert.ToString(dt.Rows[0]["IS_ENABLED"]));
                        dtoUser.SetValue("CREATED_TIME", Convert.ToDateTime(dt.Rows[0]["CREATED_TIME"]));

                        OkFormattedDetailMessage(apiFuncName, "사용자 로그인 요청 : " + dtoUser.GetString(Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID));
                    }
                }
            }
            catch (Exception ex)
            {
                ProblemFormattedDetailMessage(apiFuncName, "사용자 로그인 요청 오류", ex);
            }

            return dtoUser;
        }
    }
}
