namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class ShowControlEventArgs
    {
        /// <summary>
        /// Name of the control (value name) to show or hide.
        /// </summary>
        public string ControlName { get; set; }
        /// <summary>
        /// Whether to show or hide the control.
        /// </summary>
        public bool ShowControl { get; set; }
        /// <summary>
        /// Name of the parent control this control is a dependent (child) of.
        /// </summary>
        public string ParentControlName { get; set; }
    }
}
