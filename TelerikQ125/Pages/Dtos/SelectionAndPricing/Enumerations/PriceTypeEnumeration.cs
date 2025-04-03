

using System.Diagnostics;
using System.Linq;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class PriceTypeEnumeration : BaseEnumeration
    {
        public static readonly PriceTypeEnumeration BasePrice = new(0, "Base Price");
        public static readonly PriceTypeEnumeration UnitList = new(1, "Unit List");
        public static readonly PriceTypeEnumeration ItemMultiplier = new(2, "Item Mult");
        public static readonly PriceTypeEnumeration AddOnCharge = new(3, "Add-On Chrg");
        public static readonly PriceTypeEnumeration PaintMinCharge = new(4, "Paint_MinCharge");
        public static readonly PriceTypeEnumeration PaintSetupCharge = new(5, "Paint_SetupCharge");
        public static readonly PriceTypeEnumeration GroupMinCharge = new(6, "GroupMinCharge");
        public static readonly PriceTypeEnumeration FinishCharge = new(7, "FinishCharge");
        public static readonly PriceTypeEnumeration ListAddOn = new(8, "ListAddOn");
        public static readonly PriceTypeEnumeration FinishChargePrime = new(9, "FinishCharge_Prime");
        public static readonly PriceTypeEnumeration FinishSFPrime = new(10, "FinishSF_Prime");
        public static readonly PriceTypeEnumeration PaintSetupChargePrime = new(11, "Paint_SetupCharge_Prime");
        public static readonly PriceTypeEnumeration FinishChargeSecond = new(12, "FinishCharge_Second");
        public static readonly PriceTypeEnumeration FinishSFSecond = new(13, "FinishSF_Second");
        public static readonly PriceTypeEnumeration PaintSetupChargeSecond = new(14, "Paint_SetupCharge_Second");
        public static readonly PriceTypeEnumeration BasePriceGroup2 = new(15, "Accessory Base");
        public static readonly PriceTypeEnumeration ItemMultiplierGroup2 = new(16, "Accessory Mult");

        public PriceTypeEnumeration() { }

        public PriceTypeEnumeration(int value, string displayName) : base(value, displayName) { }

        public static bool IsAPricingVariable(string variableName)
        {
            return typeof(PriceTypeEnumeration).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select(fieldInfo => fieldInfo.GetValue(null) as PriceTypeEnumeration).Any(priceEnum => priceEnum?.DisplayName.Equals(variableName) == true);
        }
    }
}
