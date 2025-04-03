namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class ControlVisibilityStateChangedArgs
    {
        /// <summary>
        /// Name of the control.
        /// </summary>
        public string ValueName { get; set; }
        /// <summary>
        /// Step ID of the control.
        /// </summary>
        public int StepID { get; set; }
        /// <summary>
        /// Control visibility state.
        /// </summary>
        public bool IsVisible { get; set; }
        /// <summary>
        /// Control enabled state.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
