using Microsoft.Extensions.DependencyInjection;

namespace ThrottlR.DependencyInjection
{
    /// <summary>
    /// Reverse Proxy builder interface.
    /// </summary>
    public interface IThrottlerBuilder
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
