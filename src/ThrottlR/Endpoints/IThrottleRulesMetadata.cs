using System.Collections.Generic;

namespace ThrottlR
{
    public interface IThrottleRulesMetadata : IThrottleMetadata
    {
        IReadOnlyList<ThrottleRule> GeneralRules { get; }
    }
}
