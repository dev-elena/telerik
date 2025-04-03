using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class NonVariableDimValue
    {
        public string DimensionName { get; set; }
        public List<double> SingleArrayValues { get; set; }
        public List<double[]> MultiArrayValues { get; set; }
        public string ValuesString { get; set; }
    }
}
