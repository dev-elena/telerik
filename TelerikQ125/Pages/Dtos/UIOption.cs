using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;



namespace TelerikQ125.Pages.Dtos
{
    public class UIOption 
    {
        public int StepID { get; set; }
        public string ValueName { get; set; }
        public string UIDescription { get; set; }
        public string UIControlType { get; set; }
        private object selectedValue;
        // ...existing code...

        public object SelectedValue
        {
            get => selectedValue;
            set
            {
                // Only apply the change if it is an actual change
                if (UIControlType == UiControlTypeEnumeration.IntBox.DisplayName ||
                    UIControlType == UiControlTypeEnumeration.DecimalBox.DisplayName)
                {
                    if (Equals(selectedValue, value?.ConvertToNumericObject())) return;
                }
                else if (Equals(selectedValue, value)) return;

                if (UIControlType == UiControlTypeEnumeration.IntBox.DisplayName ||
                    UIControlType == UiControlTypeEnumeration.DecimalBox.DisplayName ||
                    UIControlType == UiControlTypeEnumeration.TextBox.DisplayName ||
                    UIControlType == UiControlTypeEnumeration.PerfTextBox.DisplayName)
                    selectedValue = value?.ConvertToNumericObject();
                else
                    selectedValue = value;
            }
        }
        public bool IsChildControl { get; set; }
        public IList<UIOption> DependentControls { get; set; } = new List<UIOption>();
        //private bool _showControl;
        //public bool ShowControl
        //{
        //    get
        //    {
        //        return _showControl;
        //    }
        //    set
        //    {
        //        _showControl = value;
        //    }
        //}
        private bool _enableControl = true;
        public bool EnableControl
        {
            get => _enableControl;
            internal set
            {
                _enableControl = value;
                if (DependentControls.Count != 0)
                {
                    foreach (var item in DependentControls)
                    {
                        item.EnableControl = value;
                    }
                }
            }
        }
        public bool HasDependentControls => DependentControls?.Count > 0;
        public ObservableCollection<ControlOption> UISelectOptions { get; private set; } = new ObservableCollection<ControlOption>();

    }

}
