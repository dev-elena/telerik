


using System;
using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public class CustomDictionaryComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            if (x is MotorAirflowLimit xm && y is MotorAirflowLimit ym)
            {
                return xm.CoilRows.Equals(ym.CoilRows) &&
                       xm.StaticLimit.Equals(ym.StaticLimit);

            }

            return x?.Equals(y) == true;
        }

        public int GetHashCode(object? obj)
        {
            return HashCode.Combine(obj);
        }
    }
}
