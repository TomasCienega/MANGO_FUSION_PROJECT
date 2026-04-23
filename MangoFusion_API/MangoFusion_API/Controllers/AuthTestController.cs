using MangoFusion_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangoFusion_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public ActionResult<string> GetSomething()
        {
            return "You are Authorized User";
        }

        [HttpGet("{someValue:int}")]
        [Authorize(Roles = StaticDetails.Role_Admin)]
        public ActionResult<string> GetSomething(int someValue)
        {
            return "You are Authorized User, with Role of Admin";
        }
    }
}
