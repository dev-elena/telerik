using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PriceRule
    {
        /// <summary>
        /// List of values that could be selected by the user in which the rules apply for.
        /// </summary>
        public IList<string> SelectedValues { get; set; } = new List<string>();
        /// <summary>
        /// List of pricing rules (matrix id's) to be run if one of the selected values is matched.
        /// </summary>
        public IList<string> PriceRulesToRun { get; set; } = new List<string>();
    }
}
