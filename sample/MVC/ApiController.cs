using Microsoft.AspNetCore.Mvc;
using ThrottlR;

namespace MVC
{
    // Throttle this controller with default policy
    [Throttle] 
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet("values")]
        public string[] Index()
        {
            return new string[] { "value1", "value2" };
        }

        // Disable throttle for this action
        [DisableThrottle] 
        [HttpGet("greetings")]
        public string Greetings()
        {
            return "Salutation";
        }
    }
}
