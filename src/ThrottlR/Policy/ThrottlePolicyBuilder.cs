using System;
using System.Collections.Generic;

namespace ThrottlR
{
    /// <summary>
    /// Exposes methods to build a policy.
    /// </summary>
    public class ThrottlePolicyBuilder
    {
        private readonly ThrottlePolicy _policy;


        public ThrottlePolicyBuilder()
        {
            _policy = new ThrottlePolicy();

            SafeList = new SafeListBuilder(this, _policy.SafeList);
        }

        /// <summary>
        /// 
        /// </summary>
        public SafeListBuilder SafeList { get; } 

        /// <summary>
        /// Adds the specified <paramref name="rules"/> to the policy.
        /// </summary>
        /// <param name="rules">The rules which the request has.</param>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder WithGeneralRule(TimeSpan timeWindow, double quota)
        {
            if (quota <= 0)
            {
                throw new ArgumentNullException(nameof(quota));
            }

            _policy.GeneralRules.Add(new ThrottleRule
            {
                TimeWindow = timeWindow,
                Quota = quota
            });

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="rules"/> to the policy.
        /// </summary>
        /// <param name="rules">The rules which the request has.</param>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder WithGeneralRule(params ThrottleRule[] rules)
        {
            _ = rules ?? throw new ArgumentNullException(nameof(rules));

            for (var i = 0; i < rules.Length; i++)
            {
                _policy.GeneralRules.Add(rules[i]);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="rules"/> to the policy.
        /// </summary>
        /// <param name="rules">The rules which the request has.</param>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder WithSpecificRule(string identity, TimeSpan timeWindow, double quota)
        {
            WithSpecificRule(identity, new ThrottleRule { TimeWindow = timeWindow, Quota = quota });

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="rules"/> to the policy.
        /// </summary>
        /// <param name="rules">The rules which the request has.</param>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder WithSpecificRule(string identity, params ThrottleRule[] rules)
        {
            _ = rules ?? throw new ArgumentNullException(nameof(rules));

            if (string.IsNullOrEmpty(identity))
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (!_policy.SpecificRules.TryGetValue(identity, out var ruleList))
            {
                ruleList = new List<ThrottleRule>();
                _policy.SpecificRules.Add(identity, ruleList);
            }

            for (var i = 0; i < rules.Length; i++)
            {
                ruleList.Add(rules[i]);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified <paramref name="methods"/> to the policy.
        /// </summary>
        /// <param name="methods">The methods which need to be added to the policy.</param>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder WithResolver(IResolver resolver)
        {
            _ = resolver ?? throw new ArgumentNullException(nameof(resolver));

            _policy.Resolver = resolver;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The current policy builder.</returns>
        public ThrottlePolicyBuilder ApplyPerEndpoint()
        {
            _policy.ApplyPerEndpoint = true;

            return this;
        }

        /// <summary>
        /// Builds a new <see cref="ThrottlePolicy"/> using the entries added.
        /// </summary>
        /// <returns>The constructed <see cref="ThrottlePolicy"/>.</returns>
        public ThrottlePolicy Build()
        {
            if (_policy.Resolver == null)
            {
                throw new InvalidOperationException("Resolver unspecified.");
            }

            return _policy;
        }
    }
}
