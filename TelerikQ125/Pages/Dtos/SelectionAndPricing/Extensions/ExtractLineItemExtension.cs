
using System;
using System.Collections.Generic;
using System.Linq;
using TelerikQ125.Pages.Dtos;
using static telerik_Q1_25.Pages.Dtos.Class;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class ExtractLineItemExtension
    {
        public static ProjectLineItemDisplayDto ExtractProjectLineItemDisplayDtoEx(this UnitSelectionAndPricingEngine engine, ProjectLineItemDisplayDto currentDto, bool isLight = false)
        {
            currentDto ??= new ProjectLineItemDisplayDto
            {
                ItemId = Guid.Empty // Make it an empty guid so the client knows this is a new item and not an edited item
            };

            currentDto.LineNumber = engine.LineNumber;
            currentDto.Quantity = engine.Quantity;
            currentDto.Model = engine.Model;
            currentDto.ModelDescription = engine.ModelDescription;
            engine.CleanDimensions();
            currentDto.Dimensions = engine.GetDimensionsText();
            currentDto.Sections = engine.DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionText.DisplayName)?.ToString();
            currentDto.Variances = engine.UiOptionsSelectedValuesAsString;
            if (!isLight)
            {
                currentDto.UnitList = engine.PricingManager.PricingValues.FirstOrDefault(k => k.Key == PriceTypeEnumeration.UnitList).Value ?? 0;
                currentDto.BasePrice = engine.PricingManager.PricingValues.FirstOrDefault(k => k.Key == PriceTypeEnumeration.BasePrice).Value ?? 0;
                currentDto.AddOnCharge = engine.PricingManager.PricingValues.FirstOrDefault(k => k.Key == PriceTypeEnumeration.AddOnCharge).Value ?? 0;
                currentDto.ModelMultiplier = engine.PricingManager.PricingValues.FirstOrDefault(k => k.Key == PriceTypeEnumeration.ItemMultiplier).Value ?? 0;
                currentDto.CalculatedPricingVariants = new List<PricingVariant>(engine.PricingManager.PricingVariants.Where(v => v.PricingValue > 0d));
                currentDto.PricingVersionNumber = engine.PricingVersionNumber;
            }
            currentDto.SelectedItemAccessories = engine.UISelect.ToSelectedItemAccessoryDtos(engine.ControlsManager)?.ToList();

            var configDtos = new List<ProjectLineItemConfigurationDto>
            {
                // Add IsPricingIncluded -> T/F (set to true always, so only false will exist on older configurations before this was implemented
                new() { ItemId = currentDto.ItemId, StepId = 0, AccessoryName = "IsPricingIncluded", AccessoryValue = true },
                // Add the current model version number
                new() { ItemId = currentDto.ItemId, StepId = 0, AccessoryName = "ModelRulesVerNum", AccessoryValue = engine.VersionNumber }
            };
            foreach (var (key, value) in engine.DependencyVariablesManager.GetNotNullDependencyVariables())
            {
                var configDto = new ProjectLineItemConfigurationDto()
                {
                    ItemId = currentDto.ItemId,
                    AccessoryName = key,
                    AccessoryValue = value
                };
                // Handle cases that the country code was temporarily changed during initialization
                if (configDto.AccessoryName.Equals("CtryCode", StringComparison.OrdinalIgnoreCase))
                {
                    configDto.AccessoryValue = engine.OriginalCountryCode;
                }
                var control = engine.ControlsManager.FindControlByName(key);
                var addValue = true;
                ProjectLineItemConfigurationDto descDto = null;
                if (control is not null)
                {
                    configDto.StepId = control.GetControlConfigurationStepId(engine.ControlsManager);
                    // If the control is a dependent control (child control), but not a dimension type child control, only extract the value if it is visible to the user
                    // For instance, we don't want to include entered sleeve length if the user removes the option to add a sleeve.
                    // But, if the user as a CR (round) transition collar on a unit that normally has Width and Height, but only Diameter is visible, we still want to include the Width and Height values in the extract
                    if (control.IsChildControl && !control.IsDimensionControl) addValue = control.ShowControl;
                    // Add missing selected value descriptions as <AccId>_Description variable name -> value would be the description of the chosen accessory code
                    if (control.IsSelectionTypeControl && !engine.DependencyVariablesManager.DependencyVariables.ContainsKey($"{key}_Description"))
                    {
                        var selectedValue = control.SelectedValue?.ToString();
                        if (!string.IsNullOrWhiteSpace(selectedValue))
                        {
                            var option = control.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == selectedValue);
                            if (option != null && !string.IsNullOrEmpty(option.OptionDescription))
                            {
                                descDto = new()
                                {
                                    ItemId = currentDto.ItemId,
                                    AccessoryName = $"{key}_Description",
                                    AccessoryValue = option.OptionDescription
                                };
                            }
                        }
                    }
                }
                if (addValue) configDtos.Add(configDto);
                if (descDto is not null) configDtos.Add(descDto);
            }

            currentDto.ItemFinishCharges = new List<ItemFinishChargeDto>();
            currentDto.ItemPaintSetupCharges = new List<ItemPaintSetupChargeDto>();
            var finishPricing = FinishManager.GetFinishPricingDto(engine)?.ToList();
            if (finishPricing?.Count > 0)
            {
                foreach (var finishPricingDto in finishPricing)
                {
                    if (finishPricingDto.HasFinishCharges)
                    {
                        currentDto.ItemFinishCharges.Add(new ItemFinishChargeDto()
                        {
                            AccessoryDiscountId = engine.AccessoryDiscountId,
                            AccessoryName = finishPricingDto.FinishAccessoryName,
                            ColorCode = finishPricingDto.ColorCode,
                            ColorDescription = finishPricingDto.ColorDescription,
                            FinishCode = finishPricingDto.FinishCode,
                            FinishDescription = finishPricingDto.FinishDescription,
                            ItemId = currentDto.ItemId,
                            ItemSquareFeet = finishPricingDto.SquareFeet ?? finishPricingDto.SquareFeetSecond ?? 0,
                            ListPerSquareFoot = finishPricingDto.ChargePerSquareFoot ?? finishPricingDto.ChargePerSquareFootSecond ?? 0,
                            MinCharge = finishPricingDto.MinCharge ?? 0,
                            ProjectId = currentDto.ProjectId
                        });
                    }

                    if (finishPricingDto.HasPaintSetupCharges)
                    {
                        currentDto.ItemPaintSetupCharges.Add(new ItemPaintSetupChargeDto()
                        {
                            AccessoryDiscountId = engine.AccessoryDiscountId,
                            AccessoryName = finishPricingDto.FinishAccessoryName,
                            ColorCode = finishPricingDto.ColorCode,
                            ColorDescription = finishPricingDto.ColorDescription,
                            FinishCode = finishPricingDto.FinishCode,
                            FinishDescription = finishPricingDto.FinishDescription,
                            ItemId = currentDto.ItemId,
                            ListAddOnCharge = finishPricingDto.ListAddOn ?? 0,
                            MinSetupCharge = finishPricingDto.SetupCharge ?? finishPricingDto.SetupChargeSecond ?? 0,
                            ProjectId = currentDto.ProjectId
                        });
                    }
                }
            }

            currentDto.ConfigurationVariables = configDtos;

            currentDto.CustomVariants = new List<CustomVariantDto>();
            foreach (var uIOption in engine.UISelect.Where(x => x.UIControlType == UiControlTypeEnumeration.CustomVariant.DisplayName))
            {
                currentDto.CustomVariants.Add(new CustomVariantDto
                {
                    Step = ControlsManager.GetCustomControlStep(uIOption.ValueName),
                    Name = uIOption.CustomVarName,
                    Value = uIOption.CustomVarValue,
                    Quantity = uIOption.CustomVarQuantity,
                    Amount = uIOption.CustomVarAmount,
                    OtherInformation = uIOption.CustomVarOtherInformation
                });
            }

            // Do this last to include custom variants if any should exist
            //currentDto.ExtraAccessoryDetails = currentDto.ToExtraAccessoryDetails();

            return currentDto;
        }

        // SelectWorks version

        public static SelectedItemAccessoryDto ToSelectedItemAccessoryDto(this UIOption uIOption, ControlsManager controlsManager)
        {
            if (uIOption is null || !uIOption.IsSelectionTypeControl) return null;

            return new SelectedItemAccessoryDto
            {
                //AccessoryTypeDescription is not required by api,
                //so using AccessoryTypeDescription to save accessory type id to match imported from RAPP accessories which have different than engine step ids
                AccessoryTypeDescription = uIOption.ValueName,
                SelectedAccessoryCode = uIOption.SelectedValue?.ToString(),
                AccessoryValueDescription = uIOption.UISelectOptionsFullList?.FirstOrDefault(o => o.OptionValue == uIOption.SelectedValue?.ToString())?.OptionDescription,
                AccessoryStepNumber = uIOption.StepID,
                AccessoryConfigurationStepNumber = uIOption.GetControlConfigurationStepId(controlsManager)
            };
        }

        public static IEnumerable<SelectedItemAccessoryDto> ToSelectedItemAccessoryDtos(this IList<UIOption> uIOptions, ControlsManager controlsManager)
        {
            if (!(uIOptions?.Count > 0)) return Enumerable.Empty<SelectedItemAccessoryDto>();

            var convertedItems = uIOptions.Select(o => o.ToSelectedItemAccessoryDto(controlsManager));
            return convertedItems?.Where(c => c != null);
        }

        private static int GetControlConfigurationStepId(this UIOption uIOption, ControlsManager controlsManager)
        {
            if (uIOption.IsChildControl)
            {
                // For child controls we convert it to an int that maintains the dependent step id (DSI) along with the parent step id (PSI), Ex: PSI = 1, DSI = 2 => 1002
                return (controlsManager.FindControlByName(uIOption.ParentControlName).StepID * 1000) + uIOption.StepID;
            }

            // For non dependent controls just multiply by 1000
            return uIOption.StepID * 1000;
        }
    }
}
