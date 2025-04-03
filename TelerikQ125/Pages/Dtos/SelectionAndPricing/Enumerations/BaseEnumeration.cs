using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations
{
    public abstract class BaseEnumeration : IBaseEnumeration
    {
        protected BaseEnumeration()
        {
        }

        protected BaseEnumeration(int value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public int Value { get; }

        public string DisplayName { get; }

        public override string ToString() =>
            $"{GetType().Name}.{DisplayName}";

        public static IEnumerable<TEnumeration> GetAll<TEnumeration>() where TEnumeration : BaseEnumeration, new()
        {
            var type = typeof(TEnumeration);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new TEnumeration();
                if (info.GetValue(instance) is TEnumeration locatedValue)
                    yield return locatedValue;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseEnumeration otherValue)
            {
                var typeMatches = GetType().Equals(obj.GetType());
                var valueMatches = Value.Equals(otherValue.Value);

                return typeMatches && valueMatches;
            }
            return false;
        }

        public override int GetHashCode() =>
            Value.GetHashCode();

        public int CompareTo(object other) =>
            Value.CompareTo(((BaseEnumeration)other).Value);

        public static int AbsoluteDifference(BaseEnumeration firstValue, BaseEnumeration secondValue) =>
            Math.Abs(firstValue.Value - secondValue.Value);

        public static TEnumeration FromValue<TEnumeration>(int value, bool returnNullOnInvalid = false) where TEnumeration : BaseEnumeration, new() =>
            returnNullOnInvalid ?
            TryParse<TEnumeration, int>(value, "Value", item => Equals(item.Value, value)) :
            Parse<TEnumeration, int>(value, "Value", item => Equals(item.Value, value));

        public static TEnumeration FromDisplayName<TEnumeration>(string displayName, bool returnNullOnInvalid = false) where TEnumeration : BaseEnumeration, new() =>
            returnNullOnInvalid ?
            TryParse<TEnumeration, string>(displayName, "DisplayName", item => string.Equals(item.DisplayName, displayName)) :
            Parse<TEnumeration, string>(displayName, "DisplayName", item => string.Equals(item.DisplayName, displayName));

        public static TEnumeration FromDisplayNameOrdinalIgnoreCase<TEnumeration>(string displayName, bool returnNullOnInvalid = false) where TEnumeration : BaseEnumeration, new() =>
            returnNullOnInvalid ?
            TryParse<TEnumeration, string>(displayName, "DisplayName", item => string.Equals(item.DisplayName, displayName, StringComparison.OrdinalIgnoreCase)) :
            Parse<TEnumeration, string>(displayName, "DisplayName", item => string.Equals(item.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));

        public static bool operator ==(BaseEnumeration left, BaseEnumeration right)
        {
            return left is null
                ? right is null
                : left.Equals(right);
        }

        public static bool operator !=(BaseEnumeration left, BaseEnumeration right) =>
            !(left == right);

        public static bool operator <(BaseEnumeration left, BaseEnumeration right) =>
            left is null ? right is object : left.CompareTo(right) < 0;

        public static bool operator <=(BaseEnumeration left, BaseEnumeration right) =>
            left is null || left.CompareTo(right) <= 0;

        public static bool operator >(BaseEnumeration left, BaseEnumeration right) =>
            left is object && left.CompareTo(right) > 0;

        public static bool operator >=(BaseEnumeration left, BaseEnumeration right) =>
            left is null ? right is null : left.CompareTo(right) >= 0;

        protected static TEnumeration Parse<TEnumeration, TValue>(TValue value, string description, Func<TEnumeration, bool> predicate) where TEnumeration : BaseEnumeration, new()
        {
            var matchingItem = GetAll<TEnumeration>().FirstOrDefault(predicate);
            return matchingItem ?? throw new ApplicationException($"'{value}' is not a valid {description} in {typeof(TEnumeration).Name}");
        }

        protected static TEnumeration TryParse<TEnumeration, TValue>(TValue value, string description, Func<TEnumeration, bool> predicate) where TEnumeration : BaseEnumeration, new()
        {
            return GetAll<TEnumeration>().FirstOrDefault(predicate);
        }
    }
}
