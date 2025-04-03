namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PriceMatrixVal
    {
        /// <summary>
        /// The result from a pricing matrix lookup.
        /// </summary>
        public double? MatrixResult { get; set; }
        /// <summary>
        /// The pricing method code on what to do with the result.
        /// </summary>
        public string PricingMethodCode { get; set; }
    }
}
