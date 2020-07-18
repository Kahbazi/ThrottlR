using System;
using Microsoft.Extensions.DependencyInjection;

namespace ThrottlR
{
    /// <summary>
    /// Reverse Proxy builder for DI configuration.
    /// </summary>
    public class ThrottlerBuilder : IThrottlerBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottlerBuilder"/> class.
        /// </summary>
        /// <param name="services">Services collection.</param>
        public ThrottlerBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the services collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
