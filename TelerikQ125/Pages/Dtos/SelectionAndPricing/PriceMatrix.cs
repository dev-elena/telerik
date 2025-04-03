using Newtonsoft.Json;



namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PriceMatrix
    {
        /// <summary>
        /// ID of the matrix.
        /// </summary>
        public string MatrixID { get; set; }
        /// <summary>
        /// Matrix containing pricing data.
        /// </summary>

        public object[][] DataMatrix { get; set; }
    }
}
