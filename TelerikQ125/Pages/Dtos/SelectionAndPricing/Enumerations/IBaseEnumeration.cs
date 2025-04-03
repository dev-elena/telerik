using System;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations
{
    public interface IBaseEnumeration : IComparable
    {
        int Value { get; }
        string DisplayName { get; }
        string ToString();
        bool Equals(object obj);
        int GetHashCode();
    }
}