using System.Threading.RateLimiting;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Playground
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PartitionedOptions _options;

        public RateLimitMiddleware(RequestDelegate next, IOptions<PartitionedOptions> options)
        {
            _options = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            if (endpoint.Metadata.GetMetadata<IDisableRateLimitMetadata>() is not null)
            {
                await _next(context);
                return;
            }

            var policies = endpoint.Metadata.GetOrderedMetadata<IRateLimitPolicy>();
            var IsAcquired = true;
            foreach (var policy in policies)
            {
                if (policy is IRateLimitPolicy rateLimitPolicy)
                {
                    using var lease = await _options.Policies[rateLimitPolicy.PolicyName].WaitAsync(context);
                    if (!lease.IsAcquired)
                    {
                        IsAcquired = false;
                        break;
                    }
                }
                else if (policy is IRateLimitInline rateLimitInline)
                {
                    using var lease = await rateLimitInline.RateLimiter.WaitAsync(context);
                    if (!lease.IsAcquired)
                    {
                        IsAcquired = false;
                        break;
                    }
                }
            }

            if (IsAcquired)
            {
                await _next(context);
                return;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                //OnRejected(...)
            }
        }
    }
}