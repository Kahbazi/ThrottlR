using System.Collections.Generic;
using ThrottlR.Policy;

namespace ThrottlR.Endpoints
{
    public interface IThrottleRulesMetadata : IThrottleMetadata
    {
        IReadOnlyList<ThrottleRule> GeneralRules { get; }
    }
}
