using Microsoft.AspNetCore.Mvc;
using ThrottlR;

namespace MVC
{
    // Throttle this controller with default policy
    [EnableThrottle]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet("values")]
        public string[] Index()
        {
            return new string[] { "value1", "value2" };
        }

        // Override General Rule for this action with 2 requests per second
        [Throttle(PerSecond = 2)]
        [HttpGet("custom")]
        public string[] CustomRule()
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
