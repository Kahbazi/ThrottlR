using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ThrottlR
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="policy"></param>
    /// <param name="rule"></param>
    /// <param name="retryAfter"></param>
    /// <returns></returns>
    public delegate Task QuotaExceededDelegate(HttpContext context, ThrottleRule rule, DateTime retryAfter);
}
