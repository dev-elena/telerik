using Newtonsoft.Json;



namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class IndexLookupSOM
    {
        /// <summary>
        /// Rule to use when looking up the value within the matrix.
        /// </summary>
        /// <remarks>Rules that can be used are: LE => Less-than or equal to, EQ => Equal to, GE => Greater-than or equal to, IN => Value contained within array, and RANGE => Value is within a range of values.</remarks>
        public string LookupRule { get; set; }
        /// <summary>
        /// Matrix of values being looked up.
        /// </summary>

        public object[][] Values { get; set; }
    }
}
