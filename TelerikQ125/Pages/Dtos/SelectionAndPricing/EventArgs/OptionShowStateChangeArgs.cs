
using static telerik_Q1_25.Pages.Dtos.SelectionAndPricing.BasicEnumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class OptionShowStateChangeArgs
    {
        /// <summary>
        /// Value ID of the option.
        /// </summary>
        public string OptionValue { get; set; }
        /// <summary>
        /// Show state of the option.
        /// </summary>
        public ShowStateEnum ShowState { get; set; }
    }
}
