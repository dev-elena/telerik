using Microsoft.Extensions.Logging;



using System;
using System.Collections.Generic;
using System.Linq;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;
using TelerikQ125.Pages.Dtos;


namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
	public static class FromSelectionAndPricingDtoToEngineExtension
	{
		private static ILogger<UnitSelectionAndPricingEngine> _logger;

		/// <summary>
		/// Create a new selection and pricing engine from the selection and pricing data transfer object.
		/// </summary>
		/// <param name="unitSelectionAndPricingDto">Selection and pricing data transfer object to convert from.</param>
		/// <param name="logger">ILogger that will be set in the engine (and drill down objects) to log information during the selection process.</param>
		public static UnitSelectionAndPricingEngine CreateEngineFromDto(this UnitSelectionAndPricingDto unitSelectionAndPricingDto, ILogger<UnitSelectionAndPricingEngine> logger = null, string vendorCountryCode = null)
		{
			var engine = new UnitSelectionAndPricingEngine(logger);
			unitSelectionAndPricingDto.PopulateEngineFromDto(engine, logger, vendorCountryCode);
			return engine;
		}





		/// <summary>
		/// Populate an already existing selection and pricing engine from the selection and pricing data transfer object.
		/// </summary>
		/// <param name="unitSelectionAndPricingDto">Selection and pricing data transfer object to convert from.</param>
		/// <param name="engine">Selection and pricing engine to populate with the data from the data transfer object.</param>
		/// <param name="logger">ILogger that will be set in the engine (and drill down objects) to log information during the selection process.</param>
		public static void PopulateEngineFromDto(this UnitSelectionAndPricingDto unitSelectionAndPricingDto, UnitSelectionAndPricingEngine engine, ILogger<UnitSelectionAndPricingEngine> logger = null, string vendorCountryCode = null, bool isLight = false)
		{
			_logger = logger;
			PopulateEngine(engine, unitSelectionAndPricingDto, vendorCountryCode, isLight);
		}

		private static void PopulateEngine(UnitSelectionAndPricingEngine engine, UnitSelectionAndPricingDto unitSelectionAndPricingDto, string vendorCountryCode, bool isLight = false)
		{
			// Create the list of dependent variables - commented out to allow the engine to only add what it needs to use
			//AddDependentVariableNames(unitSelectionAndPricingDto.VariableNames.ToList());

			// Set the initial system of measure
			//engine.SystemOfMeasure = (string.IsNullOrWhiteSpace(unitSelectionAndPricingDto.SystemOfMeasure) ? "Imperial" : unitSelectionAndPricingDto.SystemOfMeasure);
			SetSystemOfMeasure(engine, unitSelectionAndPricingDto.SystemOfMeasure);

			// Handle country codes that are not US, CA or UK
			List<string> validCodes = new List<string> { "US", "CA", "UK" };
			engine.OriginalCountryCode = vendorCountryCode;
			if (!validCodes.Contains(vendorCountryCode, StringComparer.OrdinalIgnoreCase))
			{
				// Default to the US country code for processing this model
				vendorCountryCode = "US";
			}
			engine.VendorCountryCode = vendorCountryCode;

			// Run through each data row
			var FirstRun = true;
			var previousControlName = string.Empty;
			foreach (var dataRow in unitSelectionAndPricingDto.DataRows)
			{
				if (FirstRun)
				{
					// Set the engine level data only in the first loop
					FirstRunProcedure(engine, dataRow);
					FirstRun = false;
				}

				// Add the control data for the row
				if (string.IsNullOrWhiteSpace(previousControlName) || previousControlName?.Equals(dataRow.Step_ID) == false)
				{
					var option = ControlFromUnitSelectionAndPricingRowDto(dataRow, vendorCountryCode, isLight);
					if (option is not null)
					{
						option.SystemOfMeasure = engine.SystemOfMeasure;
						option.IsPerformanceOption = engine._isSelectWorksEngine;
						// Transfer any dependent controls
						if (dataRow.DepStep_ID is not null)
						{
							var depControl = DependentControlFromUnitSelectionAndPricingRowDto(dataRow, vendorCountryCode, isLight);
							if (depControl is not null)
							{
								depControl.SystemOfMeasure = engine.SystemOfMeasure;
								depControl.IsPerformanceOption = engine._isSelectWorksEngine;
								option.DependentControls.Add(depControl);
								engine.ControlsManager.AddControlToIndex(depControl.ValueName, depControl.UIControlType, depControl.StepID, depControl.ParentControlName, option.StepID);
							}
						}

						engine.UISelect.Add(option);
						engine.ControlsManager.AddControlToIndex(option.ValueName, option.UIControlType, option.StepID);
					}
				}
				else
				{
					// Only needing to add another dependent control
					var option = engine.ControlsManager.FindControlByName(dataRow.Step_ID);
					if (option is not null)
					{
						var depControl = DependentControlFromUnitSelectionAndPricingRowDto(dataRow, vendorCountryCode, isLight);
						if (depControl is not null)
						{
							depControl.SystemOfMeasure = engine.SystemOfMeasure;
							depControl.IsPerformanceOption = engine._isSelectWorksEngine;
							option.DependentControls.Add(depControl);
							engine.ControlsManager.AddControlToIndex(depControl.ValueName, depControl.UIControlType, depControl.StepID, depControl.ParentControlName, option.StepID);
						}
					}
				}

				previousControlName = dataRow.Step_ID;
			}

			// Now organize any dependent controls that need to be placed under other dependent controls
			foreach (var control in engine.ControlsManager.UiSelect.Where(c => c.HasDependentControls))
			{
				var listToRemove = new List<string>();
				foreach (var depControl in control.DependentControls.Where(d => d.Level > 1))
				{
					var parentDep = control.DependentControls.LastOrDefault(d => d.Level == 1 && d.StepID < depControl.StepID);
					if (parentDep is null) continue;

					listToRemove.Add(depControl.ValueName);
					parentDep.DependentControls.Add(depControl);
					engine.ControlsManager.RemoveControlFromIndex(depControl.ValueName, depControl.UIControlType, depControl.StepID, depControl.ParentControlName, control.StepID);
					depControl.ParentControlName = parentDep.ValueName;
					engine.ControlsManager.AddControlToIndex(depControl.ValueName, depControl.UIControlType, depControl.StepID, depControl.ParentControlName, parentDep.StepID);
				}

				foreach (var name in listToRemove)
				{
					var depToRemove = control.DependentControls.FirstOrDefault(d => d.ValueName == name);
					if (depToRemove is null) continue;

					control.DependentControls.Remove(depToRemove);
				}
			}
			engine.ControlsManager.SetInitializationState(engine.ControlsManager.IsInitializing);
		}


		private static void SetSystemOfMeasure(UnitSelectionAndPricingEngine engine, string systemOfMeasure)
		{
			engine.SystemOfMeasure = (string.IsNullOrWhiteSpace(systemOfMeasure) ? "Imperial" : systemOfMeasure);
		}



		private static void FirstRunProcedure(UnitSelectionAndPricingEngine engine, UnitSelectionAndPricingRowDto rowDto)
		{
			// Set the engine level data only in the first loop
			engine.FamilyCode = rowDto.FamilyCode;
			engine.Country = rowDto.Country;
			engine.PricingVersionNumber = rowDto.VerNum;
			engine.DefaultCurrency = rowDto.DefCurrency;
			engine.Model = rowDto.Model;
			engine.ModelDescription = rowDto.ModelDesc;
			if (!engine.SetModelMultiplierFromRules(rowDto.BaseModelMultRules))
			{
				engine.ModelMultiplier = rowDto.BaseModelMult;
			}
			engine.EnablePerformanceOption = rowDto.GotPerformance;
			engine.OverallMaxMin = FromMaxMinDto(rowDto.OverallMaxMin, "MaxMin");
			engine.SectionMaxMin = FromMaxMinDto(rowDto.SectionMaxMin, "MaxMinSect");
			engine.DiscountId = rowDto.DiscountId;
			engine.AccessoryDiscountId = rowDto.AccessoryDiscountId;
			engine.ProductDiscountCombinedId = rowDto.ProductDiscountCombinedId;
			engine.VersionNumber = rowDto.VerNum;
		}


		private static UIOption ControlFromUnitSelectionAndPricingRowDto(UnitSelectionAndPricingRowDto dto, string vendorCountryCode, bool isLight = false)
		{
			if (dto is null) return null;
			if (dto.UIInfo?.AuthorizedForVendorCountry(vendorCountryCode) != true)
				return null;

			var option = new UIOption
			{
				PriceAdder = dto.BaseAdder,
				StepID = dto.Step,
				ValueName = dto.Step_ID,
				UIDescription = dto.Step_Desc,
				PriceMultiplier = dto.MatrixMultiplier,
				UIControlType = dto.UIObjectType,
				Level = dto.UILevel
			};

			var valueType = ValueTypeEnumeration.NotApplicable;
			if (!string.IsNullOrWhiteSpace(dto.UIValueType))
			{
				valueType = BaseEnumeration.FromDisplayNameOrdinalIgnoreCase<ValueTypeEnumeration>(dto.UIValueType);
			}
			option.ValueType = valueType;

			// Transfer pricing rules
			if (!isLight && dto.Pmatrix?.Count > 0)
			{
				foreach (var mat in dto.Pmatrix)
				{
					var pMatEngine = FromPMatrixDto(mat);
					if (pMatEngine is not null)
						option.PriceMatrices.Add(pMatEngine);
				}
			}

			// Transfer the UIInfo rules
			if (dto.UIInfo is not null)
			{
				// Set the default
				option.DefaultOption = dto.UIInfo.Default.GetDefaultByCountry(vendorCountryCode);
				option.CountryCode = dto.UIInfo.AccCtry;
				option.ShowOptionCodes = dto.UIInfo.ShowOptCode;

				// Add any OnSelect rules
				if (dto.UIInfo.OnSelect?.Count > 0)
				{
					option.OnSelectRules = FromOnSelectDto(dto.UIInfo.OnSelect);
				}

				// Add any selection options (for selection type control)
				if (dto.UIInfo.UISelect?.Count > 0)
				{
					foreach (var uiSelect in dto.UIInfo.UISelect)
					{
						if (!uiSelect.AuthorizedForVendorCountry(vendorCountryCode))
							continue;

						var controlOption = FromUISelectDto(uiSelect);
						if (controlOption is not null)
						{
							controlOption.ShowDescriptionOnly = !option.ShowOptionCodes;
							option.AddOption(controlOption);
						}
					}

					option.OrderOptionsLists();
				}
			}

			// Transfer any validation rules
			if (dto.VRules?.ValRules?.Count > 0)
			{
				foreach (var vRule in dto.VRules.ValRules)
				{
					option.ValidationJsonRules.Add(vRule.JsonRule);
					//option.AddAccessoryNamesFromJsonRule(vRule.JsonRule);
				}
			}

			if (!isLight && dto.VRules?.ValMatrices?.Count > 0)
			{
				foreach (var matRule in dto.VRules.ValMatrices)
				{
					option.ValidationMatrices.Add(FromJsonRuleMatrixDto(matRule));
				}
			}

			return option;
		}

		private static UIOption DependentControlFromUnitSelectionAndPricingRowDto(UnitSelectionAndPricingRowDto dto, string vendorCountryCode, bool isLight = false)
		{
			if (dto is null) return null;
			if (dto.DepUIInfo?.AuthorizedForVendorCountry(vendorCountryCode) != true)
				return null;

			// Basic information
			var depControl = new UIOption
			{
				ValueName = dto.DepStep_ID,
				UIDescription = dto.DepStep_Desc,
				StepID = dto.DepStepNumber,
				UIControlType = dto.DepUIObjectType,
				ParentControlName = dto.Step_ID,
				IsChildControl = true,
				Level = dto.DepUILevel
			};

			var valueType = ValueTypeEnumeration.NotApplicable;
			if (!string.IsNullOrWhiteSpace(dto.DepUIValueType))
			{
				valueType = BaseEnumeration.FromDisplayNameOrdinalIgnoreCase<ValueTypeEnumeration>(dto.DepUIValueType);
			}
			depControl.ValueType = valueType;

			// Pricing rules
			if (!isLight && dto.DepPMatrix?.Count > 0)
			{
				foreach (var mat in dto.DepPMatrix)
				{
					var dMatEngine = FromPMatrixDto(mat);
					if (dMatEngine is not null)
						depControl.PriceMatrices.Add(dMatEngine);
				}
			}

			// UIInfo rules
			if (dto.DepUIInfo is not null && !dto.DepUIInfo.IsEmpty())
			{
				// Using standard UI Info logic
				ConvertDependentUiInfoDto(ref depControl, dto.DepUIInfo, vendorCountryCode);
			}
			else if (dto.DepSBList is not null && !dto.DepSBList.IsEmpty())
			{
				// Using special UI Info logic
				ConvertDependentUiInfoDto(ref depControl, dto.DepSBList, vendorCountryCode);
			}

			// Validation rules
			if (!(dto.DepVRules?.ValRules?.Count > 0) && !(dto.DepVRules?.ValMatrices?.Count > 0)) return depControl;

			if (dto.DepVRules?.ValRules?.Count > 0)
			{
				foreach (var vRule in dto.DepVRules.ValRules)
				{
					depControl.ValidationJsonRules.Add(vRule.JsonRule);
					//depControl.AddAccessoryNamesFromJsonRule(vRule.JsonRule);
				}
			}

			if (!isLight && dto.VRules?.ValMatrices?.Count > 0)
			{
				foreach (var matRule in dto.VRules.ValMatrices)
				{
					depControl.ValidationMatrices.Add(FromJsonRuleMatrixDto(matRule));
				}
			}

			return depControl;
		}

		private static void ConvertDependentUiInfoDto(ref UIOption option, UIInfoDto uiInfo, string vendorCountryCode)
		{
			// Set the default
			option.DefaultOption = uiInfo.Default.GetDefaultByCountry(vendorCountryCode);

			// Add any OnSelect rules
			if (uiInfo.OnSelect?.Count > 0)
			{
				option.OnSelectRules = FromOnSelectDto(uiInfo.OnSelect);
			}

			// Add any selection options (for selection type control)
			if (!(uiInfo.UISelect?.Count > 0)) return;

			foreach (var uiSelect in uiInfo.UISelect)
			{
				if (!uiSelect.AuthorizedForVendorCountry(vendorCountryCode))
					continue;

				var controlOption = FromUISelectDto(uiSelect);
				if (controlOption is not null)
				{
					controlOption.ShowDescriptionOnly = !option.ShowOptionCodes;
					option.AddOption(controlOption);
				}
			}

			option.OrderOptionsLists();
		}

		private static ControlOption FromUISelectDto(UISelectDto uISelectDto)
		{
			if (uISelectDto is null) return null;

			var controlOption = new ControlOption
			{
				OptionValue = uISelectDto.Val,
				OptionDescription = uISelectDto.Desc,
				CountryCode = uISelectDto.OpCtry,
				OrderId = uISelectDto.OpOrder,
				ShowInOptionString = uISelectDto.AddToOptList,
				PrintOptionDescription = uISelectDto.PrintDesc,
				UnitCategory = uISelectDto.UnitCategory
			};
			if (uISelectDto.MsgID?.Count > 0)
			{
				controlOption.MessageIds = uISelectDto.MsgID;
			}
			if (uISelectDto.Dep?.Count > 0)
			{
				foreach (var dep in uISelectDto.Dep)
				{
					controlOption.Dependencies.Add(new OptionDependency
					{
						JsonRule = dep.JsonRule
					});
				}
			}

			return controlOption;
		}

		private static IList<OnSelect> FromOnSelectDto(IList<OnSelectDto> onSelectDtos, bool isLight = false)
		{
			if (!(onSelectDtos?.Count > 0)) return new List<OnSelect>();

			OnSelect onSelectRule = null;

			// Check if there are any existing pricing and/or show rules that get set
			var containsRunPricingRule = onSelectDtos.Where(o => o.Price?.Count > 0)?.Any() == true;
			var containsShowControlRule = onSelectDtos.Where(o => o.Show?.Count > 0)?.Any() == true;
			var containsHideControlRule = onSelectDtos.Where(o => o.Hide?.Count > 0)?.Any() == true;
			var containsEnableControlRule = onSelectDtos.Where(o => o.Enable?.Count > 0)?.Any() == true;

			foreach (var onSelectDto in onSelectDtos)
			{
				// If there are no show control rules in this instance of OnSelect, but there are for other instances within this control, it means hide the controls that were shown
				CreateShowRule(ref onSelectRule, onSelectDto, nameof(onSelectRule.ShowRules), nameof(onSelectDto.Show), containsShowControlRule);

				// If there are no pricing rules in this instance of OnSelect, but there are for other instances within this control, it means clear out the pricing to run
				if (!isLight && onSelectDto.Price?.Count > 0 || (containsRunPricingRule && (onSelectDto.Val?.Count > 0 || !string.IsNullOrWhiteSpace(onSelectDto.JsonVal))))
				{
					onSelectRule ??= new OnSelect();
					onSelectRule.PriceRules.Add(new PriceRule
					{
						PriceRulesToRun = onSelectDto.Price,
						SelectedValues = onSelectDto.Val
					});
				}
				// If there are no enable control rules in this instance of OnSelect, but there are for other instances within this control, it means disable the controls that were enabled
				CreateShowRule(ref onSelectRule, onSelectDto, nameof(onSelectRule.EnableRules), nameof(onSelectDto.Enable), containsEnableControlRule);

				// If there are no hide control rules in this instance of OnSelect, but there are for other instances within this control, it means show the controls that were hidden
				CreateShowRule(ref onSelectRule, onSelectDto, nameof(onSelectRule.HideRules), nameof(onSelectDto.Hide), containsHideControlRule);
			}

			return onSelectRule != null ? new List<OnSelect> { onSelectRule } : new List<OnSelect>();
		}

		private static void CreateShowRule(ref OnSelect onSelect, OnSelectDto onSelectDto, string rulePropName, string dtoPropName, bool containsRule)
		{
			if (!((onSelectDto.GetType().GetProperty(dtoPropName)?.GetValue(onSelectDto) as IList<string>)?.Count > 0)
				&& (!containsRule || (!(onSelectDto.Val?.Count > 0) && string.IsNullOrWhiteSpace(onSelectDto.JsonVal)))) return;

			onSelect ??= new OnSelect();
			(onSelect.GetType().GetProperty(rulePropName)?.GetValue(onSelect) as IList<ShowRule>)?.Add(new ShowRule()
			{
				JsonValueRule = onSelectDto.JsonVal,
				ShowOptions = onSelectDto.GetType().GetProperty(dtoPropName)?.GetValue(onSelectDto) as IList<string>,
				SelectedValues = onSelectDto.Val
			});
		}

		private static PriceMatrixEngine FromPMatrixDto(PmatrixDto pMatrixDto)
		{
			if (pMatrixDto is null) return null;

			if (pMatrixDto.IsEmpty()) return null;

			var pMatEngine = new PriceMatrixEngine
			{
				PricingMethod = pMatrixDto.PricingMethod,
				AllowDimsSwap = pMatrixDto.AllowDimsSwap
			};
			if (pMatrixDto.Cindex?.Count > 0)
			{
				foreach (var cIndex in pMatrixDto.Cindex)
				{
					var convertedCIndex = FromPricingIndexDto(cIndex);
					if (convertedCIndex is not null)
						pMatEngine.PriceColumnIndexes.Add(convertedCIndex);
				}
			}
			if (pMatrixDto.Rindex?.Count > 0)
			{
				foreach (var rIndex in pMatrixDto.Rindex)
				{
					var convertedRIndex = FromPricingIndexDto(rIndex);
					if (convertedRIndex is not null)
						pMatEngine.PriceRowIndexes.Add(convertedRIndex);
				}
			}
			if (pMatrixDto.Price is not null)
			{
				pMatEngine.JsonRule = pMatrixDto.Price.JsonRule;
				if (pMatrixDto.Price.Mat?.Count > 0)
				{
					foreach (var priceMat in pMatrixDto.Price.Mat)
					{
						pMatEngine.Matrices.Add(new PriceMatrix
						{
							MatrixID = priceMat.MatID,
							DataMatrix = priceMat.DataM
						});
					}
				}
			}

			return pMatEngine;
		}

		private static PriceMatrixEngine FromJsonRuleMatrixDto(JsonRuleMatrixDto jsonRuleMatrix)
		{
			if (jsonRuleMatrix is null) return null;

			if (jsonRuleMatrix.IsEmpty()) return null;

			var pMatEngine = new PriceMatrixEngine
			{
				PricingMethod = jsonRuleMatrix.ValidationMatrix,
				AllowDimsSwap = jsonRuleMatrix.AllowDimsSwap,
				IsValidationMatrixEngine = true
			};
			if (jsonRuleMatrix.Cindex?.Count > 0)
			{
				foreach (var cIndex in jsonRuleMatrix.Cindex)
				{
					var convertedCIndex = FromPricingIndexDto(cIndex);
					if (convertedCIndex is not null)
						pMatEngine.PriceColumnIndexes.Add(convertedCIndex);
				}
			}
			if (jsonRuleMatrix.Rindex?.Count > 0)
			{
				foreach (var rIndex in jsonRuleMatrix.Rindex)
				{
					var convertedRIndex = FromPricingIndexDto(rIndex);
					if (convertedRIndex is not null)
						pMatEngine.PriceRowIndexes.Add(convertedRIndex);
				}
			}
			if (jsonRuleMatrix.Calculation is not null)
			{
				pMatEngine.JsonRule = jsonRuleMatrix.Calculation.JsonRule;
				if (jsonRuleMatrix.Calculation.Mat?.Count > 0)
				{
					foreach (var priceMat in jsonRuleMatrix.Calculation.Mat)
					{
						pMatEngine.Matrices.Add(new PriceMatrix
						{
							MatrixID = priceMat.MatID,
							DataMatrix = priceMat.DataM
						});
					}
				}
			}

			return pMatEngine;
		}

		private static IndexLookup FromPricingIndexDto(PriceIndexDto priceIndexDto)
		{
			if (priceIndexDto is null) return null;

			var indexLookup = new IndexLookup
			{
				MatrixID = priceIndexDto.MatID,
				LookupValueName = priceIndexDto.LVal,
				LookupJsonRule = priceIndexDto.JsonRule,
				logger = _logger
			};
			if (priceIndexDto.SOM?.Count > 0)
			{
				foreach (var som in priceIndexDto.SOM)
				{
					var lookup = new IndexLookupSOM
					{
						LookupRule = som.LookupRule,
						Values = som.Val
					};
					if (som.UOM?.Equals("imperial", StringComparison.OrdinalIgnoreCase) == true)
					{
						indexLookup.Imperial = lookup;
					}
					else if (som.UOM?.Equals("metric", StringComparison.OrdinalIgnoreCase) == true)
					{
						indexLookup.Metric = lookup;
					}
					else
					{
						// Unitless
						indexLookup.Imperial = lookup;
						indexLookup.Metric = lookup;
					}
				}
			}

			return indexLookup;
		}

		private static SizingLimits FromMaxMinDto(MaxMinSizeRulesDto maxMinDto, string name)
		{
			if (maxMinDto?.IsEmpty() != false) return null;

			var maxMin = new SizingLimits
			{
				SizingLimitName = name,
				MultiSectionCalculationRuleCode = maxMinDto.MulticalcRule,
				MultiSectionPriceTypeCode = maxMinDto.MultiPriceType,
				MultiSectionPriceValue = maxMinDto.MultiPrice
			};

			if (!(maxMinDto.ValRules?.Count > 0)) return maxMin;
			foreach (var valRule in maxMinDto.ValRules)
			{
				maxMin.ValidationJsonRules.Add(valRule.JsonRule);
			}

			return maxMin;
		}
	}
}
