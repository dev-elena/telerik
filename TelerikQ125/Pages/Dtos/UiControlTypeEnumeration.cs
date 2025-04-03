using System.Reflection;


namespace TelerikQ125.Pages.Dtos
{
    public class UiControlTypeEnumeration : BaseEnumeration
    {
        public static readonly UiControlTypeEnumeration SelectionList = new UiControlTypeEnumeration(1, "selectionList");

        public static readonly UiControlTypeEnumeration Group = new UiControlTypeEnumeration(2, "group");

        public static readonly UiControlTypeEnumeration DecimalBox = new UiControlTypeEnumeration(3, "decimalBox");

        public static readonly UiControlTypeEnumeration TextBox = new UiControlTypeEnumeration(4, "textBox");

        public static readonly UiControlTypeEnumeration IntBox = new UiControlTypeEnumeration(5, "intBox");

        public static readonly UiControlTypeEnumeration MultiSelectList = new UiControlTypeEnumeration(6, "multiSelectList");

        public static readonly UiControlTypeEnumeration HigharchicalDisplay = new UiControlTypeEnumeration(7, "higharchicalDisplay");

        public static readonly UiControlTypeEnumeration BinarySwitch = new UiControlTypeEnumeration(8, "binarySwitch");

        public static readonly UiControlTypeEnumeration GridFormat = new UiControlTypeEnumeration(9, "gridFormat");

        public static readonly UiControlTypeEnumeration NotApplicable = new UiControlTypeEnumeration(10, "NA");

        public static readonly UiControlTypeEnumeration ComboBox = new UiControlTypeEnumeration(11, "comboBox");
        public static readonly UiControlTypeEnumeration Button = new UiControlTypeEnumeration(12, "button");
        public static readonly UiControlTypeEnumeration DepGroup = new UiControlTypeEnumeration(13, "depGroup");
        public static readonly UiControlTypeEnumeration PerfTextBox = new UiControlTypeEnumeration(14, "perfTextBox");

        public static readonly UiControlTypeEnumeration CustomVariant = new UiControlTypeEnumeration(15, "customVariant");

        public static readonly UiControlTypeEnumeration LabelText = new UiControlTypeEnumeration(16, "label");

        public UiControlTypeEnumeration() { }

        public UiControlTypeEnumeration(int value, string displayName) : base(value, displayName) { }

        public object GetSelectedValueFromStringAsObject(string selectedValueString)
        {
            if (string.IsNullOrWhiteSpace(selectedValueString))
                return null;

            return Value switch
            {
                1 => selectedValueString,
                3 when decimal.TryParse(selectedValueString, out var decimalResult) => decimalResult,
                4 => selectedValueString,
                5 when int.TryParse(selectedValueString, out var intResult) => intResult,
                6 => selectedValueString,
                7 => selectedValueString,
                8 when bool.TryParse(selectedValueString, out var boolResult) => boolResult,
                9 => selectedValueString,
                11 => selectedValueString,
                14 => selectedValueString,
                15 => selectedValueString,
                16 => selectedValueString,
                _ => null
            };
        }
    }
    public interface IBaseEnumeration : IComparable
    {
        int Value { get; }
        string DisplayName { get; }
        string ToString();
        bool Equals(object obj);
        int GetHashCode();
    }
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
