namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class NeedTokenEventArgs
    {
        /// <summary>
        /// User bearer token for API authorization.
        /// </summary>
        public string BearerToken { get; set; }
        /// <summary>
        /// Base address URL of the Clients API.
        /// </summary>
        public string ClientsApiBaseAddressUrl { get; set; }
        internal string ControlName { get; set; }
    }
}
