

using Newtonsoft.Json;
using System;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public static class BasicEnumerations
    {
        public enum SystemOfMeasureEnum
        {
            IMPERIAL = 0,
            METRIC = 1,
            UNIT = 2
        }

        public enum ShowStateEnum
        {
            Visible = 0,
            Hidden = 1,
            ReadOnly = 2
        }

        public enum CountryCodeEnum
        {
            US = 0,
            CA = 1,
            ALL = 3
        }

        public static string GetName(this Enum value, bool toTitleCase = false)
        {
            if (value is null) return null;

            var name = Enum.GetName(value.GetType(), value);
            return toTitleCase ? name : name;
        }

        public static T GetEnumValue<T>(this string value) where T : struct
        {
            if (string.IsNullOrWhiteSpace(value)) return default;

            return Enum.TryParse<T>(value, true, out T result) ? result : default;
        }
    }
    public class ValueTypeEnumeration : BaseEnumeration
    {
        public static readonly ValueTypeEnumeration NotApplicable = new(0, "NA");
        public static readonly ValueTypeEnumeration Temperature = new(1, "Temperature");
        public static readonly ValueTypeEnumeration Airflow = new(2, "Airflow");
        public static readonly ValueTypeEnumeration FluidFlow = new(3, "FluidFlow");
        public static readonly ValueTypeEnumeration AirVelocity = new(4, "AirVelocity");
        public static readonly ValueTypeEnumeration FluidVelocity = new(5, "FluidVelocity");
        public static readonly ValueTypeEnumeration AirPressure = new(6, "AirPressure");
        public static readonly ValueTypeEnumeration FluidPressure = new(7, "FluidPressure");
        public static readonly ValueTypeEnumeration LengthLarge = new(8, "LengthLarge");
        public static readonly ValueTypeEnumeration LengthSmall = new(9, "LengthSmall");
        public static readonly ValueTypeEnumeration HeatCapacity = new(10, "HeatCapacity");

        public ValueTypeEnumeration() { }

        public ValueTypeEnumeration(int value, string displayName) : base(value, displayName) { }
    }

    public class AirflowTypeEnumeration : BaseEnumeration
    {
        public static readonly AirflowTypeEnumeration PrimaryMax = new(0, "Primary Max");
        public static readonly AirflowTypeEnumeration PrimaryMin = new(1, "Primary Min");
        public static readonly AirflowTypeEnumeration FanMax = new(2, "Cooling Fan Max");
        public static readonly AirflowTypeEnumeration FanDeadband = new(3, "Fan Deadband");
        public static readonly AirflowTypeEnumeration HeatingFanMax = new(4, "Htg Fan Max");
        public static readonly AirflowTypeEnumeration HeatingStageOne = new(5, "Htg Stg 1");
        public static readonly AirflowTypeEnumeration HeatingStageTwo = new(6, "Htg Stg 2");
        public static readonly AirflowTypeEnumeration CoolingDeck = new(7, "Cooling Deck");
        public static readonly AirflowTypeEnumeration HeatingDeck = new(8, "Heating Deck");

        public AirflowTypeEnumeration() { }

        public AirflowTypeEnumeration(int value, string displayName) : base(value, displayName) { }
    }
}