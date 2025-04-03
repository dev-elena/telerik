using System;
using System.Globalization;
using System.Threading;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    internal class InvariantCultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public InvariantCultureScope(bool changeCulture = true)
        {
            // Get the original culture in the current thread
            _originalCulture = Thread.CurrentThread.CurrentCulture;
            // If needing to change, apply the change to the current thread
            if (changeCulture) { Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; }
        }

        public void Dispose()
        {
            // Put back the culture on the current thread to the original value
            Thread.CurrentThread.CurrentCulture = _originalCulture;
        }
    }
}
