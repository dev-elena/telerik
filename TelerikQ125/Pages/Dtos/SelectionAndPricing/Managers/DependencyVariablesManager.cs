using Newtonsoft.Json;



using System;
using System.Collections.Generic;
using System.Linq;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    internal class DependencyVariablesManager
    {
        [JsonProperty("DependencyVariables")]
        public readonly Dictionary<string, object> DependencyVariables = new();

        private readonly Dictionary<string, int> _variableUpdatedBy = new();

        /// <summary>
        /// Get a copy of the currently selected values that are not null.
        /// </summary>
        public Dictionary<string, object> GetNotNullDependencyVariables()
        {
            var notNullValues = DependencyVariables.Where(v => !string.IsNullOrWhiteSpace(v.Value?.ToString()));

            return notNullValues.ToDictionary(nnValue => nnValue.Key, nnValue => nnValue.Value);
        }

        /// <summary>
        /// Add a list of variable names to the dependency variables dictionary.
        /// </summary>
        /// <param name="variableNames">List of variable names to be added to the dictionary.</param>
        /// <remarks>The initial value for each variable added will be null.</remarks>
        public bool AddDependentVariableNames(IEnumerable<string> variableNames)
        {
            return variableNames.Aggregate(true, (current, variable) => current && DependencyVariables.TryAdd(variable, null));
        }

        /// <summary>
        /// Add a value to a dependency variable within the dictionary.
        /// </summary>
        /// <param name="key">Variable name</param>
        /// <param name="value">Variable value</param>
        /// <remarks>If the variable name does not exist in the dictionary, it will get added.</remarks>
        public void AddValueToDependencyVariable(string key, object value, int stepId)
        {
            DependencyVariables[key] = value;
            _variableUpdatedBy[key] = stepId;
        }

        /// <summary>
        /// Get a value from the dependency variables dictionary for a given variable name, must be exact match.
        /// </summary>
        /// <param name="key">Variable name</param>
        public object GetValueFromDependencyVariable(string key)
        {
            return DependencyVariables.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Get a value from the dependency variables dictionary for a given variable name, allowing for string comparison settings.
        /// </summary>
        /// <param name="key">Variable name</param>
        public object GetValueFromDependencyVariable(string key, StringComparison stringComparison)
        {
            return DependencyVariables.FirstOrDefault(k => k.Key.Equals(key, stringComparison));
        }

        public bool CanVariableBeUpdated(string variableName, int stepId)
        {
            if (_variableUpdatedBy.TryGetValue(variableName, out var step))
            {
                // Only return true if the current step id is equal or greater than the step id that set the value previously
                return stepId >= step;
            }

            // Does not exist yet, so true is default
            return true;
        }

        public void ClearVariableSteps()
        {
            _variableUpdatedBy.Clear();
        }

        internal void ApplyPerformanceCurveChanges(List<MotorAirflowLimit> curves)
        {
            if (DependencyVariables.TryGetValue("airflowCurves", out var airflowCurves))
            {
                if (airflowCurves is not List<MotorAirflowLimit> limits) return;
                foreach (var limit in curves)
                {
                    var foundLimit = limits.Find(l => l.Equals(limit));
                    if (foundLimit is null)
                    {
                        limits.Add(limit);
                        continue;
                    }

                    // Update the limits if needed
                    if (!foundLimit.StaticLimit.Equals(limit.StaticLimit)) foundLimit.StaticLimit = limit.StaticLimit;

                }
            }
            else
            {
                // Add a new key
                DependencyVariables.Add("airflowCurves", curves);
            }
        }

        internal void ClearPricingVariables(Dictionary<PriceTypeEnumeration, double?> pricingValues)
        {
            foreach (var pricingValue in pricingValues.Where(pricingValue => DependencyVariables.ContainsKey(pricingValue.Key.DisplayName)))
            {
                DependencyVariables.Remove(pricingValue.Key.DisplayName);
            }

            foreach (var pricingValue in pricingValues.Where(pricingValue => _variableUpdatedBy.ContainsKey(pricingValue.Key.DisplayName)))
            {
                _variableUpdatedBy.Remove(pricingValue.Key.DisplayName);
            }
        }
    }
}