using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PricingChangedEventArgs
    {
        /// <summary>
        /// Updated pricing variables dictionary.
        /// </summary>
        /// <remarks>
        /// <para>Always available keys are:</para>
        /// <para>
        ///     <list type="bullet">
        ///         <item>Unit List</item>
        ///         <item>Base Price</item>
        ///         <item>Add-On Chrg</item>
        ///         <item>Item Mult</item>
        ///     </list>
        /// </para>
        /// </remarks>
        public Dictionary<PriceTypeEnumeration, double?> PricingVariables { get; init; }

        public bool RaiseEventFurther { get; set; } = true;
    }
}
