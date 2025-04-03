namespace TelerikQ125.Pages.Dtos
{
    public static class Object
    {
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
    }
}
