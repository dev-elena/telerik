using System.ComponentModel;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    [DefaultProperty(nameof(Message))]
    public class InvalidOption
    {
        /// <summary>
        /// Name of the control (ValueName)
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// Control UI description
        /// </summary>
        public string ControlUIDescription { get; set; }

        /// <summary>
        /// Message indicating why the control is invalid.
        /// </summary>
        public string Message { get; set; }
    }
}