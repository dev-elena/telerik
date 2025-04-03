

using System.Collections.Generic;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public class SaveCustomPaintCodesEventArgs
    {
        /// <summary>
        ///  Custom paint colors to save for the current user.
        /// </summary>
        public IEnumerable<CustomPaintColorDto> CustomPaintColorDtos { get; set; }
    }
}
