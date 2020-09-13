using System;
using System.Text;

namespace ThrottlR
{
    public struct ThrottlerItem
    {
        public ThrottlerItem(ThrottleRule rule, string policyName, string scope, string endpointName)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            PolicyName = policyName;
            Scope = scope;
            EndpointName = endpointName;
        }

        public ThrottleRule Rule { get; }

        public string PolicyName { get; }

        public string Scope { get; }

        public string EndpointName { get; }

        public string GenerateCounterKey(string prefix = "")
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(prefix))
            {
                builder.Append(prefix);
                builder.Append(':');
            }

            builder.Append(EndpointName);
            builder.Append(':');

            builder.Append(PolicyName);
            builder.Append(':');

            builder.Append(Rule.TimeWindow);
            builder.Append(':');

            builder.Append(Scope);
            builder.Append(':');

            return builder.ToString();
        }
    }
}
