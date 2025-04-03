using Microsoft.Extensions.Logging;

using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PriceMatrixEngine
    {
        /// <summary>
        /// The pricing method code telling the unit how to handle the pricing result from the matrix.
        /// </summary>
        public string PricingMethod { get; set; }
        /// <summary>
        /// Rules on how to get the column index value for the pricing matrix based on matching matrix id values.
        /// </summary>
        public IList<IndexLookup> PriceColumnIndexes { get; set; } = new List<IndexLookup>();
        /// <summary>
        /// Rules on how to get the row index value for the pricing matrix based on matching matrix id values.
        /// </summary>
        public IList<IndexLookup> PriceRowIndexes { get; set; } = new List<IndexLookup>();
        /// <summary>
        /// Allows to swap lookup value dimension names if no value found
        /// </summary>
        public bool AllowDimsSwap { get; set; }
        /// <summary>
        /// The JsonLogic rule on how to lookup the pricing value from the pricing matrix.
        /// </summary>
        public string JsonRule { get; set; }
        /// <summary>
        /// Pricing matrices.
        /// </summary>
        public IList<PriceMatrix> Matrices { get; set; } = new List<PriceMatrix>();

        /// <summary>
        /// Is this PriceMatrixEngine being used for validation rules matrices.
        /// </summary>
        internal bool IsValidationMatrixEngine { get; set; } = false;

        /// <summary>
        /// Get the value out of a price matrix (or multiple price matrices), depending on what pricing rules should be run.
        /// </summary>
        /// <param name="dependencyVariables">Dictionary of the currently selected values for the unit.</param>
        /// <param name="pricingRulesToRun">Array of matrix id values indicating which pricing rules to run.</param>
        /// <param name="logger">Optional logger to create logs from within this method.</param>
        /// <param name="raiseEvent">Should any value changes raise the value change event?</param>
        /// <remarks>Returns a nullable double of the price value (nullable since no result could be found).</remarks>
        public double? Value(Dictionary<string, object> dependencyVariables, string[] pricingRulesToRun, ILogger logger = null, bool raiseEvent = true)
        {
            var matrices = Matrices.Where(m => pricingRulesToRun.Contains(m.MatrixID)).ToList();
            var cindexes = PriceColumnIndexes.Where(c => pricingRulesToRun.Contains(c.MatrixID)).ToList();
            var rindexes = PriceRowIndexes.Where(r => pricingRulesToRun.Contains(r.MatrixID)).ToList();

            if (!(matrices?.Count > 0) || !(cindexes?.Count > 0) || !(rindexes?.Count > 0)) return null;

            // If this is for a validation matrix, we first need to determine if this should even be run by checking the c and r index lookup values
            if (IsValidationMatrixEngine && !CanRunValidationMatrixRule(dependencyVariables)) return null;

            dependencyVariables["mat"] = matrices;
            dependencyVariables["Cindex"] = cindexes;
            dependencyVariables["Rindex"] = rindexes;

            _ = HelperMethodsAndGeneralExtensions.HandleRulesResultType(JsonRule?.RunJsonRule(dependencyVariables, logger), dependencyVariables.ConvertToDynamic(),
                out IList<KeyValuePair<string, object>> diffs, out double? returnValue, logger);

            //if no price found and matrix allows dims swap 
            if ((returnValue == null || returnValue == 0) && AllowDimsSwap)
            {
                //swap lookup value names
                for (int i = 0; i < rindexes.Count; i++)
                {
                    var tempRIndexValue = rindexes[i].LookupValueName;
                    var tempRIndexRule = rindexes[i].LookupJsonRule;
                    rindexes[i].LookupValueName = cindexes[i].LookupValueName;
                    rindexes[i].LookupJsonRule = cindexes[i].LookupJsonRule;
                    cindexes[i].LookupValueName = tempRIndexValue;
                    cindexes[i].LookupJsonRule = tempRIndexRule;
                }

                dependencyVariables["Cindex"] = cindexes;
                dependencyVariables["Rindex"] = rindexes;

                _ = HelperMethodsAndGeneralExtensions.HandleRulesResultType(JsonRule?.RunJsonRule(dependencyVariables, logger), dependencyVariables.ConvertToDynamic(), out diffs, out returnValue, logger);
            }

            if (diffs?.Count > 0)
            {
                foreach (var diffValue in diffs)
                {
                    if (raiseEvent) OnValueChange(new ValueChangeEventArgs { OptionName = diffValue.Key, OptionValue = diffValue.Value, JsonRule = JsonRule, RunRules = false });
                }
            }

            dependencyVariables.Remove("mat");
            dependencyVariables.Remove("Cindex");
            dependencyVariables.Remove("Rindex");

            return returnValue;
        }

        public (bool? isValid, IList<KeyValuePair<string, object>>? dictDiffs, double? numericResult) ValidationResult(Dictionary<string, object> dependencyVariables, ILogger logger = null)
        {
            if (!CanRunValidationMatrixRule(dependencyVariables)) return (null, null, null);

            dependencyVariables["mat"] = Matrices;
            dependencyVariables["Cindex"] = PriceColumnIndexes;
            dependencyVariables["Rindex"] = PriceRowIndexes;

            var result = HelperMethodsAndGeneralExtensions.HandleRulesResultType(JsonRule?.RunJsonRule(dependencyVariables, logger), dependencyVariables.ConvertToDynamic(),
                out IList<KeyValuePair<string, object>> diffs, out double? returnValue, logger);

            return (result, diffs, returnValue);
        }

        private bool CanRunValidationMatrixRule(Dictionary<string, object> dependencyVariables)
        {
            var hasCindexLookupValue = false;
            foreach (var cindex in PriceColumnIndexes)
            {
                if (!string.IsNullOrWhiteSpace(cindex.LookupValueName))
                {
                    if (dependencyVariables.TryGetValue(cindex.LookupValueName, out var lookupValue) && !string.IsNullOrWhiteSpace(lookupValue?.ToString()))
                    {
                        hasCindexLookupValue = true;
                    }
                    else if (cindex.LookupValueName.ToLower().Equals("zero"))
                    {
                        hasCindexLookupValue = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(cindex.LookupJsonRule))
                {
                    var variables = cindex.LookupJsonRule.GetVariableNamesFromJsonRule();
                    foreach (var variable in variables)
                    {
                        if (dependencyVariables.TryGetValue(variable, out var lookupValue) && !string.IsNullOrWhiteSpace(lookupValue?.ToString()))
                        {
                            hasCindexLookupValue = true;
                        }
                    }
                }
            }
            var hasRindexLookupValue = false;
            foreach (var rindex in PriceRowIndexes)
            {
                if (!string.IsNullOrWhiteSpace(rindex.LookupValueName))
                {
                    if (dependencyVariables.TryGetValue(rindex.LookupValueName, out var lookupValue) && !string.IsNullOrWhiteSpace(lookupValue?.ToString()))
                    {
                        hasRindexLookupValue = true;
                    }
                    else if (rindex.LookupValueName.ToLower().Equals("zero"))
                    {
                        hasRindexLookupValue = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(rindex.LookupJsonRule))
                {
                    var variables = rindex.LookupJsonRule.GetVariableNamesFromJsonRule();
                    foreach (var variable in variables)
                    {
                        if (dependencyVariables.TryGetValue(variable, out var lookupValue) && !string.IsNullOrWhiteSpace(lookupValue?.ToString()))
                        {
                            hasCindexLookupValue = true;
                        }
                    }
                }
            }

            return (hasCindexLookupValue == true && hasRindexLookupValue == true);
        }

        protected virtual void OnValueChange(ValueChangeEventArgs e) => OnValueChangeEvent?.Invoke(this, e);
        public event EventHandler<ValueChangeEventArgs> OnValueChangeEvent;
    }
}
