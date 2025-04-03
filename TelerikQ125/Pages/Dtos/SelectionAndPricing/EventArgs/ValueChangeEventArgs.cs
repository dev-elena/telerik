using System;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class ValueChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the option that had its value changed.
        /// </summary>
        public string OptionName { get; set; }
        /// <summary>
        /// New value of the option.
        /// </summary>
        public object OptionValue { get; set; }
        /// <summary>
        /// JsonRule that caused this value change event (if available)
        /// </summary>
        public string JsonRule { get; set; }
        /// <summary>
        /// Should the value change event cause any rules to be run (validation, show, on select, etc...).
        /// </summary>
        public bool RunRules { get; set; } = true;
        /// <summary>
        /// Is this event triggered from a validation procedure?
        /// </summary>
        public bool IsFromValidationProcedure { get; set; } = false;
        /// <summary>
        /// The step id of the control causing this value change event.
        /// </summary>
        public int StepId { get; set; } = -1;
        /// <summary>
        /// Does the event need to apply the changes when done, or just mark the changes to get applied in a batch later?
        /// </summary>
        public bool ApplyChangesWhenDone { get; set; } = false;
        /// <summary>
        /// Is this event triggered from validating and needing to change the custom paint code?
        /// </summary>
        public bool IsFromCustomPaintCodeValidation { get; set; } = false;
    }
}
