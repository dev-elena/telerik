

using System;
using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class SelectionAndPricingRowDtoExtensions
    {
        public static bool AuthorizedForVendorCountry(this UnitSelectionAndPricingRowDto dto, string vendorCountry)
        {
            return IsCorrectCountry(vendorCountry, dto.Country);
        }

        public static bool AuthorizedForVendorCountry(this UISelectDto dto, string vendorCountry)
        {
            return IsCorrectCountry(vendorCountry, dto.OpCtry);
        }

        public static bool AuthorizedForVendorCountry(this UIInfoDto dto, string vendorCountry)
        {
            return IsCorrectCountry(vendorCountry, dto.AccCtry);
        }

        private static bool IsCorrectCountry(string vendorCountry, string dtoCountry)
        {
            if (string.IsNullOrWhiteSpace(vendorCountry) || string.IsNullOrWhiteSpace(dtoCountry))
                return true;

            if (vendorCountry.ToLower() == "all" || dtoCountry.ToLower() == "a" || dtoCountry.ToLower() == "all")
                return true;

            switch (dtoCountry.ToLower())
            {
                case "c":
                    if (vendorCountry.ToLower() == "us" || vendorCountry.ToLower() == "uk")
                        return true;
                    else
                        return false;

                case "b":
                    if (vendorCountry.ToLower() == "us" || vendorCountry.ToLower() == "ca")
                        return true;
                    else
                        return false;

                default:
                    if (vendorCountry.Equals(dtoCountry, StringComparison.OrdinalIgnoreCase))
                        return true;

                    return false;
            }
        }

        public static string GetDefaultByCountry(this Dictionary<string, object> uiInfoDefaults, string vendorCountry)
        {
            if (uiInfoDefaults != null)
            {
                foreach (KeyValuePair<string, object> uiInfoDefault in uiInfoDefaults)
                {
                    if (IsCorrectCountry(vendorCountry, uiInfoDefault.Key))
                        return uiInfoDefault.Value.ToString();
                }
            }

            return null;
        }
    }
}
