using Microsoft.Extensions.Logging;

using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class SizingLimits
    {
        /// <summary>
        /// Code on how to run the multisection calculation rule.
        /// </summary>
        public string MultiSectionCalculationRuleCode { get; set; }
        /// <summary>
        /// Code on how to apply price adjustments for multisection units.
        /// </summary>
        public string MultiSectionPriceTypeCode { get; set; }
        /// <summary>
        /// Price value to be used with the price adjustments for multisection units.
        /// </summary>
        public string MultiSectionPriceValue { get; set; }
        /// <summary>
        /// Name of this sizing limit set of rules.
        /// </summary>
        public string SizingLimitName { get; set; }
        ///// <summary>
        ///// JsonLogic rules to apply size limits for the unit.
        ///// </summary>
        //public IList<string> SizingLimitsJsonRules { get; set; } = new List<string>();
        /// <summary>
        /// JsonLogic rules to validate the size of the unit.
        /// </summary>
        public IList<string> ValidationJsonRules { get; set; } = new List<string>();

        /// <summary>
        /// Determine if the size of the unit is valid.
        /// </summary>
        /// <param name="dependencyVariables">Dictionary of all the currently selected values for the unit.</param>
        /// <remarks>Returns true or false.</remarks>
        private string[] fsdWithChangeunitSize = { "1213", "1213M", "1213SS", "1213VB", "1223", "1223-3", "1223M", "1223M-3", "1223SS", "1223SS-3", "1223VB", "1263", "1273", "1283" };

        public (bool, string) IsSizeValid(Dictionary<string, object> dependencyVariables, ILogger logger = null)
        {
            bool isChangeSize = false;
            if (dependencyVariables.TryGetValue("model", out var modelName))
            {
                if (fsdWithChangeunitSize.Contains(modelName))
                {
                    isChangeSize = true;
                }

            }
            var result = true;
            var message = "";
            foreach (var valRule in ValidationJsonRules)
            {
                var ruleResult = HelperMethodsAndGeneralExtensions.HandleRulesResultType(valRule.RunJsonRule(dependencyVariables, logger), dependencyVariables.ConvertToDynamic(logger), out IList<KeyValuePair<string, object>> depParamDiffs, out double? _, logger); //out IEnumerable<KeyValuePair<string, object>> diffs
                if (depParamDiffs != null)
                {
                    foreach (var diff in depParamDiffs)
                    {
                        if (diff.Key.Equals("validationError", StringComparison.OrdinalIgnoreCase))
                        {
                            //here we are checking if we have a max min size validation error ONLY if we know this is a changes size model (a handful of fsd need to add 2" to nomimal sizes and section accordingly)
                            if (diff.Value.ToString().Contains("section sizes are outside the max and min limits")) //may be a better way to check?
                            {
                                if (dependencyVariables.TryGetValue("maxSectW", out var maxSectW) && dependencyVariables.TryGetValue("maxSectH", out var maxSectH) && dependencyVariables.TryGetValue("sectW", out var sectW) && dependencyVariables.TryGetValue("sectH", out var sectH) && isChangeSize)
                                {
                                    double sizeAddition = 0.0;
                                    //we should check here the max/min sizes are still valid. If they're 2" larger, the max/min size is valid in this situation
                                    if (sectW.ToUSCultureDouble() - 2 <= maxSectW.ToUSCultureDouble() || sectH.ToUSCultureDouble() - 2 <= maxSectH.ToUSCultureDouble()) { return (true, message); }
                                    if (dependencyVariables.TryGetValue("som", out var som))
                                    {
                                        if (som.ToString().ToLower() == "imperial")
                                        {
                                            sizeAddition = 2;
                                            if (sectW.ToUSCultureDouble() - sizeAddition <= maxSectW.ToUSCultureDouble() || sectH.ToUSCultureDouble() - sizeAddition <= maxSectH.ToUSCultureDouble()) { return (true, message); }
                                        }
                                        else
                                        {
                                            //use mertic conversion here
                                            sizeAddition = 50.8; //addition 2" in metric
                                            if (Math.Round(sectW.ToUSCultureDouble() - sizeAddition) <= maxSectW.ToUSCultureDouble() || Math.Round(sectH.ToUSCultureDouble() - sizeAddition) <= maxSectH.ToUSCultureDouble()) { return (true, message); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ruleResult = false;
                                message = diff.Value.ToString();
                            }
                        }
                        else
                            dependencyVariables[diff.Key] = diff.Value;
                    }
                }
                if (ruleResult is not null) result &= (bool)ruleResult;
            }
            return (result, message);
        }
    }
}
