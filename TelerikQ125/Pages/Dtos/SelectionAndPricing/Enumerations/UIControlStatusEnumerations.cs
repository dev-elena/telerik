using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class UIControlStatusEnumerations : BaseEnumeration
    {
        public static readonly UIControlStatusEnumerations Default = new UIControlStatusEnumerations(0, "Default");
        public static readonly UIControlStatusEnumerations Information = new UIControlStatusEnumerations(1, "Information");
        public static readonly UIControlStatusEnumerations Error = new UIControlStatusEnumerations(2, "Error");
        public static readonly UIControlStatusEnumerations Success = new UIControlStatusEnumerations(3, "Success");
        public static readonly UIControlStatusEnumerations Warning = new UIControlStatusEnumerations(4, "Warning");

        public UIControlStatusEnumerations() { }

        public UIControlStatusEnumerations(int value, string displayName) : base(value, displayName) { }
    }
}
