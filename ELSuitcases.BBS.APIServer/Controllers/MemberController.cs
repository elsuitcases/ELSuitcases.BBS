using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELSuitcases.BBS.APIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : BBSControllerBase
    {
        [HttpGet("login/{userId}/{userPassword}")]
        public async Task<bool> Get(string userId, string userPassword)
        {
            if ((string.IsNullOrEmpty(userId)) || (string.IsNullOrEmpty(userPassword)))
                return false;

            bool result = false;

            await Task.Delay(777);

            result = true;

            return result;
        }
    }
}
