using System;
using System.Text;
using ThrottlR.Policy;

namespace ThrottlR.Models
{
    public readonly struct ThrottlerItem
    {
        public ThrottlerItem(ThrottleRule? rule, string policyName, string scope, string endpointName)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
            _policyName = policyName;
            _scope = scope;
            _endpointName = endpointName;
        }

        private readonly ThrottleRule _rule;

        private readonly string _policyName;

        private readonly string _scope;

        private readonly string _endpointName;

        public string GenerateCounterKey(string prefix = "default")
        {
            var builder = new StringBuilder();

            builder.Append(prefix);

            builder.Append(':');
            builder.Append(_endpointName);

            builder.Append(':');
            builder.Append(_policyName);

            builder.Append(':');
            builder.Append(_rule.TimeWindow);

            builder.Append(_scope);

            return builder.ToString();
        }
    }
}
