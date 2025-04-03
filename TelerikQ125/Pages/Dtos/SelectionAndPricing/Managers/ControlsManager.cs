

using System;
using System.Collections.Generic;
using System.Linq;
using TelerikQ125.Pages.Dtos;
using static telerik_Q1_25.Pages.Dtos.Class;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public sealed class ControlsManager : IDisposable
    {
        private readonly List<ControlListItem> _unitControlsIndex = new();
        private bool _isDisposed;

        /// <summary>
        /// List of UI controls to present to the user.
        /// </summary>
        /// <remarks>Each control has a ShowControl property to determine if the control should be visible/enabled.</remarks>
        internal IList<UIOption> UiSelect { get; set; } = new List<UIOption>();

        internal IList<string> FullControlList => _unitControlsIndex.ConvertAll(c => c.ControlName);

        public bool IsInitializing { get; set; }

        internal const string CustomControlNamePrefix = "Custom_Var_";
        internal const string SpecialBasePriceControl = "SpecialBP";

        internal void SetShowStates(DependencyVariablesManager dependencyVariablesManager)
        {
            foreach (var uiOption in UiSelect)
            {
                uiOption.SetSelectOptionsShowStates(dependencyVariablesManager.DependencyVariables);

                uiOption.RunShowControlRules(dependencyVariablesManager.DependencyVariables, this, uiOption.HasOnSelectRules);
            }
        }

        internal void AddControlToIndex(string valueName, string uIControlType, int stepId, string parentControlName = null, int parentStepId = -1, string grandparentControlName = "", int grandparentStepId = -1)
        {
            var newItem = new ControlListItem { ControlName = valueName, ControlUIType = uIControlType, ControlParentName = parentControlName, ControlGrandparentName = grandparentControlName, StepId = stepId, ParentStepId = parentStepId, GrandparentStepId = grandparentStepId };
            if (!_unitControlsIndex.Contains(newItem)) _unitControlsIndex.Add(newItem);
        }

        internal void RemoveControlFromIndex(string valueName, string uIControlType, int stepId, string parentControlName = null, int parentStepId = -1, string grandparentControlName = "", int grandparentStepId = -1)
        {
            var foundItem = _unitControlsIndex.FirstOrDefault(c => c.ControlName == valueName && c.ControlUIType == uIControlType && c.StepId == stepId && c.ControlParentName == parentControlName && c.ParentStepId == parentStepId && c.ControlGrandparentName == grandparentControlName && c.GrandparentStepId == grandparentStepId);
            if (foundItem != null)
            {
                _unitControlsIndex.Remove(foundItem);
            }
        }

        internal UIOption FindControlByName(string valueName, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundControl = _unitControlsIndex.Find(c => c.ControlName.Equals(valueName, stringComparison));
            return foundControl is null ? null : RetrieveUiOption(foundControl, stringComparison);
        }

        internal UIOption FindControlByName(string valueName, string parentName, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundControl = _unitControlsIndex.Find(c => c.ControlName.Equals(valueName, stringComparison) && c.ControlParentName?.Equals(parentName, stringComparison) == true);
            return foundControl is null ? null : RetrieveUiOption(foundControl, stringComparison);
        }

        internal UIOption FindControlWhereNameStartsWith(string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundControl = _unitControlsIndex.Find(c => c.ControlName.StartsWith(value, stringComparison));
            return foundControl is null ? null : RetrieveUiOption(foundControl, stringComparison);
        }

        internal UIOption FindControlWhereNameContains(string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundControl = _unitControlsIndex.Find(c => c.ControlName.Contains(value, stringComparison));
            return foundControl is null ? null : RetrieveUiOption(foundControl, stringComparison);
        }

        internal UIOption FindControlContainingOptionCode(string optionCode, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundControl = UiSelect.FirstOrDefault(c => c.IsSelectionTypeControl && c.UISelectOptions.FirstOrDefault(o => o.OptionValue.Equals(optionCode, stringComparison)) is not null);
            return foundControl;
        }

        private UIOption RetrieveUiOption(ControlListItem optionValues, StringComparison stringComparison)
        {
            if (string.IsNullOrWhiteSpace(optionValues.ControlParentName))
            {
                return UiSelect.FirstOrDefault(c => c.ValueName.Equals(optionValues.ControlName, stringComparison));
            }

            return RetreiveUiOptionFromParent(optionValues, stringComparison, UiSelect);
        }

        private UIOption RetreiveUiOptionFromParent(ControlListItem optionValues, StringComparison stringComparison, IEnumerable<UIOption> controlsList)
        {
            var parentControl = controlsList.FirstOrDefault(c => c.ValueName.Equals(optionValues.ControlParentName, stringComparison));
            if (parentControl is null)
            {
                // Check dependent controls of dependent controls
                foreach (var control in controlsList)
                {
                    var foundControl = RetreiveUiOptionFromParent(optionValues, stringComparison, control.DependentControls.Where(c => c.HasDependentControls));
                    if (foundControl is not null) return foundControl;
                }
            }

            return parentControl?.DependentControls?.FirstOrDefault(d => d.ValueName.Equals(optionValues.ControlName, stringComparison));
        }

        static internal string GetCustomControlName(int step)
        {
            return $"{CustomControlNamePrefix}{step}";
        }

        static internal int GetCustomControlStep(string valueName)
        {
            var step = valueName[CustomControlNamePrefix.Length..];
            _ = int.TryParse(step, out int result);
            return result;
        }

        internal string GetControlNameFromStepId(int stepId)
        {
            var grandparentStepId = 0;
            var parentStepId = 0;
            var childStepId = 0;

            if (stepId >= 100000)
            {
                if ((stepId / 100000D).ToString().Contains('.'))
                {
                    grandparentStepId = (int)Math.Floor(stepId / 100000D);
                    stepId -= (int)(grandparentStepId * 100000);
                    if ((stepId / 1000D).ToString().Contains("."))
                    {
                        // Child control
                        parentStepId = (int)Math.Floor(stepId / 1000D);
                        childStepId = (int)Math.Round(((stepId / 1000D) - parentStepId) * 1000, 0);
                        return _unitControlsIndex.Find(c => c.StepId == childStepId && c.ParentStepId == parentStepId && c.GrandparentStepId == grandparentStepId)?.ControlName;
                    }
                    else
                    {
                        // Parent control
                        stepId = (int)(stepId / 1000);
                        return _unitControlsIndex.Find(c => c.StepId == stepId && c.ParentStepId == grandparentStepId && c.GrandparentStepId == -1)?.ControlName;
                    }
                }
                else
                {
                    // Grandparent control
                    stepId = (int)(stepId / 100000);
                }
            }

            if (stepId >= 1000)
            {
                if ((stepId / 1000D).ToString().Contains("."))
                {
                    // Child control
                    parentStepId = (int)Math.Floor(stepId / 1000D);
                    childStepId = (int)Math.Round(((stepId / 1000D) - parentStepId) * 1000, 0);
                    return _unitControlsIndex.Find(c => c.StepId == childStepId && c.ParentStepId == parentStepId && c.GrandparentStepId == -1)?.ControlName;
                }
                else
                {
                    // Parent control
                    stepId = (int)(stepId / 1000);
                }
            }

            // Assume parent control
            return _unitControlsIndex.Find(c => c.StepId == stepId && c.ParentStepId == -1 && c.GrandparentStepId == -1)?.ControlName;
        }

        internal void SetDefaultValues(DependencyVariablesManager dependencyVariablesManager)
        {
            foreach (var control in UiSelect)
            {
                var depVarValue = dependencyVariablesManager.GetValueFromDependencyVariable(control.ValueName);

                if (!Equals(depVarValue, control.SelectedValue) || (depVarValue is null))
                {
                    control.SetDefaultValue(false);
                }

                SetDefaultDependentValues(dependencyVariablesManager, control);
            }
        }

        private void SetDefaultDependentValues(DependencyVariablesManager dependencyVariablesManager, UIOption control)
        {
            if (control.DependentControls?.Count > 0)
            {
                foreach (var depControl in control.DependentControls)
                {
                    var depControlVarValue = dependencyVariablesManager.GetValueFromDependencyVariable(depControl.ValueName);
                    if (!Equals(depControlVarValue, depControl.SelectedValue) || (depControlVarValue is null))
                    {
                        string unitCat = null;
                        var depVarValue = dependencyVariablesManager.GetValueFromDependencyVariable(control.ValueName);
                        if (depVarValue != null)
                        {
                            var co = control.UISelectOptions.FirstOrDefault(x => x.OptionValue.ToLower() == depVarValue.ToString().ToLower());
                            if (co != null && co.UnitCategory != null)
                            {
                                if (co.UnitCategory.ToLower() == "len" || co.UnitCategory.ToLower() == "lenft")
                                {
                                    unitCat = co.UnitCategory;
                                }
                            }
                        }
                        depControl.SetDefaultValue(false, unitCat);
                    }
                    //if (!IsInitializing)
                    //{
                    //    var depVarValue = dependencyVariablesManager.GetValueFromDependencyVariable(control.ValueName);
                    //    if (depVarValue != null)
                    //    {
                    //        var co = control.UISelectOptions.FirstOrDefault(x => x.OptionValue.ToLower() == depVarValue.ToString().ToLower());
                    //        if (co != null && co.UnitCategory != null)
                    //        {
                    //            if (co.UnitCategory.ToLower() == "len" || co.UnitCategory.ToLower() == "lenft")
                    //            {
                    //                depControl.SetDefaultValue(true, co.UnitCategory.ToLower());
                    //            }
                    //        }
                    //    }
                    //}
                    //need to check the parents value, if the option code had unit cat of len or lenft, the default needs to be converted if changed to metric

                    SetDefaultDependentValues(dependencyVariablesManager, depControl);

                    //else if (!this.IsInitializing && ((control.UISelectOptions..ToLower() == "len") || control.UnitCategory.ToLower() == "lenft")) //if control manager is NOT initilalizing and the unit cat is len or lenft, this number needs to be converted depending if SOM is metric or imperial i.e sleeve length, defualt could be 16in, if metric is choosen, this defualt needs to be converted to 406mm
                    //{
                    //    depControl.SetDefaultValue();
                    //}
                    //SetDefaultDependentValues(dependencyVariablesManager, depControl);
                }
            }
        }

        /// <summary>
        /// Used when default values need to be set after rules have been run from an internal process.
        /// Was created for the initialization process of an existing item.
        /// </summary>
        internal void ApplyDefaultValues()
        {
            foreach (var control in UiSelect.Where(x => x.WasUserSet == false))
            {
                control.SetDefaultValue(false);
                ApplyDefaultDependentValues(control);
            }
        }

        internal void ApplyDefaultDependentValues(UIOption control)
        {
            if (control.DependentControls?.Count > 0)
            {
                foreach (var depControl in control.DependentControls.Where(x => x.WasUserSet == false))
                {
                    depControl.SetDefaultValue(false);
                    ApplyDefaultDependentValues(depControl);
                }
            }
        }

        internal void SetCanApplyDefaultValues(bool canApplyDefaultValues)
        {
            foreach (var control in UiSelect)
            {
                control.CanSetDefaultValues = canApplyDefaultValues;
                SetCanApplyDefaultDependentValues(canApplyDefaultValues, control);
            }
        }

        private void SetCanApplyDefaultDependentValues(bool canApplyDefaultValues, UIOption control)
        {
            if (control.DependentControls?.Count > 0)
            {
                foreach (var depControl in control.DependentControls)
                {
                    depControl.CanSetDefaultValues = canApplyDefaultValues;
                    SetCanApplyDefaultDependentValues(canApplyDefaultValues, depControl);
                }
            }
        }

        internal void SetMandatoryOptions(DependencyVariablesManager dependencyVariablesManager)
        {
            foreach (var control in UiSelect)
            {
                if (control.IsSelectionTypeControl && (Equals(string.Empty, control.SelectedValue) || control.SelectedValue is null) && control.UISelectOptions.Count == 1)
                {
                    if (control.ValidationErrors.Count == 1 && control.ValidationErrors.First().Message.Contains("must be selected"))
                    {
                        control.SetSingleMandatoryValue();
                    }
                }
            }
        }

        internal void SetAllControlsToWithinValidationProcedure(bool value)
        {
            foreach (var control in UiSelect)
            {
                control.IsWithinParentValidationProcedure = value;
                SetAllControlsToWithinValidationProcedureForDependentControls(value, control);
            }
        }

        private void SetAllControlsToWithinValidationProcedureForDependentControls(bool value, UIOption control)
        {
            foreach (var depControl in control.DependentControls)
            {
                depControl.IsWithinParentValidationProcedure = value;
                SetAllControlsToWithinValidationProcedureForDependentControls(value, depControl);
            }
        }

        internal void ResetUserSetStatus(string controlNameToExclude = "")
        {
            foreach (var control in UiSelect)
            {
                if (control.ValueName.Equals(controlNameToExclude))
                {
                    ResetUserSetStatusForDependentControls(controlNameToExclude, control);
                    continue;
                }
                control.UserChangedControlName = controlNameToExclude;
                control.WasUserSet = false;
                ResetUserSetStatusForDependentControls(controlNameToExclude, control);
            }
        }

        internal void ResetUserSetStatus(List<string> controlNamesToExclude, string initiatingControl)
        {
            foreach (var control in UiSelect)
            {
                if (controlNamesToExclude.Contains(control.ValueName))
                {
                    ResetUserSetStatusForDependentControls(control.ValueName, control);
                    continue;
                }
                control.UserChangedControlName = initiatingControl;
                control.WasUserSet = false;
                ResetUserSetStatusForDependentControls(control.ValueName, control);
            }
        }

        private void ResetUserSetStatusForDependentControls(string controlNameToExclude, UIOption control)
        {
            foreach (var depControl in control.DependentControls)
            {
                if (depControl.ValueName.Equals(controlNameToExclude))
                {
                    ResetUserSetStatusForDependentControls(controlNameToExclude, depControl);
                    continue;
                }
                depControl.UserChangedControlName = controlNameToExclude;
                depControl.WasUserSet = false;
                ResetUserSetStatusForDependentControls(controlNameToExclude, depControl);
            }
        }

        internal void SetInitializationState(bool initializationState)
        {
            foreach (var control in UiSelect)
            {
                control.IsInitializing = initializationState;
                foreach (var depControl in control.DependentControls)
                {
                    depControl.IsInitializing = initializationState;
                }
            }
        }

        internal void ResetAllControlsStatusToDefault()
        {
            foreach (var control in UiSelect)
            {
                control.Status = UIControlStatusEnumerations.Default;
                ResetAllDependentControlsStatusToDefault(control);
            }
        }

        private void ResetAllDependentControlsStatusToDefault(UIOption control)
        {
            foreach (var depControl in control.DependentControls)
            {
                depControl.Status = UIControlStatusEnumerations.Default;
                ResetAllDependentControlsStatusToDefault(depControl);
            }
        }

        internal void SortControls()
        {
            UiSelect = UiSelect.OrderBy(o => o.StepID).ToList();
            foreach (var uiOption in UiSelect.Where(u => u.HasDependentControls))
            {
                uiOption.DependentControls = uiOption.DependentControls.OrderBy(o => o.StepID).ToList();
                SortDependentControls(uiOption);
            }
        }

        private void SortDependentControls(UIOption control)
        {
            foreach (var depControl in control.DependentControls)
            {
                depControl.DependentControls = depControl.DependentControls.OrderBy(o => o.StepID).ToList();
                SortDependentControls(depControl);
            }
        }

        internal List<UIOption> GetNonVariableDimControls()
        {
            var parentControls = UiSelect.Where(c => c.VariableDim == false);
            var nonVarControls = new List<UIOption>();
            if (parentControls?.Any() == true) nonVarControls.AddRange(parentControls);
            foreach (var uiOption in UiSelect.Where(c => c.HasDependentControls))
            {
                var depControls = uiOption.DependentControls.Where(c => c.VariableDim == false);
                if (depControls?.Any() == true) nonVarControls.AddRange(depControls);
            }

            return nonVarControls;
        }

        internal List<UIOption> GetNonVariableDimPricingRules(List<string> nonVarDimNames)
        {
            var nonVarPricingControls = new List<UIOption>();
            var parentControls = UiSelect.Where(c => GetNonVariableDimPricingRulesFromControl(c, nonVarDimNames)?.Any() == true);
            if (parentControls?.Any() == true) nonVarPricingControls.AddRange(parentControls);
            foreach (var uiOption in UiSelect.Where(c => c.HasDependentControls))
            {
                var depControls = uiOption.DependentControls.Where(c => GetNonVariableDimPricingRulesFromControl(c, nonVarDimNames)?.Any() == true);
                if (depControls?.Any() == true) nonVarPricingControls.AddRange(depControls);
            }

            return nonVarPricingControls;
        }

        private IEnumerable<PriceMatrixEngine> GetNonVariableDimPricingRulesFromControl(UIOption control, List<string> nonVarDimNames)
        {
            return control.PriceMatrices.Where(p =>
                (p.PriceColumnIndexes.Where(i =>
                    nonVarDimNames.Contains(i.LookupValueName) || nonVarDimNames.Any(s => i.LookupJsonRule?.Contains(s) == true))?.Any() == true ||
                p.PriceRowIndexes.Where(i =>
                    nonVarDimNames.Contains(i.LookupValueName) || nonVarDimNames.Any(s => i.LookupJsonRule?.Contains(s) == true))?.Any() == true) && p.PricingMethod == "B");
        }

        internal void PropagateSom(string som)
        {
            foreach (var uiOption in UiSelect)
            {
                uiOption.SystemOfMeasure = som;
            }
        }

        internal void AddCustomVariant(CustomVariantDto customVariantDto)
        {
            var nextStepId = 1;
            var customVariants = UiSelect.Where(x => x.UIControlType == UiControlTypeEnumeration.CustomVariant.DisplayName);
            if (customVariants != null)
                nextStepId = customVariants.Count() + 1;

            customVariantDto.Step = nextStepId;

            var customOption = new UIOption
            {
                PriceAdder = 0,
                StepID = UiSelect.Max(x => x.StepID) + 1,
                ValueName = GetCustomControlName(nextStepId),
                UIDescription = customVariantDto.Name,
                PriceMultiplier = 0,
                UIControlType = UiControlTypeEnumeration.CustomVariant.DisplayName,
                Level = 0,
                ValueType = ValueTypeEnumeration.NotApplicable,
                CustomVarName = customVariantDto.Name,
                CustomVarValue = customVariantDto.Value,
                CustomVarQuantity = customVariantDto.Quantity,
                CustomVarAmount = customVariantDto.Amount,
                CustomVarOtherInformation = customVariantDto.OtherInformation,
                ShowControl = true,
                SelectedValue = customVariantDto.Value,
                DefaultOption = customVariantDto.Value,
                IsSelected = true
            };
            UiSelect.Add(customOption);
            AddControlToIndex(customOption.ValueName, customOption.UIControlType, customOption.StepID);
        }

        internal void AddControl(UIOption newControl)
        {
            newControl.StepID = UiSelect.Max(x => x.StepID) + 1;
            UiSelect.Add(newControl);
            AddControlToIndex(newControl.ValueName, newControl.UIControlType, newControl.StepID);
        }

        internal void RemoveControl(string controlName)
        {
            var control = FindControlByName(controlName);
            if (control is null) return;
            UiSelect.Remove(control);
            RemoveControlFromIndex(control.ValueName, control.UIControlType, control.StepID);
        }

        internal void UpdateCustomVariant(string valueName, CustomVariantDto customVariantDto)
        {
            var control = FindControlByName(valueName);
            if (control == null) return;

            control.CustomVarName = customVariantDto.Name;
            control.CustomVarValue = customVariantDto.Value;
            control.CustomVarQuantity = customVariantDto.Quantity;
            control.CustomVarAmount = customVariantDto.Amount;
            control.CustomVarOtherInformation = customVariantDto.OtherInformation;
            control.SelectedValue = customVariantDto.Value;
            control.DefaultOption = customVariantDto.Value;
        }

        internal void DeleteCustomVariant(string valueName)
        {
            var control = FindControlByName(valueName);
            if (control == null) return;

            if (!control.UIControlType.Equals(UiControlTypeEnumeration.CustomVariant.DisplayName)) return;

            UiSelect.Remove(control);
            RemoveControlFromIndex(control.ValueName, control.UIControlType, control.StepID);
        }

        internal UIOption AddBasePriceVariant()
        {
            var basePriceControl = new UIOption
            {
                ValueName = SpecialBasePriceControl,
                UIDescription = "Base Price",
                UIControlType = UiControlTypeEnumeration.DecimalBox.DisplayName,
                Level = 0,
                StepID = UiSelect.Max(x => x.StepID) + 1,
                ShowControl = true
            };

            UiSelect.Add(basePriceControl);
            AddControlToIndex(basePriceControl.ValueName, basePriceControl.UIControlType, basePriceControl.StepID);

            return basePriceControl;
        }

        internal void RemoveBasePriceVariant()
        {
            var control = FindControlByName(SpecialBasePriceControl);
            if (control == null) return;

            UiSelect.Remove(control);
            RemoveControlFromIndex(control.ValueName, control.UIControlType, control.StepID);
        }

        internal bool HasBasePriceVariant()
        {
            var control = FindControlByName(SpecialBasePriceControl);
            return (control is not null);
        }

        internal bool HasSpecialBasePriceValue()
        {
            var control = FindControlByName(SpecialBasePriceControl);
            if (control == null) return false;

            return (double.TryParse(control.SelectedValue?.ToString(), out var basePriceValue) && basePriceValue > 0);
        }

        private void Cleanup()
        {
            foreach (var uiOption in UiSelect)
            {
                uiOption.Dispose();
            }
            UiSelect?.Clear();
            _unitControlsIndex?.Clear();
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                Cleanup();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _isDisposed = true;
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ControlsManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}