using System.Collections.Generic;
using System.Linq;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class DependencyVariablesExtensions
    {
        public static Dictionary<string, object> GetSortedByControlId(this Dictionary<string, object> unsortedVariables, ControlsManager controlsManager, bool OrderByDescending = false)
        {
            var sortedVariables = new Dictionary<int, string>();
            foreach (var (key, _) in unsortedVariables)
            {
                var control = controlsManager.FindControlByName(key);
                if (control is not null)
                {
                    if (string.IsNullOrWhiteSpace(control.ParentControlName))
                    {
                        sortedVariables.Add(control.StepID * 1000, key);
                    }
                    else
                    {
                        var parentControlId = controlsManager.FindControlByName(control.ParentControlName).StepID;
                        sortedVariables.Add(parentControlId * 1000 + control.StepID, key);
                    }
                    continue;
                }

                sortedVariables.Add(900000 + sortedVariables.Count, key);
            }

            var sorted = OrderByDescending ? sortedVariables.OrderByDescending(k => k.Key) : sortedVariables.OrderBy(k => k.Key);

            return sorted.ToDictionary(keyValuePair => keyValuePair.Value, keyValuePair => unsortedVariables[keyValuePair.Value]);
        }
    }
}
