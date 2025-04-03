using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class FinishEnumeration : BaseEnumeration
    {
        public static readonly FinishEnumeration PaintCode = new(0, "paintCode");
        public static readonly FinishEnumeration PaintCodeCustom = new(1, "paintCodeCust");
        public static readonly FinishEnumeration PaintDescriptionCustom = new(2, "paintDescCust");

        public FinishEnumeration() { }

        public FinishEnumeration(int value, string displayName) : base(value, displayName) { }
    }
}
