using System.Collections.Generic;
using System.Linq;
using TelerikQ125.Pages.Dtos;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    internal class ControlSelectionChanges
    {
        public string ControlName { get; set; }
        public List<ControlOption> OptionsToAdd { get; set; } = new();
        public List<ControlOption> OptionsToRemove { get; set; } = new();

        public bool HasOptionsToAdd
        {
            get => OptionsToAdd?.Count > 0;
        }

        public bool HasOptionsToRemove
        {
            get => OptionsToRemove?.Count > 0;
        }
    }
}