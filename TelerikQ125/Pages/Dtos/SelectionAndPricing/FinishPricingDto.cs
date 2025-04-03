namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class FinishPricingDto
    {
        public string FinishCode { get; set; }
        public string FinishDescription { get; set; }
        public string ColorCode { get; set; }
        public string ColorDescription { get; set; }
        public double? SquareFeet { get; set; }
        public double? SquareFeetSecond { get; set; }
        public double? MinCharge { get; set; }
        public double? ChargePerSquareFoot { get; set; }
        public double? ChargePerSquareFootSecond { get; set; }
        public string AccessoryDiscountId { get; set; }
        public double? ListAddOn { get; set; }
        public double? SetupCharge { get; set; }
        public double? SetupChargeSecond { get; set; }
        public string FinishAccessoryName { get; set; }

        internal bool HasFinishCharges => (MinCharge > 0) || (ChargePerSquareFoot > 0) || (ChargePerSquareFootSecond > 0);

        internal bool HasPaintSetupCharges => (ListAddOn > 0) || (SetupCharge > 0) || (SetupChargeSecond > 0);
    }
}
