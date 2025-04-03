
using Microsoft.Extensions.Logging;

using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class IndexLookup
    {
        /// <summary>
        /// Name of the value being looked up.
        /// </summary>
        public string LookupValueName { get; set; }
        /// <summary>
        /// JsonLogic rule to provide the lookup value if needed.
        /// </summary>
        public string LookupJsonRule { get; set; }
        /// <summary>
        /// The ID of the matrix that will be searched within.
        /// </summary>
        public string MatrixID { get; set; }
        /// <summary>
        /// Determins if using the Metric or Imperial matrix/rules.
        /// </summary>
        public bool IsMetric { get; set; }
        /// <summary>
        /// Metric matrix and rules.
        /// </summary>
        public IndexLookupSOM Metric { get; set; }
        /// <summary>
        /// Imperial matrix and rules.
        /// </summary>
        public IndexLookupSOM Imperial { get; set; }

        public ILogger logger;

        public bool HasLookupValueName => !string.IsNullOrWhiteSpace(LookupValueName) && !LookupValueName.Equals("zero", StringComparison.OrdinalIgnoreCase);
        public bool HasLookupJsonRule => !string.IsNullOrWhiteSpace(LookupJsonRule);

        /// <summary>
        /// Get the index value of where the lookup value is located in the matrix.
        /// </summary>
        /// <param name="lookupValue">The value being looked up within the matrix.</param>
        /// <remarks>Returns a nullable int (nullable since the lookup might not find a match).</remarks>
        public int? GetLookupIndex(object lookupValue)
        {
            var indexLookup = IsMetric ? Metric : Imperial;
            logger?.LogInformation($"Entered {nameof(GetLookupIndex)} method for lookup value {lookupValue} with a lookup rule of {indexLookup.LookupRule}.");

            switch (indexLookup.LookupRule.ToUpper())
            {
                case "LE":
                case "<=":
                    // Assume single dimension array
                    return LessThanOrEqualTo(lookupValue.ToUSCultureDouble(), indexLookup.Values[0].Select(v => v.ToUSCultureDouble()).ToList());
                case "EQ":
                case "=":
                    // Assume single dimension array
                    if (lookupValue == null)
                        return EqualToNull(indexLookup.Values[0].ToList());
                    else
                        return EqualTo(lookupValue, indexLookup.Values[0].ToList());
                case "GE":
                case ">=":
                    // Assume single dimension array
                    return GreaterThanOrEqualTo(lookupValue.ToUSCultureDouble(), indexLookup.Values[0].Select(v => v.ToUSCultureDouble()).ToList());
                case "IN":
                    return InArray(lookupValue, indexLookup.Values);
                case "RANGE":
                    // Assume two dimension array
                    return Range(lookupValue.ToUSCultureDouble(), indexLookup.Values.ConvertToMultiArr<double>().ToList());
            }

            return null; // Default value if not found
        }

        public string GetValuesAsCommaDelimitedString()
        {
            var indexLookup = IsMetric ? Metric : Imperial;
            switch (indexLookup?.LookupRule.ToUpper())
            {
                case "LE":
                case "<=":
                case "EQ":
                case "=":
                case "GE":
                case ">=":
                    // Assume single dimension array
                    return "(" + string.Join(", ", indexLookup.Values[0].Select(v => v.ToUSCultureDouble())) + ")";
                case "IN":
                case "RANGE":
                    // Assume two dimension array
                    var values = indexLookup.Values.ConvertToMultiArr<double>().ToList();
                    var sb = new StringBuilder();
                    var valueCount = 0;
                    sb.Append('(');
                    foreach (var valueList in values)
                    {
                        sb.Append('[');
                        sb.Append(string.Join(',', valueList.Select(v => v.ToUSCultureDouble())));
                        sb.Append(']');
                        valueCount += 1;
                        if (valueCount != values.Count) sb.Append(',');
                    }
                    sb.Append(')');
                    return sb.ToString();
            }

            return "";
        }

        public (List<double>, List<double[]>) GetValuesAsList()
        {
            var indexLookup = IsMetric ? Metric : Imperial;
            switch (indexLookup?.LookupRule.ToUpper())
            {
                case "LE":
                case "<=":
                case "EQ":
                case "=":
                case "GE":
                case ">=":
                    // Assume single dimension array
                    return (indexLookup.Values[0].Select(v => v.ToUSCultureDouble()).ToList(), null);
                case "IN":
                case "RANGE":
                    // Assume two dimension array
                    return (null, indexLookup.Values.ConvertToMultiArr<double>().ToList());
            }

            return (null, null);
        }

        private static List<T> ConvertValuesToParameterType<T>(List<object> values)
        {
            if (!(values?.Count > 0)) return new List<T>();
            if (values[0].GetType() == typeof(T))
            {
                return values.Select(x => (T)x).ToList();
            }
            var returnList = new List<T>();
            try
            {
                returnList.AddRange(values.Select(x => x.ConvertTo<T>()));
            }
            catch { /*Do nothing here, just ignore if it can't convert*/ }

            return returnList;
        }

        private static int? LessThanOrEqualTo(double lookupParameter, List<double> values)
        {
            double? nextHighest = values.Find(v => v >= lookupParameter);
            if (nextHighest is null) return null; // The lookup parameter is larger than everything in the list

            return values.IndexOf(Convert.ToDouble(nextHighest));
        }

        private static int? EqualTo(object lookupParameter, List<object> values)
        {
            if (string.IsNullOrEmpty(lookupParameter?.ToString())) return null;
            var returnIndex = values.IndexOf(lookupParameter);
            if (returnIndex == -1)
            {
                // Check if we need to convert the list to match the lookup parameter type first, just in case
                var parameterType = lookupParameter.GetType();
                if (parameterType.IsNumericType() || lookupParameter.ToString().IsNumeric())
                {
                    // Force to double and compare
                    var convertedValues = ConvertValuesToParameterType<double>(values);
                    if (convertedValues?.Count > 0)
                    {
                        returnIndex = convertedValues.IndexOf(lookupParameter.ToUSCultureDouble());
                    }
                }
                else
                {
                    // Force to string and compare
                    var convertedValues = ConvertValuesToParameterType<string>(values);
                    if (convertedValues?.Count > 0)
                    {
                        returnIndex = convertedValues.IndexOf(lookupParameter.ToString());
                    }
                }
            }

            return (returnIndex == -1) ? null : returnIndex;
        }

        private static int? EqualToNull(List<object> values)
        {
            var returnIndex = values.IndexOf(null);
            return (returnIndex == -1) ? null : returnIndex;
        }

        private static int? GreaterThanOrEqualTo(double lookupParameter, List<double> values)
        {
            double? lastLower = values.LastOrDefault(v => v <= lookupParameter);
            if (lastLower is null) return null; // The lookup parameter is smaller than everything in the list

            return values.IndexOf(Convert.ToDouble(lastLower));
        }

        private static int? InArray(object lookupParameter, object[][] values)
        {
            if (values is null || values.Length == 0) return null;

            // Assume an array of objects containing arrays of objects (basically 2D object array)
            // The index we will be looking for is for each sub array in the parent array
            for (var i = 0; i < values.Length; i++)
            {
                var subArr = values[i];
                if (subArr is not null && Array.Find(subArr, v => v?.ToString()?.Equals(lookupParameter.ToString(), StringComparison.OrdinalIgnoreCase) == true) is not null)
                {
                    return i;
                }
            }

            // Lookup parameter is not found
            return null;
        }

        private static int? Range(double lookupParameter, List<double[]> ranges)
        {
            // For the first array, we will check if equal to or greater than the first index, along with equal to or less than the second index
            if (lookupParameter >= ranges[0]?[0] && lookupParameter <= ranges[0]?[1]) return 0;
            // For the remaining arrays, we will check if it is greater than the first index and equal to or less than the second index
            var rangeIndex = ranges.FindIndex(r => lookupParameter > r[0] && lookupParameter <= r[1]);
            if (rangeIndex != -1) return rangeIndex;
            return null;
        }
    }
}
