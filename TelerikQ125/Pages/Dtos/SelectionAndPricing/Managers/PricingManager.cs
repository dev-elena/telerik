using Microsoft.Extensions.Logging;


using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static telerik_Q1_25.Pages.Dtos.Class;
using TelerikQ125.Pages.Dtos;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public sealed class PricingManager
    {
        private ILogger _logger;
        private bool _logInformation;

        internal bool CanUserSeePricing { get; set; }

        public PricingManager(bool canUserSeePricing = true)
        {
            CanUserSeePricing = canUserSeePricing;
        }

        /// <summary>
        /// Pricing variables housing the results from running the pricing process.
        /// </summary>
        public Dictionary<PriceTypeEnumeration, double?> PricingValues { get; } = new();

        public List<PricingVariant> PricingVariants { get; set; } = new();

        internal bool InPricingProcedure { get; set; }

        internal DependencyVariablesManager DependencyVariablesManager { get; set; }

        /// <summary>
        /// Run the pricing processes for unit and all its options.
        /// </summary>
        /// <remarks>This process is automatically run each time a value is set for an option.</remarks>
        internal void CalculatePricing(ControlsManager controlsManager, SizingLimits overallMaxMin, double defaultModelMultiplier, ILogger logger = null, bool loginformation = false)
        {
            if (DependencyVariablesManager is null) return;
            _logger = logger;
            _logInformation = loginformation;

            InPricingProcedure = true;

            ClearPricingVariables(defaultModelMultiplier, false); // Do not raise the change event until after this method is complete

            // Check for any invalid dimension values for any non-variable dim options
            if (HandleNonVariableDimensions(controlsManager))
            {
                InPricingProcedure = false;
                return; // Cannot continue if there are invalid dimension values entered
            }

            var basePriceControls = new List<string>();

            // Determine if the unit is an accessory item that might not have any options for the user to select,
            // Which could result in no default being set during testing and no pricing being run
            // So we want to force the pricing rules to run if the unit only has one option, even if the option is null
            var hasOnlyOneAccessory = controlsManager.FullControlList.Count == 1;

            foreach (var option in controlsManager.UiSelect?.Where(o => o.PriceMatrices?.Count > 0 || o.UIControlType == UiControlTypeEnumeration.CustomVariant.DisplayName))
            {
                //todo: delete time tracking
                Stopwatch oneOption = new();
                oneOption.Restart();

                var tmpVars = new Dictionary<string, object>();
                foreach (var dependencyVariable in DependencyVariablesManager.DependencyVariables)
                {
                    tmpVars[dependencyVariable.Key] = dependencyVariable.Value;
                }

                var priceValues = option.GetOptionPriceValues(DependencyVariablesManager.DependencyVariables, hasOnlyOneAccessory || option.UIControlType.Equals(UiControlTypeEnumeration.NotApplicable.DisplayName));
                if (priceValues?.Length > 0)
                {
                    UpdatePriceVariables(priceValues, option.ValueName, defaultModelMultiplier);
                    if (priceValues.Any(p => p.PricingMethodCode.ToUpper() == "B")) basePriceControls.Add(option.ValueName);
                }

                var diffs = DependencyVariablesManager.DependencyVariables.Except(tmpVars);
                if (diffs?.Any() == true)
                {
                    foreach (var diffValue in diffs)
                    {
                        FinishManager.SetFinishPricingForControl(option, diffValue.Key, diffValue.Value);
                    }
                }

                //add custom variant price as add-on price
                if (option.UIControlType == UiControlTypeEnumeration.CustomVariant.DisplayName)
                {
                    var priceMatrixValues = new List<PriceMatrixVal> { new PriceMatrixVal { MatrixResult = option.CustomVarAmount * option.CustomVarQuantity, PricingMethodCode = "AB" } };
                    UpdatePriceVariables(priceMatrixValues, option.ValueName, defaultModelMultiplier);
                }

                oneOption.Stop();
                _logger?.LogInformation($"Timing - Pricing for {option.UIDescription}: priceValues.Count = {priceValues.Length}, time = {oneOption.Elapsed.Milliseconds}");
            }

            if (overallMaxMin is not null && !string.IsNullOrWhiteSpace(overallMaxMin.MultiSectionPriceTypeCode) && DependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.NumberOfSections.DisplayName, out var numSects))
            {
                if (numSects is not null && Convert.ToInt32(numSects) > 1)
                {
                    UpdatePriceVariables(new List<PriceMatrixVal>() { new() { MatrixResult = overallMaxMin.MultiSectionPriceValue.ToUSCultureDouble(), PricingMethodCode = overallMaxMin.MultiSectionPriceTypeCode } }, overallMaxMin.SizingLimitName, defaultModelMultiplier);
                }
            }

            PricingValues.TryGetValue(PriceTypeEnumeration.BasePrice, out var basePriceValue);
            PricingValues.TryGetValue(PriceTypeEnumeration.BasePriceGroup2, out var acceBasePriceValue);
            PricingValues.TryGetValue(PriceTypeEnumeration.UnitList, out var unitListValue);
            PricingValues.TryGetValue(PriceTypeEnumeration.ItemMultiplier, out var itemMultiplierValue);
            PricingValues.TryGetValue(PriceTypeEnumeration.ItemMultiplierGroup2, out var acceMultiplierValue);
            PricingValues.TryGetValue(PriceTypeEnumeration.AddOnCharge, out var addOnChargeValue);

            if (basePriceValue is null)
            {
                // Provide the user with a notice that the pricing for this configuration is not available, most likely due to an invalid configuration that the rules don't exist in the current system for
                const string message = "No price found for this configuration. Please refer to the latest list price schedule to make the proper adjustments.";
                if (basePriceControls.Count > 0)
                {
                    foreach (var controlStr in basePriceControls)
                    {
                        var control = controlsManager.FindControlByName(controlStr);

                        if (control != null)
                            control?.ValidationErrors.Add(new InvalidOption() { ControlName = controlStr, ControlUIDescription = control.UIDescription, Message = message });
                    }
                }
                else
                {
                    // Add the error to the first control in the list
                    var control = controlsManager.UiSelect?.Count > 0 ? controlsManager.UiSelect[0] : null;
                    if (control != null)
                        control?.ValidationErrors.Add(new InvalidOption() { ControlName = control.ValueName, ControlUIDescription = control.UIDescription, Message = message });
                }
                InPricingProcedure = false;

                return; // Cannot continue if there is no base price to work with.
            }

            unitListValue = basePriceValue * (itemMultiplierValue ?? 1);

            foreach (var ab in PricingValues.Where(p => p.Key.DisplayName.Contains("-accessorybase", StringComparison.OrdinalIgnoreCase)))
            {
                addOnChargeValue += ab.Value ?? 0;
            }
            foreach (var ab in PricingValues.Where(p => p.Key.DisplayName.Contains("-addtobase", StringComparison.OrdinalIgnoreCase)))
            {
                addOnChargeValue += ab.Value ?? 0;
            }

            // Handle group 2 pricing now
            if (acceBasePriceValue is not null || PricingValues.Any(p => p.Key.DisplayName.EndsWith("G2", StringComparison.OrdinalIgnoreCase)))
            {
                if ((acceMultiplierValue ?? 1) == 0)
                {
                    acceMultiplierValue = 1;
                } //check if null or 0, 0110g Guage=22G gives grp2multipler of 0
                var group2base = (acceBasePriceValue ?? 0) * acceMultiplierValue;

                //var group2base = (acceBasePriceValue ?? 0) * (acceMultiplierValue ?? 1);
                var group2Addon = 0d;
                foreach (var ab in PricingValues.Where(p => p.Key.DisplayName.EndsWith("G2", StringComparison.OrdinalIgnoreCase)))
                {
                    if(!ab.Key.DisplayName.ToString().ToLower().Contains("multiplier")) group2Addon += ab.Value ?? 0;
                }
                addOnChargeValue += group2base + group2Addon;
            }
            unitListValue += addOnChargeValue ?? 0;

            PricingValues[PriceTypeEnumeration.BasePrice] = basePriceValue?.ToPrecision(2);
            PricingValues[PriceTypeEnumeration.UnitList] = unitListValue?.ToPrecision(2);
            PricingValues[PriceTypeEnumeration.ItemMultiplier] = itemMultiplierValue?.ToPrecision(2);
            PricingValues[PriceTypeEnumeration.AddOnCharge] = addOnChargeValue?.ToPrecision(2);

            // Add minimum and setup charges
            var paintMinCharge = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.PaintMinCharge.DisplayName);
            var paintSetupCharge = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.PaintSetupCharge.DisplayName);
            var groupMinCharge = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.GroupMinCharge.DisplayName);
            var finishCharge = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.FinishCharge.DisplayName);
            var finishChargePrime = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.FinishChargePrime.DisplayName);
            var finishSfPrime = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.FinishSFPrime.DisplayName);
            var paintSetupChargePrime = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.PaintSetupChargePrime.DisplayName);
            var finishChargeSecond = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.FinishChargeSecond.DisplayName);
            var finishSfSecond = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.FinishSFSecond.DisplayName);
            var paintSetupChargeSecond = DependencyVariablesManager.GetValueFromDependencyVariable(PriceTypeEnumeration.PaintSetupChargeSecond.DisplayName);

            if (paintMinCharge is not null && double.TryParse(paintMinCharge.ToString(), out var paintMinChargeValue))
            {
                PricingValues[PriceTypeEnumeration.PaintMinCharge] = paintMinChargeValue.ToPrecision(2);
            }
            if (paintSetupCharge is not null && double.TryParse(paintSetupCharge.ToString(), out var paintSetupChargeValue))
            {
                PricingValues[PriceTypeEnumeration.PaintSetupCharge] = paintSetupChargeValue.ToPrecision(2);
            }
            if (groupMinCharge is not null && double.TryParse(groupMinCharge.ToString(), out var groupMinChargeValue))
            {
                PricingValues[PriceTypeEnumeration.GroupMinCharge] = groupMinChargeValue.ToPrecision(2);
            }
            if (finishCharge is not null && double.TryParse(finishCharge.ToString(), out var finishChargeValue))
            {
                PricingValues[PriceTypeEnumeration.FinishCharge] = finishChargeValue.ToPrecision(2);
            }
            if (finishChargePrime is not null && double.TryParse(finishChargePrime.ToString(), out var finishChargePrimeValue))
            {
                PricingValues[PriceTypeEnumeration.FinishChargePrime] = finishChargePrimeValue.ToPrecision(2);
            }
            if (finishSfPrime is not null && double.TryParse(finishSfPrime.ToString(), out var finishSfPrimeValue))
            {
                PricingValues[PriceTypeEnumeration.FinishSFPrime] = finishSfPrimeValue.ToPrecision(2);
            }
            if (paintSetupChargePrime is not null && double.TryParse(paintSetupChargePrime.ToString(), out var paintSetupChargePrimeValue))
            {
                PricingValues[PriceTypeEnumeration.PaintSetupChargePrime] = paintSetupChargePrimeValue.ToPrecision(2);
            }
            if (finishChargeSecond is not null && double.TryParse(finishChargeSecond.ToString(), out var finishChargeSecondValue))
            {
                PricingValues[PriceTypeEnumeration.FinishChargeSecond] = finishChargeSecondValue.ToPrecision(2);
            }
            if (finishSfSecond is not null && double.TryParse(finishSfSecond.ToString(), out var finishSfSecondValue))
            {
                PricingValues[PriceTypeEnumeration.FinishSFSecond] = finishSfSecondValue.ToPrecision(2);
            }
            if (paintSetupChargeSecond is not null && double.TryParse(paintSetupChargeSecond.ToString(), out var paintSetupChargeSecondValue))
            {
                PricingValues[PriceTypeEnumeration.PaintSetupChargeSecond] = paintSetupChargeSecondValue.ToPrecision(2);
            }

            // For engineering roles we do not allow pricing to be shown, so just don't notify the UI of the prices
            if (!CanUserSeePricing)
            {
                InPricingProcedure = false;
                return;
            }

            PricingVariablesChanged(new PricingChangedEventArgs { PricingVariables = PricingValues });
            InPricingProcedure = false;
        }

        /// <summary>
        /// Calculates price for regular item with IsSpecialOverride = true (does not apply for special model)
        /// </summary>
        /// <param name="controlsManager"></param>
        /// <returns>true is price was calculated</returns>
        internal bool CalculatePricingForSpecialOverride(ControlsManager controlsManager)
        {
            LogInformation($"Entered {nameof(CalculatePricingForSpecialOverride)}");
            var basePriceControl = controlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
            if (basePriceControl != null && basePriceControl.SelectedValue != null)
            {
                double basePrice = 0;
                if (basePriceControl.SelectedValue.GetType() == typeof(double) || basePriceControl.SelectedValue.GetType() == typeof(decimal) || basePriceControl.SelectedValue.GetType() == typeof(float) || basePriceControl.SelectedValue.GetType() == typeof(Int16) || basePriceControl.SelectedValue.GetType() == typeof(int) || basePriceControl.SelectedValue.GetType() == typeof(long))
                {
                    basePrice = basePriceControl.SelectedValue.ToUSCultureDouble();
                }
                else if (basePriceControl.SelectedValue.GetType() == typeof(string))
                {
                    _ = double.TryParse(basePriceControl.SelectedValue.ToString(), out basePrice);
                }

                PricingValues[PriceTypeEnumeration.BasePrice] = basePrice;
                PricingValues[PriceTypeEnumeration.UnitList] = basePrice;

                if (basePrice == 0)
                {
                    basePriceControl.Status = UIControlStatusEnumerations.Error;
                    return false;
                }
                basePriceControl.Status = UIControlStatusEnumerations.Default;

                if (!CanUserSeePricing)
                {
                    InPricingProcedure = false;
                    return true;
                }
                PricingVariablesChanged(new PricingChangedEventArgs { PricingVariables = PricingValues });
                InPricingProcedure = false;
                return true;
            }
            if (basePriceControl is not null) basePriceControl.Status = UIControlStatusEnumerations.Error;
            return false;
        }

        internal bool HandleNonVariableDimensions(ControlsManager controlsManager)
        {
            // Check if there are any non-variable dimension controls that need to be validated against pricing row/column values
            var nonVarDims = controlsManager.GetNonVariableDimControls();
            if (nonVarDims?.Count > 0)
            {
                var namesList = nonVarDims.Select(o => o.ValueName).ToList();
                //var pricingOpts = controlsManager.UiSelect.Where(c => c.ValidationJsonRules.Where(r => r.ToLower().Contains("vardim-"))?.Any() == true);
                var pricingOpts = controlsManager.GetNonVariableDimPricingRules(namesList);
                foreach (var pricingOpt in pricingOpts)
                {
                    var dimValues = pricingOpt.GetNonVariableDimValues(DependencyVariablesManager.DependencyVariables, namesList);
                    if (dimValues.Count <= 0) continue;

                    var sb = new StringBuilder();
                    foreach (var dimValue in dimValues.Where(v => namesList.Contains(v.DimensionName)))
                    {
                        sb.Clear();
                        sb.Append("Only the following dimensions are permitted:\n");
                        var nonVarDim = nonVarDims.First(o => o.ValueName == dimValue.DimensionName);
                        if (!double.TryParse(nonVarDim.SelectedValue?.ToString(), out var enteredValue)) continue;

                        if (dimValue.SingleArrayValues?.Count > 0 && !dimValue.SingleArrayValues.Contains(enteredValue))
                        {
                            sb.Append($"{nonVarDim.ValueName} {dimValue.ValuesString}\n");
                            sb.Append("\nPlease contact the factory for more information.");
                            nonVarDim.ValidationErrors.Add(new InvalidOption() { ControlName = nonVarDim.ValueName, ControlUIDescription = nonVarDim.UIDescription, Message = sb.ToString() });
                        }
                        else if (dimValue.MultiArrayValues?.Count > 0)
                        {
                            sb.Append($"{nonVarDim.ValueName}\n");
                            if (dimValue.MultiArrayValues.Any(multiArrayValue => !multiArrayValue.Contains(enteredValue)))
                            {
                                sb.Append($"{nonVarDim.ValueName} {dimValue.ValuesString}\n");
                            }
                            sb.Append("\nPlease contact the factory for more information.");
                            nonVarDim.ValidationErrors.Add(new InvalidOption() { ControlName = nonVarDim.ValueName, ControlUIDescription = nonVarDim.UIDescription, Message = sb.ToString() });
                        }
                    }
                }

                if (nonVarDims.Any(n => n.ValidationErrors.Count > 0))
                {
                    return true;
                }
            }

            return false;
        }

        internal void ClearPricingVariables(double defaultModelMultiplier, bool raiseEvent = true)
        {
            DependencyVariablesManager?.ClearPricingVariables(PricingValues);
            PricingValues.Clear();
            PricingVariants.Clear();
            PricingValues.Add(PriceTypeEnumeration.UnitList, null);
            PricingValues.Add(PriceTypeEnumeration.BasePrice, null);
            PricingValues.Add(PriceTypeEnumeration.AddOnCharge, 0.0D);
            PricingValues.Add(PriceTypeEnumeration.ItemMultiplier, defaultModelMultiplier);
            PricingValues.Add(PriceTypeEnumeration.PaintMinCharge, null);
            PricingValues.Add(PriceTypeEnumeration.PaintSetupCharge, null);
            PricingValues.Add(PriceTypeEnumeration.GroupMinCharge, null);
            PricingValues.Add(PriceTypeEnumeration.FinishCharge, null);
            PricingValues.Add(PriceTypeEnumeration.ListAddOn, null);
            PricingValues.Add(PriceTypeEnumeration.FinishChargePrime, null);
            PricingValues.Add(PriceTypeEnumeration.FinishSFPrime, null);
            PricingValues.Add(PriceTypeEnumeration.PaintSetupChargePrime, null);
            PricingValues.Add(PriceTypeEnumeration.FinishChargeSecond, null);
            PricingValues.Add(PriceTypeEnumeration.FinishSFSecond, null);
            PricingValues.Add(PriceTypeEnumeration.PaintSetupChargeSecond, null);
            PricingValues.Add(PriceTypeEnumeration.ItemMultiplierGroup2, 0d);
            PricingValues.Add(PriceTypeEnumeration.BasePriceGroup2, null);

            //if (raiseEvent) PricingVariablesChanged(new PricingChangedEventArgs { PricingVariables = PricingValues });
            PricingVariablesChanged(new PricingChangedEventArgs { PricingVariables = PricingValues, RaiseEventFurther = raiseEvent });
        }

        private void UpdatePriceVariables(IEnumerable<PriceMatrixVal> priceMatrixValues, string optionValueName, double defaultModelMultiplier)
        {
            var g1ModelMultResult = 0d;
            var g2ModelMultResult = 0d;
            var setg1ModelMult = false;
            var setg2ModelMult = false;
            foreach (var priceValue in priceMatrixValues)
            {
                var priceGroup = "";
                if (priceValue.PricingMethodCode?.StartsWith("G2") == true)
                {
                    priceGroup = "G2";
                    priceValue.PricingMethodCode = priceValue.PricingMethodCode.Replace("G2", "");
                }
                PricingVariants.Add(new PricingVariant { PricingCode = priceValue.PricingMethodCode, VariantName = optionValueName, PricingValue = priceValue.MatrixResult ?? 0.0 });
                switch (priceValue.PricingMethodCode)
                {
                    case "A": // Add to the base price
                    case "C": // Add on charge per list
                    case "D": // Add on charge per section
                    case "E": // Add on charge per foot
                    case "F": // Add on charge per circumference
                    case "K": // Add on charge per sqft
                        // commented out since we can just use -accessorybase for all these instead, making it easier to work with
                        //if (priceValue.MatrixResult is not null)
                        //{
                        //    LogInformation($"Adding {optionValueName}-addtobase{priceGroup} to pricing values.", LogLevel.Warning);
                        //    PricingValues.Add(new PriceTypeEnumeration(PricingValues.Count, $"{optionValueName}-addtobase{priceGroup}"), priceValue.MatrixResult);
                        //}
                        //break;
                    case "AB": // Accessory base price
                        if (priceValue.MatrixResult is not null)
                        {
                            LogInformation($"Adding {optionValueName}-accessorybase{priceGroup} to pricing values.", LogLevel.Warning);
                            PricingValues.Add(new PriceTypeEnumeration(PricingValues.Count, $"{optionValueName}-accessorybase{priceGroup}"), priceValue.MatrixResult);
                        }
                        break;
                    case "B":
                        // Base price
                        UpdateBasePrice(priceValue.MatrixResult, priceGroup);
                        break;
                    case "G": // Base price per foot
                    case "J": // Base price per linear inch (w+h)
                    case "L": // List price multiplier
                    case "S": // Setup charge other than paint setup charge
                    case "V": // Variable
                        LogInformation($"Adding {priceValue.MatrixResult} to pricing values as {priceValue.PricingMethodCode}{priceGroup}.", LogLevel.Warning);
                        PricingValues.Add(new PriceTypeEnumeration(PricingValues.Count, $"{optionValueName}-Unknown price type: {priceValue.PricingMethodCode}{priceGroup}"), priceValue.MatrixResult);
                        break;
                    case "BM":
                        // Special one created to handle Base Price Mult from price table 7
                        if (!string.IsNullOrWhiteSpace(priceGroup))
                        {
                            LogInformation($"Multiplying accessory base of {PricingValues[PriceTypeEnumeration.BasePriceGroup2]} by {(1 + priceValue.MatrixResult ?? 0)}.", LogLevel.Warning);
                            PricingValues[PriceTypeEnumeration.BasePriceGroup2] *= (1 + priceValue.MatrixResult ?? 0);
                        }
                        else
                        {
                            LogInformation($"Multiplying model base of {PricingValues[PriceTypeEnumeration.BasePrice]} by {(1 + priceValue.MatrixResult ?? 0)}.", LogLevel.Warning);
                            PricingValues[PriceTypeEnumeration.BasePrice] *= (1 + priceValue.MatrixResult ?? 0);
                        }
                        break;
                    case "M":
                        // Add to multiplier
                        if (!string.IsNullOrWhiteSpace(priceGroup))
                        {
                            LogInformation($"Adding {priceValue.MatrixResult ?? 0} to the accessory base multiplier of {PricingValues[PriceTypeEnumeration.ItemMultiplierGroup2]}.", LogLevel.Warning);
                            PricingValues[PriceTypeEnumeration.ItemMultiplierGroup2] += priceValue.MatrixResult ?? 0;
                        }
                        else
                        {
                            LogInformation($"Adding {priceValue.MatrixResult ?? 0} to the model base multiplier of {PricingValues[PriceTypeEnumeration.ItemMultiplier]}.", LogLevel.Warning);
                            PricingValues[PriceTypeEnumeration.ItemMultiplier] += priceValue.MatrixResult ?? 0;
                        }
                        // Add this just for reporting purposes - this is not used in the pricing calculation since it is already handled above
                        PricingValues.Add(new PriceTypeEnumeration(PricingValues.Count, $"{optionValueName}-multiplier{priceGroup}"), priceValue.MatrixResult);
                        break;
                    case "N":
                        // Model multiplier
                        if (!string.IsNullOrWhiteSpace(priceGroup))
                        {
                            setg2ModelMult = true;
                            g2ModelMultResult += priceValue.MatrixResult ?? 0;
                            //PricingValues[PriceTypeEnumeration.ItemMultiplierGroup2] = priceValue.MatrixResult ?? defaultModelMultiplier;
                        }
                        else
                        {
                            setg1ModelMult = true;
                            g1ModelMultResult += priceValue.MatrixResult ?? 0;
                            //PricingValues[PriceTypeEnumeration.ItemMultiplier] = priceValue.MatrixResult ?? defaultModelMultiplier;
                        }
                        break;
                    case "H":
                        // Minimum billing
                        LogInformation($"Adding {priceValue.MatrixResult} to {optionValueName}-minimumbilling{priceGroup}.", LogLevel.Warning);
                        PricingValues.Add(new PriceTypeEnumeration(PricingValues.Count, $"{optionValueName}-minimumbilling{priceGroup}"), priceValue.MatrixResult);
                        break;
                    case "NA":
                        // Not applicable
                        break;
                }
            }
            if (setg1ModelMult)
            {
                if (g1ModelMultResult > 0)
                {
                    LogInformation($"Setting {PriceTypeEnumeration.ItemMultiplier.DisplayName} to {g1ModelMultResult}.", LogLevel.Warning);
                    PricingValues[PriceTypeEnumeration.ItemMultiplier] = g1ModelMultResult;
                }
                else
                {
                    LogInformation($"Setting {PriceTypeEnumeration.ItemMultiplier.DisplayName} to {defaultModelMultiplier}.", LogLevel.Warning);
                    PricingValues[PriceTypeEnumeration.ItemMultiplier] = defaultModelMultiplier;
                }
            }
            if (setg2ModelMult)
            {
                if (g2ModelMultResult > 0)
                {
                    LogInformation($"Setting {PriceTypeEnumeration.ItemMultiplierGroup2.DisplayName} to {g2ModelMultResult}.", LogLevel.Warning);
                    PricingValues[PriceTypeEnumeration.ItemMultiplierGroup2] = g2ModelMultResult;
                }
                else
                {
                    LogInformation($"Setting {PriceTypeEnumeration.ItemMultiplierGroup2.DisplayName} to {defaultModelMultiplier}.", LogLevel.Warning);
                    PricingValues[PriceTypeEnumeration.ItemMultiplierGroup2] = defaultModelMultiplier;
                }
            }
        }

        private void UpdateBasePrice(double? priceValue, string priceGroup)
        {
            PriceTypeEnumeration baseType = PriceTypeEnumeration.BasePrice;
            if (!string.IsNullOrWhiteSpace(priceGroup))
            {
                baseType = PriceTypeEnumeration.BasePriceGroup2;
            }

            LogInformation($"Adding {(priceValue ?? 0d)} to base price ({baseType.DisplayName}).", LogLevel.Warning);
            if (PricingValues[baseType] is not null)
                PricingValues[baseType] += priceValue ?? 0d;
            else
                PricingValues[baseType] = priceValue;
        }

        private void LogInformation(string message, LogLevel logLevel = LogLevel.Information)
        {
            if (_logInformation)
            {
                _logger?.Log(logLevel, message);
            }
        }

        private void PricingVariablesChanged(PricingChangedEventArgs e) => PricingVariablesChangedEvent?.Invoke(this, e);

        public event EventHandler<PricingChangedEventArgs> PricingVariablesChangedEvent;
    }
}