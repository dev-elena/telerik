using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace TelerikQ125.Pages.Dtos
{
    public class ControlOption 
    {
        private bool showOption;
        internal bool IsPerformanceOption { get; set; }

        /// <summary>
        /// The value of the option (AKA: option code).
        /// </summary>
        public string OptionValue { get; set; }

        /// <summary>
        /// The description of the value.
        /// </summary>
        public string OptionDescription { get; set; }

        ///// <summary>
        ///// is from AV_AddiValueUnitCategory to determine if defualt value needs to be converted if metric is selected.
        ///// </summary>
        public string UnitCategory { get; set; }

        /// <summary>
        /// Any messages that need to get displayed to the UI when this option is selected.
        /// </summary>
        public IList<object> MessageIds { get; set; } = new List<object>();

        /// <summary>
        /// Any dependency rules that need to run validating this option's availability.
        /// </summary>

    }
}
