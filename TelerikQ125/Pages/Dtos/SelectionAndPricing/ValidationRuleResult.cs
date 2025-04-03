using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    internal class ValidationRuleResult
    {
        public bool? Result { get; set; }
        public IEnumerable<KeyValuePair<string, object>> Diffs { get; set; }
        public double? Price { get; set; }
        public string ValRule { get; set; }
    }
}
