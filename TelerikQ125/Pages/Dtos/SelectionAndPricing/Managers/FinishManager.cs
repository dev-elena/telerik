using System.Collections.Generic;
using System.Linq;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;
using TelerikQ125.Pages.Dtos;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public static class FinishManager
    {
        private static readonly List<string> FinishVariables = new()
        {
            "Finish",
            "Finish2",
            "FinishInt",
            "IntFinish",
            "OBDFinish",
            "IntrPaint",
            "FinishCtn"
        };

        public static IEnumerable<FinishPricingDto> GetFinishPricingDto(UnitSelectionAndPricingEngine engine)
        {
            var pricing = new List<FinishPricingDto>();

            foreach (var finishVariable in FinishVariables)
            {
                var control = engine.ControlsManager.FindControlByName(finishVariable);
                if (control is null || !(control.FinishPricing?.Count > 0)) continue;

                var dto = new FinishPricingDto()
                {
                    FinishAccessoryName = finishVariable,
                    FinishCode = control.SelectedValue.ToString(),
                    FinishDescription = control.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == control.SelectedValue.ToString())?.OptionDescription
                };

                var color = GetColorData(control);
                if (color is not null)
                {
                    dto.ColorCode = color.Code;
                    dto.ColorDescription = color.Description;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.FinishSFPrime, out var finishSfPrime))
                {
                    dto.SquareFeet = finishSfPrime;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.FinishSFSecond, out var finishSfSecond))
                {
                    dto.SquareFeetSecond = finishSfSecond;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.PaintMinCharge, out var minCharge))
                {
                    dto.MinCharge = minCharge;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.FinishChargePrime, out var finishChargePrime))
                {
                    dto.ChargePerSquareFoot = finishChargePrime;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.FinishChargeSecond, out var finishChargeSecond))
                {
                    dto.ChargePerSquareFootSecond = finishChargeSecond;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.ListAddOn, out var listAddOn))
                {
                    dto.ListAddOn = listAddOn;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.PaintSetupChargePrime, out var setupChargePrime))
                {
                    dto.SetupCharge = setupChargePrime;
                }

                if (control.FinishPricing.TryGetValue(PriceTypeEnumeration.PaintSetupChargeSecond, out var setupChargeSecond))
                {
                    dto.SetupChargeSecond = setupChargeSecond;
                }

                dto.AccessoryDiscountId = engine.AccessoryDiscountId;
                pricing.Add(dto);
            }

            return pricing;
        }

        public static void SetFinishPricingForControl(UIOption control, string key, object value)
        {
            PriceTypeEnumeration priceType = BaseEnumeration.FromDisplayName<PriceTypeEnumeration>(key, true);
            if (priceType is null) return;

            if (priceType == PriceTypeEnumeration.FinishChargePrime ||
                priceType == PriceTypeEnumeration.FinishChargeSecond ||
                priceType == PriceTypeEnumeration.ListAddOn ||
                priceType == PriceTypeEnumeration.PaintSetupChargePrime ||
                priceType == PriceTypeEnumeration.PaintSetupChargeSecond ||
                priceType == PriceTypeEnumeration.FinishSFPrime ||
                priceType == PriceTypeEnumeration.FinishSFSecond ||
                priceType == PriceTypeEnumeration.PaintMinCharge)
            {
                if (double.TryParse(value?.ToString(), out var price)) control.FinishPricing[priceType] = price;
                else if (value is null) control.FinishPricing[priceType] = null;
            }
        }

        public static IList<UnitColor> GetColors(UnitSelectionAndPricingEngine engine)
        {
            var colors = new List<UnitColor>();

            foreach (var finishVariable in FinishVariables)
            {
                var control = engine.ControlsManager.FindControlByName(finishVariable);
                if (control is null) continue;

                var color = GetColorData(control);
                if (color is not null) colors.Add(color);
            }

            return colors;
        }

        private static UnitColor GetColorData(UIOption control)
        {
            var color = new UnitColor();

            if (!(control.DependentControls?.Count > 0)) return null;

            foreach (var depControl in control.DependentControls)
            {
                if ((depControl.ValueName.Equals(FinishEnumeration.PaintCode.DisplayName) ||
                    depControl.ValueName.Equals(FinishEnumeration.PaintCodeCustom.DisplayName)) && depControl.ShowControl)
                {
                    color.Code = depControl.SelectedValue?.ToString();
                    // Get the description from the color code if available
                    var desc = depControl.UISelectOptionsFullList.FirstOrDefault(c => c.OptionValue == color.Code)?.OptionDescription;
                    if (string.IsNullOrWhiteSpace(desc)) continue;

                    color.Description = desc;
                    break;
                }
                else if (string.IsNullOrWhiteSpace(color.Code))
                {
                    // apply finishCode when control hasn't a dependent control with a mandatory colorCode and has finishCharges 
                    // but only if it wasn't already set above during an earlier iteration of this loop
                    color.Code = control.SelectedValue?.ToString();
                }
                if (depControl.ValueName.Equals(FinishEnumeration.PaintDescriptionCustom.DisplayName) && depControl.ShowControl)
                {
                    color.Description = depControl.SelectedValue?.ToString();
                }
            }

            return string.IsNullOrWhiteSpace(color.Code) ? null : color;
        }
    }
}
