using Microsoft.Extensions.Logging;



using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;
using static telerik_Q1_25.Pages.Dtos.Class;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsEnumerable(this object d)
        {
            return d != null && (d.GetType().IsArray || (d as IEnumerable<object>) != null);
        }

        public static IEnumerable<object> MakeEnumerable(this object value)
        {
            if (value is Array) return (value as Array).Cast<object>();

            if (value is IEnumerable<object>) return (value as IEnumerable<object>);

            throw new ArgumentException("Argument is not enumerable");
        }

        public static bool EqualTo(this object value, object other)
        {
            if (value is string || other is string)
                return Convert.ToString(value).Equals(Convert.ToString(other));

            if ((value.IsNumeric() || value is bool) && (other.IsNumeric() || other is bool))
                return Convert.ToDouble(value).Equals(Convert.ToDouble(other));

            // special handling for nulls to avoid NullReferenceException
            if (value == null)
            {
                return other == null;
            }

            return value.Equals(other);
        }


        /// <summary>
        /// Equivalent to JavaScript "===" comparer. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool StrictEqualTo(this object value, object other)
        {
            // Added to compare number to number to handle situations where a numeric value is a string type in the client.
            if (value.CanBeNumeric() && other.CanBeNumeric())
            {
                return Convert.ToDouble(value).Equals(Convert.ToDouble(other));
            }

            return value == null || other == null ? value == other : value.Equals(other);
        }


        public static bool IsNumeric(this object value)
        {
            return (value is short || value is int || value is long || value is decimal || value is float || value is double);
        }

        public static bool CanBeNumeric(this object value)
        {
            if (value is null) return false;

            var isNumeric = value.IsNumeric();
            if (!isNumeric) isNumeric = short.TryParse(value.ToString(), out _);
            if (!isNumeric) isNumeric = int.TryParse(value.ToString(), out _);
            if (!isNumeric) isNumeric = long.TryParse(value.ToString(), out _);
            if (!isNumeric) isNumeric = decimal.TryParse(value.ToString(), out _);
            if (!isNumeric) isNumeric = float.TryParse(value.ToString(), out _);
            if (!isNumeric) isNumeric = double.TryParse(value.ToString(), out _);

            return isNumeric;
        }

        public static bool IsTruthy(this object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;
            if (value.IsNumeric()) return Convert.ToDouble(value) != 0;
            if (value.IsEnumerable()) return value.MakeEnumerable().Count() > 0;
            if (value is string) return (value as string).Length > 0;
            return true;
        }
    }
    public static class HelperMethodsAndGeneralExtensions
    {
        private static HashSet<Type> _numericTypes = new()
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(decimal),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(long),
            typeof(ulong),
            typeof(nint),
            typeof(nuint),
            typeof(float)
        };

        /// <summary>
        /// Handle the result from a JsonLogic rule. It could be either a bool, number, dynamic object, or multi-dimensional array.
        /// </summary>
        /// <param name="rulesResult">The result from running the JsonLogic rule.</param>
        /// <param name="dependencyParameters">Dynamic object containing all the currently selected keys/values for the unit.</param>
        /// <param name="depParamDiffs">If the rules result is a dynamic object or multi-dimensional array, this will contain any key/value pairs in the dependent variables (parameters) collection that need to be updated to new values.</param>
        /// <param name="numericResult">If the rules result is a numeric value, this will contain the resulting value.</param>
        public static bool? HandleRulesResultType(object rulesResult, object dependencyParameters, out IList<KeyValuePair<string, object>> depParamDiffs, out double? numericResult, ILogger logger = null)
        {
            depParamDiffs = null;
            numericResult = null;
            if (rulesResult is not null) // Null result is treated as 'rule not applicable'
            {
                if (rulesResult.GetType().Equals(typeof(bool)))
                {
                    return (bool)rulesResult;
                }

                if (rulesResult.GetType().IsNumericType() || rulesResult.ToString().IsNumeric())
                {
                    numericResult = rulesResult.ToUSCultureDouble();
                    return null;
                }

                if (rulesResult.GetType().Equals(typeof(object[])))
                {
                    bool? returnResult = null;
                    // Check each value in the array for bool types as it could be a bool returned in an array of results
                    foreach (var value in (object[])rulesResult)
                    {
                        var tempResult = HandleRulesResultType(value, dependencyParameters, out var diffs, out var num, logger);
                        if (diffs?.Count > 0)
                        {
                            var depList = (depParamDiffs is null) ? new List<KeyValuePair<string, object>>() : depParamDiffs;
                            foreach (var diff in diffs)
                            {
                                if (!depList.Contains(diff)) depList.Add(diff);
                            }
                            depParamDiffs = depList;
                        }

                        if (num is not null) numericResult = num;

                        if (tempResult is null) continue;
                        if (returnResult is not null)
                        {
                            returnResult &= tempResult;
                        }
                        else
                        {
                            returnResult = tempResult;
                        }
                    }

                    return returnResult;
                }

                // Assume a dynamic object; if it is of string or structure type, we can ignore as validation rules should not be returning those
                // If here, we will assume to replace the current dependency parameters with the newly updated ones
                try
                {
                    var depDictionary = rulesResult.ConvertToDictionary(logger, new CustomDictionaryComparer());
                    var curDepDictionary = dependencyParameters.ConvertToDictionary(logger, new CustomDictionaryComparer());
                    depParamDiffs = depDictionary.Except(curDepDictionary).ToList();
                }
                catch
                {
                    // Do nothing
                }
            }

            return null;
        }

        /// <summary>
        /// Limit the decimal precision for a double variable type.
        /// </summary>
        /// <param name="value">Double variable type to limit the precision for.</param>
        /// <param name="maxPrecision">The maximum decimal precision (decimal places) to leave.</param>
        public static double ToPrecision(this double value, int maxPrecision = 3)
        {
            return Math.Round(value, maxPrecision);
        }

        /// <summary>
        /// Convert a dictionary of IBaseEnumeration key to a dictionary of string key.
        /// </summary>
        /// <param name="dictionary">Dictionary of IBaseEnumeration key to convert.</param>
        public static Dictionary<string, double?> ToStringDictionary(this Dictionary<IBaseEnumeration, double?> dictionary)
        {
            return dictionary.ToDictionary(baseEnum => baseEnum.Key.DisplayName, baseEnum => baseEnum.Value);
        }

        /// <summary>
        /// Is the variable type a numeric type?
        /// </summary>
        /// <param name="type">Variable type to check.</param>
        public static bool IsNumericType(this Type type)
        {
            return _numericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
        }

        /// <summary>
        /// Is the string value numeric?
        /// </summary>
        /// <param name="value">String value to check.</param>
        public static bool IsNumeric(this string value)
        {
            return double.TryParse(value, out _);
        }

        /// <summary>
        /// Is the string value an integer number?
        /// </summary>
        /// <param name="value">String value to check.</param>
        public static bool IsNumericInt(this string value)
        {
            return int.TryParse(value, out _);
        }

        public static T GetDictionaryValue<T>(this Dictionary<string, T> source, string key, bool caseSensitive = true)
        {
            if (caseSensitive) return source.GetValueOrDefault(key);
            key = source.Keys.FirstOrDefault(k => string.Compare(key, k, StringComparison.OrdinalIgnoreCase) == 0);
            if (key == null) return default(T);
            return source[key];
        }

        public static double? NormalRounding(this double? value, int precision = 2)
        {
            if (value == null) return null;
            return Math.Round(value.ToUSCultureDouble(), precision, MidpointRounding.AwayFromZero);
        }

        public static bool EqualsProjectLineItemDto(this ProjectLineItemDisplayDto currentDto,
            ProjectLineItemDisplayDto compareDto)
        {
            if (currentDto.ItemId != compareDto.ItemId) return false;
            if (currentDto.ProjectId != compareDto.ProjectId) return false;
            if (currentDto.LineNumber != compareDto.LineNumber) return false;
            if (currentDto.Quantity != compareDto.Quantity) return false;
            if (currentDto.Model != compareDto.Model) return false;
            if (currentDto.ModelDescription != compareDto.ModelDescription) return false;
            if (currentDto.Dimensions != compareDto.Dimensions) return false;
            if (currentDto.Sections != compareDto.Sections) return false;
            if (currentDto.Variances != compareDto.Variances) return false;
            if (currentDto.ExtraAccessoryDetails != compareDto.ExtraAccessoryDetails) return false;
            if (Math.Abs(currentDto.UnitList - compareDto.UnitList) > 0.0) return false;
            if (Math.Abs(currentDto.BasePrice - compareDto.BasePrice) > 0.0) return false;
            if (Math.Abs(currentDto.AddOnCharge - compareDto.AddOnCharge) > 0.0) return false;
            if (Math.Abs(currentDto.ModelMultiplier - compareDto.ModelMultiplier) > 0.0) return false;
            if (currentDto.SystemOfMeasure != compareDto.SystemOfMeasure) return false;
            if (Math.Abs(currentDto.StandardDiscount - compareDto.StandardDiscount) > 0.0) return false;
            if (Math.Abs(currentDto.SdaDiscount - compareDto.SdaDiscount) > 0.0) return false;
            if (Math.Abs(currentDto.Discount - compareDto.Discount) > 0.0) return false;
            if (currentDto.ScheduleType != compareDto.ScheduleType) return false;
            if (currentDto.SpecialHandling != compareDto.SpecialHandling) return false;
            if (Math.Abs(currentDto.RushPercent - compareDto.RushPercent) > 0.0) return false;
            if (Math.Abs(currentDto.ProjectRushPercent - compareDto.ProjectRushPercent) > 0.0) return false;
            if (Math.Abs(currentDto.MarkupPercent - compareDto.MarkupPercent) > 0.0) return false;

            if (currentDto.Tags.Count != compareDto.Tags.Count) return false;
            for (var i = 0; i < currentDto.Tags.Count; i++)
            {
                if (currentDto.Tags[i].ItemId != compareDto.Tags[i].ItemId) return false;
                if (currentDto.Tags[i].TagId != compareDto.Tags[i].TagId) return false;
                if (currentDto.Tags[i].SortSeqNo != compareDto.Tags[i].SortSeqNo) return false;
                if (currentDto.Tags[i].Quantity != compareDto.Tags[i].Quantity) return false;
                if (currentDto.Tags[i].GroupTag != compareDto.Tags[i].GroupTag) return false;
                if (currentDto.Tags[i].FloorTag != compareDto.Tags[i].FloorTag) return false;
                if (currentDto.Tags[i].Tag1 != compareDto.Tags[i].Tag1) return false;
                if (currentDto.Tags[i].Tag2 != compareDto.Tags[i].Tag2) return false;
            }

            if (currentDto.ConfigurationVariables.Count != compareDto.ConfigurationVariables.Count) return false;
            for (var i = 0; i < currentDto.ConfigurationVariables.Count; i++)
            {
                if (currentDto.ConfigurationVariables[i].ItemId != compareDto.ConfigurationVariables[i].ItemId) return false;
                if (currentDto.ConfigurationVariables[i].StepId != compareDto.ConfigurationVariables[i].StepId) return false;
                if (currentDto.ConfigurationVariables[i].AccessoryName != compareDto.ConfigurationVariables[i].AccessoryName) return false;
                if (currentDto.ConfigurationVariables[i].AccessoryValue != compareDto.ConfigurationVariables[i].AccessoryValue) return false;
            }


            if (currentDto.CalculatedPricingVariants.Count != compareDto.CalculatedPricingVariants.Count) return false;
            for (var i = 0; i < currentDto.CalculatedPricingVariants.Count; i++)
            {
                if (currentDto.CalculatedPricingVariants[i].PricingCode != compareDto.CalculatedPricingVariants[i].PricingCode) return false;
                if (currentDto.CalculatedPricingVariants[i].VariantName != compareDto.CalculatedPricingVariants[i].VariantName) return false;
                if (Math.Abs(currentDto.CalculatedPricingVariants[i].PricingValue - compareDto.CalculatedPricingVariants[i].PricingValue) > 0.0) return false;
            }

            if (currentDto.PricingVersionNumber != compareDto.PricingVersionNumber) return false;
            if (currentDto.UpdatedOn != compareDto.UpdatedOn) return false;
            if (currentDto.UpdatedById != compareDto.UpdatedById) return false;
            if (currentDto.DiscountCalculatedOn != compareDto.DiscountCalculatedOn) return false;

            if (currentDto.Status != compareDto.Status) return false;
            if (currentDto.ValidationMessages != compareDto.ValidationMessages) return false;
            if (currentDto.DataVerNo != compareDto.DataVerNo) return false;
            if (currentDto.OrderID != compareDto.OrderID) return false;
            if (currentDto.OrderRevision != compareDto.OrderRevision) return false;

            if (currentDto.ItemFinishCharges.Count != compareDto.ItemFinishCharges.Count) return false;
            for (var i = 0; i < currentDto.ItemFinishCharges.Count; i++)
            {
                if (currentDto.ItemFinishCharges[i].ProjectId != compareDto.ItemFinishCharges[i].ProjectId) return false;
                if (currentDto.ItemFinishCharges[i].ItemId != compareDto.ItemFinishCharges[i].ItemId) return false;
                if (currentDto.ItemFinishCharges[i].AccessoryName != compareDto.ItemFinishCharges[i].AccessoryName) return false;
                if (currentDto.ItemFinishCharges[i].FinishCode != compareDto.ItemFinishCharges[i].FinishCode) return false;
                if (currentDto.ItemFinishCharges[i].FinishDescription != compareDto.ItemFinishCharges[i].FinishDescription) return false;
                if (currentDto.ItemFinishCharges[i].ColorCode != compareDto.ItemFinishCharges[i].ColorCode) return false;
                if (currentDto.ItemFinishCharges[i].ColorDescription != compareDto.ItemFinishCharges[i].ColorDescription) return false;
                if (Math.Abs(currentDto.ItemFinishCharges[i].ItemSquareFeet - compareDto.ItemFinishCharges[i].ItemSquareFeet) > 0.0) return false;
                if (Math.Abs(currentDto.ItemFinishCharges[i].MinCharge - compareDto.ItemFinishCharges[i].MinCharge) > 0.0) return false;
                if (Math.Abs(currentDto.ItemFinishCharges[i].ListPerSquareFoot - compareDto.ItemFinishCharges[i].ListPerSquareFoot) > 0.0) return false;
                if (currentDto.ItemFinishCharges[i].AccessoryDiscountId != compareDto.ItemFinishCharges[i].AccessoryDiscountId) return false;
                if (currentDto.ItemFinishCharges[i].UpdatedOn != compareDto.ItemFinishCharges[i].UpdatedOn) return false;
                if (currentDto.ItemFinishCharges[i].UpdatedById != compareDto.ItemFinishCharges[i].UpdatedById) return false;
            }

            if (currentDto.ItemPaintSetupCharges.Count != compareDto.ItemPaintSetupCharges.Count) return false;
            for (var i = 0; i < currentDto.ItemPaintSetupCharges.Count; i++)
            {
                if (currentDto.ItemPaintSetupCharges[i].ProjectId != compareDto.ItemPaintSetupCharges[i].ProjectId) return false;
                if (currentDto.ItemPaintSetupCharges[i].ItemId != compareDto.ItemPaintSetupCharges[i].ItemId) return false;
                if (currentDto.ItemPaintSetupCharges[i].AccessoryName != compareDto.ItemPaintSetupCharges[i].AccessoryName) return false;
                if (currentDto.ItemPaintSetupCharges[i].FinishCode != compareDto.ItemPaintSetupCharges[i].FinishCode) return false;
                if (currentDto.ItemPaintSetupCharges[i].FinishDescription != compareDto.ItemPaintSetupCharges[i].FinishDescription) return false;
                if (currentDto.ItemPaintSetupCharges[i].ColorCode != compareDto.ItemPaintSetupCharges[i].ColorCode) return false;
                if (currentDto.ItemPaintSetupCharges[i].ColorDescription != compareDto.ItemPaintSetupCharges[i].ColorDescription) return false;
                if (Math.Abs(currentDto.ItemPaintSetupCharges[i].ListAddOnCharge - compareDto.ItemPaintSetupCharges[i].ListAddOnCharge) > 0.0) return false;
                if (Math.Abs(currentDto.ItemPaintSetupCharges[i].MinSetupCharge - compareDto.ItemPaintSetupCharges[i].MinSetupCharge) > 0.0) return false;
                if (currentDto.ItemPaintSetupCharges[i].AccessoryDiscountId != compareDto.ItemPaintSetupCharges[i].AccessoryDiscountId) return false;
                if (currentDto.ItemPaintSetupCharges[i].UpdatedOn != compareDto.ItemPaintSetupCharges[i].UpdatedOn) return false;
                if (currentDto.ItemPaintSetupCharges[i].UpdatedById != compareDto.ItemPaintSetupCharges[i].UpdatedById) return false;
            }

            if (currentDto.CalculatedPricingVariants.Count != compareDto.CalculatedPricingVariants.Count) return false;
            for (var i = 0; i < currentDto.CalculatedPricingVariants.Count; i++)
            {
                if (currentDto.CalculatedPricingVariants[i].PricingCode != compareDto.CalculatedPricingVariants[i].PricingCode) return false;
                if (currentDto.CalculatedPricingVariants[i].VariantName != compareDto.CalculatedPricingVariants[i].VariantName) return false;
                if (Math.Abs(currentDto.CalculatedPricingVariants[i].PricingValue - compareDto.CalculatedPricingVariants[i].PricingValue) > 0.0) return false;
            }

            if (currentDto.SelectedItemAccessories.Count != compareDto.SelectedItemAccessories.Count) return false;
            for (var i = 0; i < currentDto.SelectedItemAccessories.Count; i++)
            {
                if (currentDto.SelectedItemAccessories[i].AccessoryTypeDescription != compareDto.SelectedItemAccessories[i].AccessoryTypeDescription) return false;
                if (currentDto.SelectedItemAccessories[i].AccessoryValueDescription != compareDto.SelectedItemAccessories[i].AccessoryValueDescription) return false;
                if (currentDto.SelectedItemAccessories[i].SelectedAccessoryCode != compareDto.SelectedItemAccessories[i].SelectedAccessoryCode) return false;
                if (currentDto.SelectedItemAccessories[i].AccessoryStepNumber != compareDto.SelectedItemAccessories[i].AccessoryStepNumber) return false;
                if (currentDto.SelectedItemAccessories[i].AccessoryConfigurationStepNumber != compareDto.SelectedItemAccessories[i].AccessoryConfigurationStepNumber) return false;
            }

            if (currentDto.CustomVariants.Count != compareDto.CustomVariants.Count) return false;
            for (var i = 0; i < currentDto.CustomVariants.Count; i++)
            {
                if (currentDto.CustomVariants[i].Step != compareDto.CustomVariants[i].Step) return false;
                if (currentDto.CustomVariants[i].Name != compareDto.CustomVariants[i].Name) return false;
                if (currentDto.CustomVariants[i].Value != compareDto.CustomVariants[i].Value) return false;
                if (currentDto.CustomVariants[i].Quantity != compareDto.CustomVariants[i].Quantity) return false;
                if (currentDto.CustomVariants[i].Amount != compareDto.CustomVariants[i].Amount) return false;
                if (currentDto.CustomVariants[i].OtherInformation != compareDto.CustomVariants[i].OtherInformation) return false;
            }

            return true;
        }

        public static double ToUSCultureDouble(this object value)
        {
            if (value == null) return 0;
            if (value is double dv) return dv;
            if (value is string sv)
            {
                if (double.TryParse(sv, CultureInfo.CreateSpecificCulture("en-US"), out var result)) return result;
            }
            else
            {
                if (double.TryParse(value.ToString(), out var result)) return result;
            }
            return 0;
        }

        public static object ConvertToNumericObject(this object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.IsNumeric()) return value;
            if (value.CanBeNumeric())
            {
                if (short.TryParse(value.ToString(), out var valueShort)) return valueShort;
                if (int.TryParse(value.ToString(), out var valueInt)) return valueInt;
                if (long.TryParse(value.ToString(), out var valueLong)) return valueLong;
                if (double.TryParse(value.ToString(), out var valueDbl)) return valueDbl;
                if (decimal.TryParse(value.ToString(), out var valueDec)) return valueDec;
                if (float.TryParse(value.ToString(), out var valueFl)) return valueFl;
            }
            return value;
        }

        public static object ConvertToDoubleObject(this object value)
        {
            if (value == null) return null;
            if (value.IsNumeric()) return value;
            if (value.CanBeNumeric())
            {
                if (double.TryParse(value.ToString(), out var valueDbl)) return valueDbl;
            }
            return value;
        }

        public static bool NullableEquals(this object value, object compareValue, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase, bool forceNullMatch = true)
        {
            if (forceNullMatch)
            {
                if (value is null && compareValue is null) return true;
                else if (value is null && compareValue is not null) return false;
                else if (value is not null && compareValue is null) return false;
            }

            if (value is null && string.IsNullOrEmpty(compareValue?.ToString())) return true;
            if (compareValue is null && string.IsNullOrEmpty(value?.ToString())) return true;

            return string.Equals(value?.ToString(), compareValue?.ToString(), stringComparison);
        }
    }
}
