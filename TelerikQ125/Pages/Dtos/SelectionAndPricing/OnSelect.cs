using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class OnSelect
    {
        /// <summary>
        /// Any show rules that need to be run when an option is selected.
        /// </summary>
        public IList<ShowRule> ShowRules { get; set; } = new List<ShowRule>();
        /// <summary>
        /// Any pricing matrices and rules that need to be set to run when an option is selected.
        /// </summary>
        public IList<PriceRule> PriceRules { get; set; } = new List<PriceRule>();
        /// <summary>
        /// Any enable rules that need to be run when an option is selected.
        /// </summary>
        public IList<ShowRule> EnableRules { get; set; } = new List<ShowRule>();
        /// <summary>
        /// Any controls that need to be hidden when an option is selected.
        /// </summary>
        public IList<ShowRule> HideRules { get; set; } = new List<ShowRule>();
    }
}
