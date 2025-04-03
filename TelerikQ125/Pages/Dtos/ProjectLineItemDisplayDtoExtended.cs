
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Telerik.Blazor.Components.MultiSelect;

using TelerikQ125.Pages.Dtos;

namespace telerik_Q1_25.Pages.Dtos
{

    public class ProjectLineItemDisplayDtoExtended 
    {
        public int LineNumber { get; set; }

        public long Quantity { get; set; }

        public string Model { get; set; }

        public string Dimensions { get; set; }
        public ObservableCollection<UIOption> DimsUiOptions { get; set; } =
            new ObservableCollection<UIOption>();



    }
 

}
