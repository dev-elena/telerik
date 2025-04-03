using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class ShowRule
    {
        /// <summary>
        /// List of values that could be selected by the user in which the rules apply for.
        /// </summary>
        /// <remarks>If SelectedValues and JsonValueRule both contain data to use, only one needs to be true.</remarks>
        public IList<string> SelectedValues { get; set; } = new List<string>();
        /// <summary>
        /// JsonLogic rule to determine whether to show the options listed within ShowOptions.
        /// </summary>
        /// <remarks>If SelectedValues and JsonValueRule both contain data to use, only one needs to be true.</remarks>
        public string JsonValueRule { get; set; }
        /// <summary>
        /// List of controls (value names) that should be shown if one of the selected values is matched.
        /// </summary>
        public IList<string> ShowOptions { get; set; } = new List<string>();
    }
}
