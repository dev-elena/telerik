
using static telerik_Q1_25.Pages.Dtos.SelectionAndPricing.BasicEnumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class OptionDependency
    {
        private SystemOfMeasureEnum systemOfMeasure;

        /// <summary>
        /// The unit of measure (system of measure) for this option dependency rule.
        /// </summary>
        public string UOM
        {
            get => systemOfMeasure.GetName(true); //Enum.GetName(systemOfMeasure).ToTitleCase();
            set => systemOfMeasure = value.GetEnumValue<SystemOfMeasureEnum>(); //Enum.Parse<SystemOfMeasureEnum>(value.ToUpper());
        }
        /// <summary>
        /// The JsonLogic rule to run.
        /// </summary>
        public string JsonRule { get; set; }
    }
}
