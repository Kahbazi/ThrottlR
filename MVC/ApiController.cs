using Microsoft.AspNetCore.Mvc;
using ThrottlR;

namespace MVC
{
    [Throttle]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet("")]
        public string[] Index()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
