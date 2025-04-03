using Microsoft.Extensions.Logging;


using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using Telerik.DataSource.Extensions;

using static telerik_Q1_25.Pages.Dtos.SelectionAndPricing.BasicEnumerations;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;
using static telerik_Q1_25.Pages.Dtos.Class;
using TelerikQ125.Pages.Dtos;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
	public class UnitSelectionAndPricingEngine : IDisposable
	{
		// Create all the options that the UI needs to provide the user
		// along with the rules and verification processes

		private SystemOfMeasureEnum _systemOfMeasure;

		private readonly List<ValueChangeEventArgs> _valuesToChange = new();

		private readonly IUnitSelectionAndPricingEngineStateParser _optionsStringGenerator =
			new UnitSelectionAndPricingEngineStateParser();
		public bool IsLight { get; set; } = false;
		private BasicEnumerations.CountryCodeEnum _countryCode;
		private string _vendorCountryCode = null;
		private readonly ILogger<UnitSelectionAndPricingEngine> _logger;
		private bool _logInformation = false;
		private SectioningManager SectioningManager { get; set; }
		private UnitSelectionAndPricingDto _unitSelectionAndPricingDto;
		internal PricingManager PricingManager { get; set; }
		private bool _isDisposed;
		private bool _canUserSeePricing;
		private ProjectLineItemDisplayDto _projectLineItemDisplayDto;
		private string _needTokenControl;
		internal bool _isSelectWorksEngine = false;
		private protected string _model;
		private bool _isSpecialModel;
		private bool _isSpecialOverride;
		private readonly List<string> _changedAccessoryNames = new();
		private bool _isSelectWorksItem;
		private bool _isValidatingOnCommand = false;

		public const string SelectWorksItemVarName = "SelectWorksItem";
		public const string SpecialOverrideVarName = "SpecialOverride";
		public const string SpecialModelVarName = "SpecialModel";

		/// <summary>
		/// The controls and UiOptions country codes are compared against VendorCountryCode for inclusion in the Models Engine
		/// </summary>
		public string VendorCountryCode
		{
			get => _vendorCountryCode;
			set
			{
				_vendorCountryCode = value;
				DependencyVariablesManager.AddValueToDependencyVariable("CtryCode", _vendorCountryCode?.ToUpper(), 0);
			}
		}

		/// <summary>
		/// Id of the line item.
		/// </summary>
		public Guid Id { get; set; } = Guid.NewGuid();

		/// <summary>
		/// Project/Order/Cart line number for this item.
		/// </summary>
		public int LineNumber { get; set; }

		public int VersionNumber { get; internal set; }

		/// <summary>
		/// List of UI controls to present to the user.
		/// </summary>
		/// <remarks>Each control has a ShowControl property to determine if the control should be visible/enabled.</remarks>
		public IList<UIOption> UISelect
		{
			protected set => ControlsManager.UiSelect = value;
			get => ControlsManager.UiSelect;
		}

		/// <summary>
		/// Family code of the unit.
		/// </summary>
		public string FamilyCode { get; set; }

		/// <summary>
		/// Country code this pricing model is designed for.
		/// </summary>
		public string Country
		{
			get => _countryCode.GetName();
			set => _countryCode =
				value.GetEnumValue<BasicEnumerations.CountryCodeEnum>();
		}

		/// <summary>
		/// Pricing model version number.
		/// </summary>
		public int PricingVersionNumber { get; set; }

		/// <summary>
		/// Default currency type.
		/// </summary>
		public string DefaultCurrency { get; set; }

		/// <summary>
		/// Model of the unit.
		/// </summary>
		public string Model
		{
			get => _model;
			set
			{
				_model = value;
				DependencyVariablesManager.AddValueToDependencyVariable("model", value, 0);
				if (_model.Equals("SPECIAL", StringComparison.OrdinalIgnoreCase))
				{
					_isSpecialModel = true;
					DependencyVariablesManager.AddValueToDependencyVariable(SpecialModelVarName, true, 0);
					// Set special override to true so if this item is later edited, the value will exist
					_isSpecialOverride = true;
					DependencyVariablesManager.AddValueToDependencyVariable(SpecialOverrideVarName, true, 0);
				}
			}
		}

		public bool IsSpecialModel
		{
			get => _isSpecialModel;
		}

		/// <summary>
		/// IsSelectWorksItem set as "true" when item is imported from SelectWorks, user should not have an ability to change it. 
		/// </summary>
		public bool IsSelectWorksItem
		{
			get => _isSelectWorksItem;
			set
			{
				_isSelectWorksItem = value;
				DependencyVariablesManager.AddValueToDependencyVariable(SelectWorksItemVarName, _isSelectWorksItem, 0);
				if (value)//set only to true
				{
					//set item as special to give user ability to uncheck "special" to run pricing and validation
					_isSpecialOverride = value;
					DependencyVariablesManager.AddValueToDependencyVariable(SpecialOverrideVarName, _isSpecialOverride, 0);
				}
			}
		}

		public bool ProPricingMode { get; set; }
		public bool ProSelectionMode { get; set; }

		public bool IsSpecialOverride
		{
			get => _isSpecialOverride;
			set
			{
				_isSpecialOverride = value;
				DependencyVariablesManager.AddValueToDependencyVariable(SpecialOverrideVarName, _isSpecialOverride, 0);
				if (!IsLight) PricingManager.ClearPricingVariables(ModelMultiplier);
				if (_isSpecialOverride)
				{
					InvalidOptions.Clear();
					GeneralMessages.Clear();

					_setValidStateFromOptions = true;

					foreach (var uiOption in UISelect)
					{
						uiOption.ShowControl = true;
						uiOption.UISelectOptions.Clear();
						uiOption.UISelectOptions.AddRange(uiOption.UISelectOptionsFullList);
						uiOption.Status = UIControlStatusEnumerations.Default;
					}


					if (!_isSelectWorksItem)
					{
						LogInformation("Checking if the base price control already exists.");
						if (!ControlsManager.HasBasePriceVariant())
						{
							LogInformation("Base price control does not exist yet, so adding it to the UI.");
							var basePriceControl = ControlsManager.AddBasePriceVariant();
							basePriceControl.OnValueChangeEvent += OnValueChangeEvent;

							if (!ProPricingMode && !IsLight) InternalRunPricing();
						}
						else
						{
							var basePriceControl = ControlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
							var result = basePriceControl?.RunValidation(DependencyVariablesManager.DependencyVariables.ConvertToDynamic(_logInformation ? _logger : null)) ?? false;
							_setValidStateFromOptions = result;
							SetErrorState(basePriceControl);
							LogInformation("Base price control already exists.");
						}
					}
				}
				else
				{
					var basePriceControl = ControlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
					if (basePriceControl != null && !(basePriceControl.ValidationJsonRules?.Count > 0)) // If there are no validation rules, this is a custom added one from above and not a new model ruleset.
					{
						LogInformation("Base price control exists, so removing it.");
						basePriceControl.OnValueChangeEvent -= OnValueChangeEvent;
						ControlsManager.RemoveBasePriceVariant();
						DependencyVariablesManager.AddValueToDependencyVariable(ControlsManager.SpecialBasePriceControl, null, 0);
					}
					if (!IsLight) ControlsManager.SetShowStates(DependencyVariablesManager);
					InvalidOptions.Clear();

					if (!IsLight)
					{
						RunAllRules(string.Empty);
						ApplyRuleChanges();
					}
				}
				if (!ProPricingMode && !IsLight) InternalRunPricing();
			}
		}

		/// <summary>
		/// Description of the model.
		/// </summary>
		public string ModelDescription { get; set; }

		/// <summary>
		/// Overall max and min sizing limit rules.
		/// </summary>
		public SizingLimits OverallMaxMin { get; set; }

		/// <summary>
		/// Sectioning max and min sizing limit rules.
		/// </summary>
		public SizingLimits SectionMaxMin { get; set; }

		/// <summary>
		/// Initial (base) model price multiplier.
		/// </summary>
		public double ModelMultiplier { get; set; } = 1;

		internal bool SetModelMultiplierFromRules(ModelMultRulesDto modelMultRules)
		{
			if (modelMultRules?.ModelMultRules?.Length > 0)
			{
				string? modelMultStr = null;
				double modelMult = 0;
				foreach (var modelMultRule in modelMultRules.ModelMultRules)
				{
					IList<KeyValuePair<string, object>> diffs = new List<KeyValuePair<string, object>>();
					double? numberResult = 0;
					_ = HelperMethodsAndGeneralExtensions.HandleRulesResultType(modelMultRule.JsonRule.RunJsonRule(DependencyVariablesManager.DependencyVariables, _logger), DependencyVariablesManager.DependencyVariables.ConvertToDynamic(), out diffs, out numberResult, _logger);
					if (diffs?.Count > 0)
					{
						modelMultStr = diffs.FirstOrDefault(k => k.Key.Equals("modelmult", StringComparison.OrdinalIgnoreCase)).Value?.ToString();
					}
					if (double.TryParse(modelMultStr, out var mult))
					{
						modelMult += mult;
					}
				}

				if (modelMult != 0)
				{
					ModelMultiplier = modelMult;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Are all the visible/enabled options still valid for this unit.
		/// </summary>
		public bool AreOptionsValid => _setValidStateForOverall && _setValidStateFromOptions;
		internal bool _setValidStateFromOptions;
		private bool _setValidStateForOverall = true;

		public ObservableCollection<string> OverallValidationErrors { get; protected set; } = new();

		/// <summary>
		/// The unit's system of measure ('Metric' or 'Imperial')
		/// </summary>
		public string SystemOfMeasure
		{
			get => _systemOfMeasure.GetName(true);
			set
			{
				_systemOfMeasure = value.GetEnumValue<SystemOfMeasureEnum>();
				DependencyVariablesManager.AddValueToDependencyVariable("som", _systemOfMeasure.GetName().ToLower(), 0);
				PropagateSom();
			}
		}

		/// <summary>
		/// List of options that are invalid after running the validation process.
		/// </summary>
		public ObservableCollection<InvalidOption> InvalidOptions { get; set; } =
			new ObservableCollection<InvalidOption>();

		/// <summary>
		/// Pricing variables housing the results from running the pricing process.
		/// </summary>
		public Dictionary<PriceTypeEnumeration, double?> PricingVariables => PricingManager.PricingValues;

		/// <summary>
		/// Quantity of this unit per the user.
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Options string for reporting and main display.
		/// </summary>
		public string UiOptionsSelectedValuesAsString { get; private set; }

		/// <summary>
		/// Options string for reconstruction use.
		/// </summary>
		public string UiOptionsSelectedValuesWithStepIdAsString { get; private set; }

		/// <summary>
		/// Should the Performance Selections toggle be enabled?
		/// </summary>
		public bool EnablePerformanceOption { get; set; }

		internal DependencyVariablesManager DependencyVariablesManager { get; private set; }

		public ControlsManager ControlsManager { get; private set; }

		public string DiscountId { get; set; }
		public string AccessoryDiscountId { get; set; }
		public string ProductDiscountCombinedId { get; set; }
		public IList<UnitColor> ColorInformation => FinishManager.GetColors(this);
		public bool HasColorInformation => ColorInformation?.Count > 0;

		internal string UserChangedControlName { get; set; }

		public double? SquareFootage
		{
			get
			{
                var width = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Width.DisplayName);
                var height = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Height.DisplayName);
                var length = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Length.DisplayName);
                var heightSz = DependencyVariablesManager.GetValueFromDependencyVariable("HeightSz_Height");
                if (width is not null && height is not null) //for louvers with 2 dims
                {
                    if (SystemOfMeasure.Equals("metric", StringComparison.OrdinalIgnoreCase))
                    {
                        return width.ToUSCultureDouble().MillimetersToFeet() * height.ToUSCultureDouble().MillimetersToFeet();
                    }

                    return width.ToUSCultureDouble().InchesToFeet() * height.ToUSCultureDouble().InchesToFeet();
                }
                else if (width is not null && length is not null && heightSz is not null) //for louvers with 3 dims: 1600phb, 1600phm
                {
                    double dim1;
                    double dim2;
                    double dim3;
                    double sqft;
                    if (SystemOfMeasure.Equals("metric", StringComparison.OrdinalIgnoreCase))
                    {
                        dim1 = width.ToUSCultureDouble().MillimetersToFeet() * heightSz.ToUSCultureDouble().MillimetersToFeet() * 2;
                        dim2 = length.ToUSCultureDouble().MillimetersToFeet() * heightSz.ToUSCultureDouble().MillimetersToFeet() * 2;
                        dim3 = length.ToUSCultureDouble().MillimetersToFeet() * width.ToUSCultureDouble().MillimetersToFeet();
                        sqft = dim1 + dim2 + dim3;
                        return sqft;
                    }
                    dim1 = width.ToUSCultureDouble().InchesToFeet() * heightSz.ToUSCultureDouble().InchesToFeet() * 2;
                    dim2 = length.ToUSCultureDouble().InchesToFeet() * heightSz.ToUSCultureDouble().InchesToFeet() * 2;
                    dim3 = length.ToUSCultureDouble().InchesToFeet() * width.ToUSCultureDouble().InchesToFeet();
                    sqft = dim1 + dim2 + dim3;

                    return sqft;
                }
                return null;
            }
		}

		public double? SquareMillimeter
		{
			get
			{
                var width = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Width.DisplayName);
                var height = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Height.DisplayName);
                var length = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Length.DisplayName);
                var heightSz = DependencyVariablesManager.GetValueFromDependencyVariable("HeightSz_Height");
                if (width is not null && height is not null) //for louvers with 2 dims
                {
                    if (SystemOfMeasure.Equals("imperial", StringComparison.OrdinalIgnoreCase))
                    {
                        return width.ToUSCultureDouble().InchesToMillimeters() * height.ToUSCultureDouble().InchesToMillimeters();
                    }

                    return width.ToUSCultureDouble() * height.ToUSCultureDouble();
                }
                else if (width is not null && length is not null && heightSz is not null) //for louvers with 3 dims: 1600phb, 1600phm
                {
                    double dim1;
                    double dim2;
                    double dim3;
                    double sqmm;
                    if (SystemOfMeasure.Equals("imperial", StringComparison.OrdinalIgnoreCase))
                    {
                        dim1 = width.ToUSCultureDouble().InchesToMillimeters() * heightSz.ToUSCultureDouble().InchesToMillimeters() * 2;
                        dim2 = length.ToUSCultureDouble().InchesToMillimeters() * heightSz.ToUSCultureDouble().InchesToMillimeters() * 2;
                        dim3 = length.ToUSCultureDouble().InchesToMillimeters() * width.ToUSCultureDouble().InchesToMillimeters();
                        sqmm = dim1 + dim2 + dim3;
                        return sqmm;
                    }
                    dim1 = width.ToUSCultureDouble() * heightSz.ToUSCultureDouble()* 2;
                    dim2 = length.ToUSCultureDouble() * heightSz.ToUSCultureDouble() * 2;
                    dim3 = length.ToUSCultureDouble() * width.ToUSCultureDouble();
                    sqmm = dim1 + dim2 + dim3;

                    return sqmm;
                }
                return null;
              
			}
		}

		public bool HasDimensionalData => SquareFootage is not null;

		public ObservableCollection<string> GeneralMessages = new ObservableCollection<string>();

		internal string OriginalCountryCode { get; set; }

		public UnitSelectionAndPricingEngine(ILogger<UnitSelectionAndPricingEngine> logger = null, bool isLight = false)
		{
			_logger = logger;
			IsLight = isLight;
			SetupManagers();
		}

		private void SetupManagers()
		{
			DependencyVariablesManager = new DependencyVariablesManager();
			ControlsManager = new ControlsManager();
			if (!IsLight)
			{
				SectioningManager = new SectioningManager();
				PricingManager = new PricingManager();
			}
		}

		/// <summary>
		/// Initialize this class from DTO model data.
		/// </summary>
		/// <param name="unitSelectionAndPricingDto">Selection and pricing DTO model data to initialize from.</param>
		/// <param name="setDefaultValues">Should default values be set during initialization?</param>
		/// <param name="logInformation">Should the engine log information during usage?</param>
		/// <param name="canUserSeePricing">Should the calculated prices be visible to the user? If true, the pricing changed event is raise, otherwise the event is not raised.</param>
		/// <param name="proPricingMode">Should the prices be calculated by demand or automatically? If true, the prices will be calculated by demand.</param>
		/// <remarks>The standard Initialize() method is also called from here.</remarks>
		public void InitializeFromDtoModel(UnitSelectionAndPricingDto unitSelectionAndPricingDto,
			bool setDefaultValues = true, bool logInformation = false, bool canUserSeePricing = true, string vendorCountryCode = null, bool proPricingMode = true, bool isLight = false)
		{
			IsLight = isLight;
			ControlsManager.IsInitializing = true;
			ControlsManager.SetInitializationState(true);
			_unitSelectionAndPricingDto = unitSelectionAndPricingDto;
			//CreateFromInitialDtoModel(setDefaultValues, logInformation, canUserSeePricing, vendorCountryCode);
			InternalInitialization(false, setDefaultValues, logInformation, canUserSeePricing, vendorCountryCode, proPricingMode);
			// Create the initial state which can be used to determine if the configuration has any unsaved changes.
			_projectLineItemDisplayDto = ExtractProjectLineItemDisplayDto();
			ControlsManager.IsInitializing = false;
			ControlsManager.SetInitializationState(false);
		}

		/// <summary>
		/// Initialize this class from DTO model data, but setting it to already configured data.
		/// </summary>
		/// <param name="unitSelectionAndPricingDto">Selection and pricing DTO model data to initialize from</param>
		/// <param name="projectLineItemDisplayDto">Project line item DTO containing the already configured data to set variables to.</param>
		/// <param name="setDefaultValues">Should default values be set during initialization?</param>
		/// <param name="logInformation">Should the engine log information during usage?</param>
		/// <param name="canUserSeePricing">Should the calculated prices be visible to the user? If true, the pricing changed event is raise, otherwise the event is not raised.</param>
		/// <param name="proPricingMode">Should the prices be calculated by demand or automatically? If true, the prices will be calculated by demand.</param>
		public void InitializeFromDtoModelForEdit(UnitSelectionAndPricingDto unitSelectionAndPricingDto,
			ProjectLineItemDisplayDto projectLineItemDisplayDto,
			bool setDefaultValues = true, bool logInformation = false, bool canUserSeePricing = true, string vendorCountryCode = null, bool proPricingMode = true, IList<string> selectWorksAccessories = null,
			bool isLight = false)
		{
			IsLight = isLight;
			ControlsManager.IsInitializing = true;
			ControlsManager.SetInitializationState(true);
			_projectLineItemDisplayDto = projectLineItemDisplayDto;
			CleanCurrentProjectLineItemDisplayDto();
			_unitSelectionAndPricingDto = unitSelectionAndPricingDto;

			InternalInitialization(true, setDefaultValues, logInformation, canUserSeePricing, vendorCountryCode, proPricingMode);

			//mark accessories that might have been imported from SelectWorks
			if (selectWorksAccessories?.Count > 0)
			{
				var isSelectWorksItemConfVar = _projectLineItemDisplayDto.ConfigurationVariables.FirstOrDefault(x => x.AccessoryName == SelectWorksItemVarName);
				if (isSelectWorksItemConfVar != null && (bool.TryParse(isSelectWorksItemConfVar.AccessoryValue.ToString(), out bool isSelectWorksItem) && isSelectWorksItem))
				{
					foreach (var accessoryName in selectWorksAccessories)
					{
						var control = ControlsManager.FindControlByName(accessoryName);
						if (control != null)
							control.SelectWorksImported = true;
					}
				}
			}
			ControlsManager.IsInitializing = false;
			ControlsManager.SetInitializationState(false);
		}

		private void CleanCurrentProjectLineItemDisplayDto()
		{
			if (_projectLineItemDisplayDto is null) return;

			// Clean up the dto by clearing some values that need to be replaced by the current configuration during extraction
			// Some models never update these values in the engine, so they never update the dto with the empty values, hence the need to do it here initially
			_projectLineItemDisplayDto.Dimensions = "";
			_projectLineItemDisplayDto.Sections = "";
			_projectLineItemDisplayDto.Variances = "";
		}

		private void InternalInitialization(bool forEditing = false, bool setDefaultValues = true, bool logInformation = false,
			bool canUserSeePricing = true, string vendorCountryCode = null, bool proPricingMode = true)
		{
			CreateFromInitialDtoModel(setDefaultValues, logInformation, canUserSeePricing, vendorCountryCode, proPricingMode);
			if (forEditing)
			{
				SetValuesFromPreviousConfiguration(_projectLineItemDisplayDto.ConfigurationVariables, _projectLineItemDisplayDto.CustomVariants, setDefaultValues);
				if (canUserSeePricing && !_isSelectWorksEngine && !proPricingMode && !IsLight) InternalRunPricing();
				UpdateOptionStrings();
			}
			if (!setDefaultValues)
			{
				// If it was set to not apply defaults during initialization, most likely due to editing or testing
				// reset the controls back to allow setting default values when the user makes a change
				ControlsManager.SetCanApplyDefaultValues(true);
			}
			ControlsManager.SetAllControlsToWithinValidationProcedure(true);
			ControlsManager.SetDefaultValues(DependencyVariablesManager);
			// Commented this out since we do not want to set mandatory options during model loading - Per Julian
			//ControlsManager.SetMandatoryOptions(DependencyVariablesManager);
			ControlsManager.SetAllControlsToWithinValidationProcedure(false);
			if (!_hasRunAllRules) RunValidation();
		}

		private void SetValuesFromPreviousConfiguration(IList<ProjectLineItemConfigurationDto> configurationDtos, IList<CustomVariantDto> customVariantDtos, bool setDefaultValues = true)
		{
			LineNumber = _projectLineItemDisplayDto.LineNumber;
			Quantity = Convert.ToInt32(_projectLineItemDisplayDto.Quantity);
			SetValuesFromConfigurationInternal(configurationDtos, customVariantDtos, setDefaultValues);
		}

		/// <summary>
		/// Set UIOptions Selected Values from configured data.
		/// </summary>
		public void SetValuesFromConfiguration(IList<ProjectLineItemConfigurationDto> configurationDtos, IList<CustomVariantDto> customVariantDtos, bool setDefaultValues = true)
		{
			SetValuesFromConfigurationInternal(configurationDtos, customVariantDtos, setDefaultValues);
		}

		private void SetValuesFromConfigurationInternal(IList<ProjectLineItemConfigurationDto> configurationDtos, IList<CustomVariantDto> customVariantDtos, bool setDefaultValues = true)
		{
			// Sort it by the step id to make sure it will run the rules accordingly
			configurationDtos = configurationDtos.SortLineItemConfigurationDtos();
			// Should we keep the special override status at the end of this method? Default is no.
			var keepSpecialOverride = false;

			//check if item has special override (regular model without any pricing and validation rules)
			var isSpecialOverride = configurationDtos.FirstOrDefault(x => x.AccessoryName.Equals(SpecialOverrideVarName, StringComparison.OrdinalIgnoreCase));

			//check if item is imported from SelectWorks and has not been priced yet
			var isSelectWorksItem = configurationDtos.FirstOrDefault(x => x.AccessoryName.Equals(SelectWorksItemVarName, StringComparison.OrdinalIgnoreCase));

			// check if item is special model
			var isSpecialModel = configurationDtos.FirstOrDefault(x => x.AccessoryName.Equals(SpecialModelVarName, StringComparison.OrdinalIgnoreCase));
			var isSelectWorks = false;
			if (isSelectWorksItem != null)
			{
				_ = bool.TryParse(isSelectWorksItem.AccessoryValue.ToString(), out isSelectWorks);
			}

			var isSpecialM = false;
			if (isSpecialModel != null)
			{
				_ = bool.TryParse(isSpecialModel.AccessoryValue.ToString(), out isSpecialM);
			}

			if (isSpecialOverride != null)
			{
				if (bool.TryParse(isSpecialOverride.AccessoryValue.ToString(), out bool isOverride) && isOverride)
				{
					keepSpecialOverride = isOverride; // Set to what the user setting was
					if (isSelectWorks || isSpecialM)
					{
						_isSpecialOverride = true;
						IsSelectWorksItem = isSelectWorks;
					}
					else
					{
						_isSpecialOverride = true;
						if (!ControlsManager.HasBasePriceVariant())
						{
							var basePrice = configurationDtos.FirstOrDefault(x => x.AccessoryName == ControlsManager.SpecialBasePriceControl);
							if (basePrice != null)
							{
								LogInformation("Found base price configuration value, so adding the control to the UI.");
								var basePriceControl = ControlsManager.AddBasePriceVariant();
								basePriceControl.OnValueChangeEvent += OnValueChangeEvent;
								var basePriceValue = basePrice.AccessoryValue.ToUSCultureDouble();
								basePriceControl.SelectedValueInternal = basePriceValue;
								DependencyVariablesManager.AddValueToDependencyVariable(ControlsManager.SpecialBasePriceControl, basePriceValue, 0);
							}
						}
						//for special item set all controls visible
						foreach (var uiOption in UISelect)
						{
							uiOption.ShowControl = true;
							uiOption.ValidationErrors.Clear();
							uiOption.UISelectOptions.Clear();
							uiOption.UISelectOptions.AddRange(uiOption.UISelectOptionsFullList);
						}
						InvalidOptions.Clear();
						_setValidStateFromOptions = true;
					}
				}
			}

			// During initializing for an edit, we want to force what the user selected, and only what the user selected
			// And only run validation rules afterwards.
			// The simplest approach for now is to mimic checking the Is Special checkbox to true before setting the values.
			ApplyRuleChanges();//will clear SelectionItemsToChange.OptionsToAdd and SelectionItemsToChangeOptionsToRemove collections set during initialization
			IsSpecialOverride = true;
			var userOptions = new List<string>();
			var configModel = "";
			var modelConfigDto = configurationDtos.FirstOrDefault(c => c.AccessoryName.Equals("model", StringComparison.OrdinalIgnoreCase));
			if (modelConfigDto is not null) configModel = modelConfigDto.AccessoryValue.ToString();
			var somConfigDto = configurationDtos.FirstOrDefault(c => c.AccessoryName.Equals("som", StringComparison.OrdinalIgnoreCase));
			if (somConfigDto is not null) SystemOfMeasure = somConfigDto.AccessoryValue.ToString();
			var differentCulture = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
			foreach (var projectLineItemConfigurationDto in configurationDtos)
			{
				if (projectLineItemConfigurationDto.AccessoryName.Equals("som", StringComparison.OrdinalIgnoreCase))
				{
					// Already handled above
					continue;
				}

				if (projectLineItemConfigurationDto.AccessoryName.Equals("model", StringComparison.OrdinalIgnoreCase))
				{
					// ignore this since it is already set from the model rules
					if (string.IsNullOrWhiteSpace(configModel)) configModel = projectLineItemConfigurationDto.AccessoryValue.ToString();
					continue;
				}

				if (projectLineItemConfigurationDto.AccessoryName.EndsWith("_Description", StringComparison.OrdinalIgnoreCase) ||
					projectLineItemConfigurationDto.AccessoryName.Equals("IsPricingIncluded", StringComparison.OrdinalIgnoreCase) ||
					projectLineItemConfigurationDto.AccessoryName.Equals("ModelRulesVerNum", StringComparison.OrdinalIgnoreCase) ||
					projectLineItemConfigurationDto.AccessoryName.Equals(SpecialOverrideVarName, StringComparison.OrdinalIgnoreCase) ||
					projectLineItemConfigurationDto.AccessoryName.Equals(SelectWorksItemVarName, StringComparison.OrdinalIgnoreCase) ||
					projectLineItemConfigurationDto.AccessoryName.Equals("CtryCode", StringComparison.OrdinalIgnoreCase))
				{
					// do not bring these in since they will be recreated again during extraction (and actuator selection if it is actuator related)
					continue;
				}

				var control = ControlsManager.FindControlByName(projectLineItemConfigurationDto.AccessoryName);
				var accValue = projectLineItemConfigurationDto.AccessoryValue;
				// determine if the value is numeric and should be converted to the current culture (if different than invariant)
				// first change the current thread to invariant, since numbers in the db are stored this way
				// then extract the value as an actual number type (if numeric), which when outside of this "using" will convert to current culture
				using (InvariantCultureScope cultureScope = new(differentCulture))
				{
					accValue = accValue.ConvertToNumericObject();
				}

				// If the accessory is not a control, just update the dependency variables if it is the same model
				// Otherwise we will assume it is not needed and let the validation process create what it needs later
				if (control is null)
				{
					if (Model?.Equals(configModel) == true)
					{
						DependencyVariablesManager.AddValueToDependencyVariable(projectLineItemConfigurationDto.AccessoryName, accValue, 0);
					}
					continue;
				}
				else if (control.IsSelectionTypeControl && control.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == projectLineItemConfigurationDto.AccessoryValue?.ToString()) is null)
				{
					if (!control.IsCustomPaintCodeControl)
					{
						// The value does not exist for this model, so do not set it
						continue;
					}
					// else allow to continue forward to enter the value for the user since it might not have already existed, such as when editing someone else's configuration
				}

				userOptions.Add(control.ValueName);

				// Call this to mimic the user setting the value instead of the system setting the value
				ControlsManager.ResetUserSetStatus(userOptions, control.ValueName);
				// Changed to internal property to handle an infinite loop issue
				//control.SelectedValue = projectLineItemConfigurationDto.AccessoryValue;
				if (control.IsSelectionTypeControl)
				{
					// Keep as the original value since it is a code even if numeric
					control.SelectedValueInternal = projectLineItemConfigurationDto.AccessoryValue;
				}
				else
				{
					// Use the converted value if numeric to ensure current culture
					control.SelectedValueInternal = accValue;
				}

				// Setting this will let the engine know that this value was a user selection when originally selected
				control.WasUserSet = true;
			}

			foreach (var customVariant in customVariantDtos.OrderBy(x => x.Step))
			{
				ControlsManager.AddCustomVariant(customVariant);
			}

			// Apply any default value changes from rules that occurred during the above processes
			if (setDefaultValues) ControlsManager.ApplyDefaultValues();
			// Reset all controls to no longer be user set, which will allow for the user to make changes and other rules to work as intended going forward
			ControlsManager.ResetUserSetStatus();
			// Once we are done, if the item is not a special override item, we will reset back to False, forcing the validation and pricing process to run now.
			if (!keepSpecialOverride)
			{
				IsSpecialOverride = false;
			}
			else if (!isSelectWorks && !isSpecialM)
			{
				InvalidOptions.Clear();
				_setValidStateFromOptions = true;
			}
		}

		/// <summary>
		/// Attempt to apply a change to a configuration value. This is only for the global change process.
		/// </summary>
		/// <param name="changes">Configuration changes to apply.</param>
		/// <returns>Returns which changes succeeded and which ones failed.</returns>
		public (bool succeeded, string message, string compactSuccessMessage, string compactFailedMessage) ChangeConfigurationValues(IList<ProjectLineItemConfigurationDto> changes)
		{
			var sb = new StringBuilder();
			var sbCompactSuccess = new StringBuilder();
			var sbCompactFailed = new StringBuilder();
			var stagedChanges = new List<ProjectLineItemConfigurationDto>();
			var status = false;

			sbCompactSuccess.Append("Successfully updated values for ");
			sbCompactFailed.Append("Failed to update values for ");
			//check if the user is changing the special checkbox value
			var isSpecialOverride = changes.FirstOrDefault(x => x.AccessoryName.Equals(SpecialOverrideVarName, StringComparison.OrdinalIgnoreCase));
			if (isSpecialOverride is not null)
			{
				// User is changing the IsSpecial state
				if (bool.TryParse(isSpecialOverride.AccessoryValue.ToString(), out bool isOverride))
				{
					if (isOverride) IsSpecialOverride = true;
					else
					{
						// if this is also a SelectWorks imported item, we will want to remove that along with special
						IsSpecialOverride = false;
						IsSelectWorksItem = false;
					}
					sb.AppendLine($"Successfully updated {isSpecialOverride.AccessoryName} to {isSpecialOverride.AccessoryValue.ToString()}.");
					sbCompactSuccess.Append($"{isSpecialOverride.AccessoryName},");
					status = true;
				}
				else
				{
					sb.AppendLine($"Unable to apply the value of {isSpecialOverride.AccessoryValue?.ToString()} to accessory {isSpecialOverride.AccessoryName} because the value is not in the correct format.");
					sbCompactFailed.Append($"{isSpecialOverride.AccessoryName},");
				}

				// Remove it now that we have already handled it
				changes.Remove(isSpecialOverride);
			}

			//check if the user is changing the system of measure
			var som = changes.FirstOrDefault(x => x.AccessoryName.Equals("som", StringComparison.OrdinalIgnoreCase));
			if (som is not null)
			{
				var curSom = SystemOfMeasure;
				SystemOfMeasure = som.AccessoryValue?.ToString() ?? curSom;
				if (SystemOfMeasure.Equals(som.AccessoryValue?.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					sb.AppendLine($"Successfully updated the system of measure to {SystemOfMeasure}.");
					sbCompactSuccess.Append($"system of measure,");
				}
				else
				{
					sb.AppendLine($"Unable to apply the value of {som.AccessoryValue?.ToString() ?? "(no value)"} to the system of measure because the value is not in the correct format.");
					sbCompactFailed.Append($"system of measure,");
				}
				// Remove it now that it is already handled
				changes.Remove(som);
			}

			foreach (var change in changes)
			{
				var foundControl = ControlsManager.FindControlByName(change.AccessoryName);
				if (foundControl is null)
				{
					// accessory does not exist, but check if it should be ignored
					if (BaseEnumeration.FromDisplayName<PriceTypeEnumeration>(change.AccessoryName, true) is null &&
						BaseEnumeration.FromDisplayName<SizingEnumeration>(change.AccessoryName, true) is null &&
						!change.AccessoryName.EndsWith("_description", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.EndsWith("-assessorybase", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.EndsWith("-addtobase", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Contains("-Unknown price", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Contains("-multiplier", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Contains("-minimumbilling", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("Default", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("CtryCode", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("IsPricingIncluded", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("model", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("ModelRulesVerNum", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("SelectWorksItem", StringComparison.OrdinalIgnoreCase) &&
						!change.AccessoryName.Equals("SystemOfMeasure", StringComparison.OrdinalIgnoreCase))
					{
						sb.AppendLine($"Unable to apply the value of {change.AccessoryValue?.ToString()} because accessory {change.AccessoryName} does not exist for model {Model}.");
						sbCompactFailed.Append($"{change.AccessoryName},");
					}
					continue;
				}
				var differentCulture = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
				var accValue = change.AccessoryValue;
				// determine if the value is numeric and should be converted to the current culture (if different than invariant)
				// first change the current thread to invariant, since numbers in the db are stored this way
				// then extract the value as an actual number type (if numeric), which when outside of this "using" will convert to current culture
				using (InvariantCultureScope cultureScope = new(differentCulture))
				{
					if (accValue is not null)
						accValue = accValue.ConvertToNumericObject();
				}
				if (foundControl.IsSelectionTypeControl && !string.IsNullOrEmpty(change.AccessoryValue?.ToString()) && foundControl.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == change.AccessoryValue?.ToString()) is null)
				{
					if (!foundControl.IsCustomPaintCodeControl)
					{
						// option within accessory does not exist
						sb.AppendLine($"Unable to apply the value of {change.AccessoryValue?.ToString()} to accessory {change.AccessoryName} because that value does not exist as an option.");
						sbCompactFailed.Append($"{change.AccessoryName},");
						continue;
					}
				}
				if (foundControl.IsSelectionTypeControl && !string.IsNullOrEmpty(change.AccessoryValue?.ToString()) && foundControl.UISelectOptions.FirstOrDefault(o => o.OptionValue == change.AccessoryValue?.ToString()) is null)
				{
					if (!foundControl.IsCustomPaintCodeControl)
					{
						// option within accessory does exist, but is currently unavailable, so we will mark this one to try again later just in case another change enables it
						stagedChanges.Add(change);
						continue;
					}
				}
				if (string.IsNullOrEmpty(change.AccessoryValue?.ToString()))
					sb.AppendLine($"Successfully cleared the value of {change.AccessoryName}.");
				else
					sb.AppendLine($"Successfully updated {change.AccessoryName} to {change.AccessoryValue}.");
				sbCompactSuccess.Append($"{change.AccessoryName},");
				status = true;
				if (foundControl.IsSelectionTypeControl)
				{
					// Keep as the original value since it is a code even if numeric
					foundControl.SelectedValueInternal = change.AccessoryValue;
				}
				else
				{
					// Use the converted value if numeric to ensure current culture
					foundControl.SelectedValueInternal = accValue;
				}
			}

			if (stagedChanges.Count > 0)
			{
				var secondStagedChanges = new List<ProjectLineItemConfigurationDto>();
				while (stagedChanges.Count > 0)
				{
					for (var i = stagedChanges.Count - 1; i >= 0; i--)
					{
						var stagedChange = stagedChanges[i];
						var foundControl = ControlsManager.FindControlByName(stagedChange.AccessoryName);
						// The control should exist since it did above
						if (foundControl.UISelectOptions.FirstOrDefault(o => o.OptionValue == stagedChange.AccessoryValue?.ToString()) is null)
						{
							// Still not an enabled option, so lets check the rules on this option to see if it needs what we still needs to be applied
							// First we will determine if we already tried this, and if so, then just mark as unsuccessful and move on
							if (secondStagedChanges.FirstOrDefault(c => c.AccessoryName == stagedChange.AccessoryName && c.AccessoryValue == stagedChange.AccessoryValue) is null)
							{
								var option = foundControl.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == stagedChange.AccessoryValue?.ToString());
								// The option should exist since it did above
								foreach (var rule in option.Dependencies)
								{
									var names = rule.JsonRule.GetVariableNamesFromJsonRule();
									if (stagedChanges.Any(c => names.Contains(c.AccessoryName)))
									{
										// It might get enabled after running the remaining changes
										secondStagedChanges.Add(stagedChange);
										continue;
									}
								}
							}

							sb.AppendLine($"Unable to apply the value of {stagedChange.AccessoryValue?.ToString()} to accessory {stagedChange.AccessoryName} because that value is not an available option.");
							sbCompactFailed.Append($"{stagedChange.AccessoryName},");
							stagedChanges.RemoveAt(i);
							continue;
						}
						if (string.IsNullOrEmpty(stagedChange.AccessoryValue?.ToString()))
							sb.AppendLine($"Successfully cleared the value of {stagedChange.AccessoryName}.");
						else
							sb.AppendLine($"Successfully updated {stagedChange.AccessoryName} to {stagedChange.AccessoryValue}.");
						sbCompactSuccess.Append($"{stagedChange.AccessoryName},");
						status = true;
						foundControl.SelectedValueInternal = stagedChange.AccessoryValue;
						stagedChanges.RemoveAt(i);
					}
				}
			}

			if (sbCompactSuccess.ToString() == "Successfully updated values for ")
			{
				sbCompactSuccess.Clear();
			}
			else
			{
				var msg = sbCompactSuccess.ToString();
				msg = msg[..^1];
				sbCompactSuccess.Clear();
				sbCompactSuccess.Append(msg);
				sbCompactSuccess.Append(" accessories.");
			}

			if (sbCompactFailed.ToString() == "Failed to update values for ")
			{
				sbCompactFailed.Clear();
			}
			else
			{
				var msg = sbCompactFailed.ToString();
				msg = msg[..^1];
				sbCompactFailed.Clear();
				sbCompactFailed.Append(msg);
				sbCompactFailed.Append(" accessories.");
			}

			return (status, sb.ToString(), sbCompactSuccess.ToString(), sbCompactFailed.ToString());
		}

		private void CreateFromInitialDtoModel(bool setDefaultValues = true, bool logInformation = false, bool canUserSeePricing = true, string vendorCountryCode = null, bool proPricingMode = true)
		{
			LogInformation($"Entered {nameof(CreateFromInitialDtoModel)} method");

			_unitSelectionAndPricingDto.PopulateEngineFromDto(this, _logInformation ? _logger : null, vendorCountryCode);
			_hasRunAllRules = false; // reset it back since the intialization process of SOM can trigger it before any controls exist yet

			Initialize(setDefaultValues, logInformation, canUserSeePricing, proPricingMode);
		}

		/// <summary>
		/// Initialize this class to setup necessary event handling as well as run initial setup processes for the unit.
		/// </summary>
		/// <param name="setDefaultValues">Should default values be set during initialization?</param>
		/// <param name="logInformation">Should the engine log information during usage?</param>
		/// <param name="canUserSeePricing">Should the calculated prices be visible to the user? If true, the pricing changed event is raise, otherwise the event is not raised.</param>
		/// <param name="proPricingMode">Should the prices be calculated by demand or automatically? If true, the prices will be calculated by demand.</param>
		public virtual void Initialize(bool setDefaultValues = true, bool logInformation = false, bool canUserSeePricing = true, bool proPricingMode = true)
		{
			_logInformation = logInformation;
			if (!IsLight)
			{
				_canUserSeePricing = canUserSeePricing;
				ProPricingMode = proPricingMode;
				PricingManager.CanUserSeePricing = _canUserSeePricing;
			}

			// Make sure the controls are in order
			ControlsManager.SortControls();
			ControlsManager.SetCanApplyDefaultValues(setDefaultValues);

			SetupEvents();

			if (!IsLight)
			{
				PricingManager.DependencyVariablesManager = DependencyVariablesManager;
				PricingManager.ClearPricingVariables(ModelMultiplier);
			}

			InitializeStartingVariables(setDefaultValues);

			if (canUserSeePricing && !_isSelectWorksEngine && !proPricingMode && !IsLight) InternalRunPricing();
			UpdateOptionStrings();
		}



		protected void InitializeStartingVariables(bool setDefaultValues)
		{
			// Initialize any starting variables
			Quantity = 0;
			DependencyVariablesManager.AddValueToDependencyVariable("Default", "DefaultValueNotChanged", 0);

			// Set controls show state
			ControlsManager.SetShowStates(DependencyVariablesManager);

			// Set default values for each control
			if (!setDefaultValues) return;
			ControlsManager.SetDefaultValues(DependencyVariablesManager);
		}

		protected void SetupEvents()
		{
			// Setup the events
			if (!IsLight) PricingManager.PricingVariablesChangedEvent += OnPricingVariablesChangedEvent;

			foreach (var uiOption in UISelect)
			{
				ControlsManager.AddControlToIndex(uiOption.ValueName, uiOption.UIControlType, uiOption.StepID);
				uiOption.engineLogger = _logger;
				uiOption.OnValueChangeEvent += OnValueChangeEvent;
				uiOption.OnInternalValueChangeEvent += OnInternalValueChangeEvent;
				uiOption.ShowControlEvent += OnShowControlEvent;
				uiOption.EnableControlEvent += OnEnableControlEvent;
				//uiOption.NeedTokenEvent += OnNeedTokenEvent;
				uiOption.NeedCustomPaintCodesEvent += UiOption_NeedCustomPaintCodesEvent;
				uiOption.SaveCustomPaintCodesEvent += UiOption_SaveCustomPaintCodesEvent;
				uiOption.CurveChangedEvent += OnPerformanceCurveChangeOrAddEvent;
				uiOption.ButtonClickedEvent += UiOption_ButtonClickedEvent;
				uiOption.Initialize(_logInformation);

				if (!(uiOption.DependentControls?.Count > 0)) continue;
				SetupDependentEvents(uiOption);
			}
		}

		private void SetupDependentEvents(UIOption uiOption)
		{
			// Sort the dependent controls while we are here
			uiOption.DependentControls = uiOption.DependentControls.OrderBy(d => d.StepID).ToList();
			foreach (var uiChildOption in uiOption.DependentControls)
			{
				ControlsManager.AddControlToIndex(uiChildOption.ValueName, uiChildOption.UIControlType, uiChildOption.StepID,
					uiOption.ValueName, uiOption.StepID);
				uiChildOption.engineLogger = _logger;
				uiChildOption.OnValueChangeEvent += OnValueChangeEvent;
				uiChildOption.OnInternalValueChangeEvent += OnInternalValueChangeEvent;
				uiChildOption.ShowControlEvent += OnShowControlEvent;
				uiChildOption.EnableControlEvent += OnEnableControlEvent;
				//uiChildOption.NeedTokenEvent += OnNeedTokenEvent;
				uiChildOption.NeedCustomPaintCodesEvent += UiOption_NeedCustomPaintCodesEvent;
				uiChildOption.SaveCustomPaintCodesEvent += UiOption_SaveCustomPaintCodesEvent;
				uiChildOption.CurveChangedEvent += OnPerformanceCurveChangeOrAddEvent;
				uiChildOption.ButtonClickedEvent += UiOption_ButtonClickedEvent;
				uiChildOption.Initialize(_logInformation);

				if (!(uiChildOption.DependentControls?.Count > 0)) continue;
				SetupDependentEvents(uiChildOption);
			}
		}

		private void CleanupEvents()
		{
			if (!IsLight) PricingManager.PricingVariablesChangedEvent -= OnPricingVariablesChangedEvent;
			foreach (var uiOption in UISelect)
			{
				uiOption.OnValueChangeEvent -= OnValueChangeEvent;
				uiOption.OnInternalValueChangeEvent -= OnInternalValueChangeEvent;
				uiOption.ShowControlEvent -= OnShowControlEvent;
				uiOption.EnableControlEvent -= OnEnableControlEvent;
				//uiOption.NeedTokenEvent -= OnNeedTokenEvent;
				uiOption.NeedCustomPaintCodesEvent -= UiOption_NeedCustomPaintCodesEvent;
				uiOption.SaveCustomPaintCodesEvent -= UiOption_SaveCustomPaintCodesEvent;
				uiOption.CurveChangedEvent -= OnPerformanceCurveChangeOrAddEvent;
				uiOption.ButtonClickedEvent -= UiOption_ButtonClickedEvent;

				if (!(uiOption.DependentControls?.Count > 0)) continue;
				CleanupDependentEvents(uiOption);
			}
		}

		private void CleanupDependentEvents(UIOption uiOption)
		{
			foreach (var uiChildOption in uiOption.DependentControls)
			{
				uiChildOption.OnValueChangeEvent -= OnValueChangeEvent;
				uiChildOption.OnInternalValueChangeEvent -= OnInternalValueChangeEvent;
				uiChildOption.ShowControlEvent -= OnShowControlEvent;
				uiChildOption.EnableControlEvent -= OnEnableControlEvent;
				//uiChildOption.NeedTokenEvent -= OnNeedTokenEvent;
				uiChildOption.NeedCustomPaintCodesEvent -= UiOption_NeedCustomPaintCodesEvent;
				uiChildOption.SaveCustomPaintCodesEvent -= UiOption_SaveCustomPaintCodesEvent;
				uiChildOption.CurveChangedEvent -= OnPerformanceCurveChangeOrAddEvent;
				uiChildOption.ButtonClickedEvent -= UiOption_ButtonClickedEvent;

				if (!(uiChildOption.DependentControls?.Count > 0)) continue;
				CleanupDependentEvents(uiChildOption);
			}
		}

		private void OnPricingVariablesChangedEvent(object sender, PricingChangedEventArgs e)
		{
			foreach (var ePricingVariable in e.PricingVariables)
			{
				DependencyVariablesManager.AddValueToDependencyVariable(ePricingVariable.Key.DisplayName, ePricingVariable.Value, 0);
			}
			if (e.RaiseEventFurther) PricingVariablesChanged(e);
		}

		/// <summary>
		/// When a user changes a value.
		/// </summary>
		/// <param name="sender">The object that triggered this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnValueChangeEvent(object sender, ValueChangeEventArgs e)
		{
			Stopwatch userValueChange = new();
			userValueChange.Start();
			DependencyVariablesManager.ClearVariableSteps();
			//_changedAccessoryNames.Clear();

			// Reset the user set status for each control, except for the one that just got changed
			UserChangedControlName = e.OptionName;
			ControlsManager.ResetUserSetStatus(e.OptionName);
			ControlsManager.ResetAllControlsStatusToDefault();

			ValueChangeEventMethod(e);

			if (!_isSelectWorksEngine && !ProPricingMode && !IsLight) InternalRunPricing();

			UpdateOptionStrings();
			userValueChange.Stop();
			LogInformation($"User change event for {e.OptionName} ({e.OptionValue}) took {userValueChange.Elapsed.Milliseconds} milliseconds to complete.", LogLevel.Information);
		}

		/// <summary>
		/// When the internal system changes a value.
		/// </summary>
		/// <param name="sender">The object that triggered this event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnInternalValueChangeEvent(object sender, ValueChangeEventArgs e)
		{
			ValueChangeEventMethod(e);
			UpdateOptionStrings();
		}

		private void OnPerformanceCurveChangeOrAddEvent(object sender, CurveChangedEventArgs e)
		{
			DependencyVariablesManager.ApplyPerformanceCurveChanges(e.ChangedOrAddedLimits);
		}

		private void OnShowControlEvent(object sender, ShowControlEventArgs e)
		{
			SetShow(new List<ShowControlEventArgs> { e });
		}

		private void OnEnableControlEvent(object sender, EnableControlEventArgs e)
		{
			SetEnable(new List<EnableControlEventArgs>() { e });
		}

		private void UiOption_ButtonClickedEvent(object sender, string e)
		{
			var uiOption = ControlsManager.FindControlByName(e);
			uiOption.PerformButtonClickRoutine(DependencyVariablesManager.DependencyVariables, ControlsManager);
		}

		private void UiOption_SaveCustomPaintCodesEvent(object sender, SaveCustomPaintCodesEventArgs e)
		{
			SaveCustomPaintCodes(e);
		}

		private void UiOption_NeedCustomPaintCodesEvent(object sender, NeedCustomPaintCodesEventArgs e)
		{
			_needTokenControl = e.ControlName;
			NeedCustomPaintCodes();
		}

		public void SetCustomPaintColors(IEnumerable<CustomPaintColorDto> customPaintColorDtos)
		{
			var control = ControlsManager.FindControlByName(_needTokenControl);
			_needTokenControl = "";
			control?.SetInitialCustomPaintCodesFromDtos(customPaintColorDtos);
		}

		public void AddCustomVariant(CustomVariantDto customVariantDto)
		{
			ControlsManager.AddCustomVariant(customVariantDto);
			if (!ProPricingMode && !IsLight) InternalRunPricing();
			UpdateOptionStrings();
		}

		public void UpdateCustomVariant(string valueName, CustomVariantDto customVariantDto)
		{
			ControlsManager.UpdateCustomVariant(valueName, customVariantDto);
			if (!ProPricingMode && !IsLight) InternalRunPricing();
			UpdateOptionStrings();
		}

		public void DeleteCustomVariant(string valueName)
		{
			ControlsManager.DeleteCustomVariant(valueName);
			if (!ProPricingMode && !IsLight) InternalRunPricing();
			UpdateOptionStrings();
		}

		/// <summary>
		/// Rebuild the options state for this engine from a concatenated string of options, including each option's step Id and value name.
		/// </summary>
		/// <param name="optionsStringWithStepIdsAndValueNames">Concatenated string of options with each step Id and value name.</param>
		public List<IApiResultMessageModel> RebuildOptionsState(string optionsStringWithStepIdsAndValueNames)
		{
			return new List<IApiResultMessageModel>();
		}

		public void ResetEngineToDefaults(bool editingUnit = false)
		{
			DependencyVariablesManager.DependencyVariables.Clear();
			SetupManagers();
			InternalInitialization(editingUnit, true, _logInformation, _canUserSeePricing, VendorCountryCode, ProPricingMode);
		}

		private void PropagateSom()
		{
			// Set all controls to the system of measure
			ControlsManager.PropagateSom(SystemOfMeasure);

			// Raise an event that a value has been changed
			ValueChangeEventMethod(new ValueChangeEventArgs() { ApplyChangesWhenDone = true, IsFromCustomPaintCodeValidation = false, IsFromValidationProcedure = false, OptionName = nameof(SystemOfMeasure), OptionValue = _systemOfMeasure, RunRules = true, StepId = 0 });
		}

		internal Dictionary<string, object> GetSortedNotNullDependencyVariables()
		{
			var unsortedVariables = DependencyVariablesManager.GetNotNullDependencyVariables();
			return unsortedVariables.GetSortedByControlId(ControlsManager);
		}



		/// <summary>
		/// Extract the ProjectLineItemDisplayDto from the engine after configuration is complete.
		/// </summary>
		public ProjectLineItemDisplayDto ExtractProjectLineItemDisplayDto()
		{
			return this.ExtractProjectLineItemDisplayDtoEx(_projectLineItemDisplayDto, IsLight);
		}

		public bool IsConfigurationDirty()
		{
			if (_projectLineItemDisplayDto is null) return true;
			var currentState = ExtractProjectLineItemDisplayDto();
			return !_projectLineItemDisplayDto.EqualsProjectLineItemDto(currentState);
		}

		private void RunValidationProcedure()
		{
			LogInformation($"Entered {nameof(RunValidationProcedure)} method.");

			//todo: delete time tracking
			Stopwatch entireValidation = new();
			entireValidation.Start();

			ControlsManager.SetAllControlsToWithinValidationProcedure(true);
			var result = true;
			InvalidOptions.Clear();
			GeneralMessages.Clear();

			ControlsManager.SetShowStates(DependencyVariablesManager);

			foreach (var uiOption in UISelect.Where(o => o.ShowControl))
			{
				var controlType = BaseEnumeration.FromDisplayNameOrdinalIgnoreCase<UiControlTypeEnumeration>(uiOption.UIControlType);
				if (controlType == UiControlTypeEnumeration.Button) continue; // Button controls only run when they are clicked by the user.

				//todo: delete time tracking
				Stopwatch oneOption = new();
				oneOption.Restart();

				var tmpResult = uiOption.RunValidation(DependencyVariablesManager.DependencyVariables.ConvertToDynamic(_logInformation ? _logger : null));//,_changedAccessoryNames);

				oneOption.Stop();
				LogInformation($"Timing - Validation for {uiOption.UIDescription}: rules count = {uiOption.ValidationJsonRules.Count}, time = {oneOption.Elapsed.Milliseconds}");

				result &= tmpResult;
				if (uiOption.GeneralMessages.Count > 0)
				{
					foreach (var generalMessage in uiOption.GeneralMessages)
					{
						GeneralMessages.Add(generalMessage);
					}
				}
				if (tmpResult) continue;
				SetErrorState(uiOption);
			}

			if (SectionMaxMin is not null)
			{
				LogInformation("Checking if sectioning size is valid.");
				var tmpSectionMaxMinResult = SectionMaxMin.IsSizeValid(DependencyVariablesManager.DependencyVariables, _logInformation ? _logger : null);
				result &= tmpSectionMaxMinResult.Item1;
			}

			entireValidation.Stop();
			LogInformation($"Timing - Entire validation time = {entireValidation.Elapsed.Milliseconds}");

			_setValidStateFromOptions = result;
			ControlsManager.SetAllControlsToWithinValidationProcedure(false);
		}

		private void SetErrorState(UIOption uiOption)
		{
			foreach (var invalidRule in uiOption.ValidationErrors)
			{
				AddToInvalidOptionsCollection(invalidRule);
				if (invalidRule.ControlName.Equals(uiOption.ValueName))
				{
					uiOption.Status = UIControlStatusEnumerations.Error;
				}
				else
				{
					var control = ControlsManager.FindControlByName(invalidRule.ControlName);
					if (control == null) continue;
					control.Status = UIControlStatusEnumerations.Error;
				}
			}
		}

		private void RunOnSelectRules(string controlName)
		{
			LogInformation($"Entered {nameof(RunOnSelectRules)} method.");
			var control = ControlsManager.FindControlByName(controlName);
			control?.RunOnSelectRules(DependencyVariablesManager.DependencyVariables, ControlsManager, IsSpecialOverride);
		}

		private void SetShow(IEnumerable<ShowControlEventArgs> showControlNames)
		{
			foreach (var controlName in showControlNames)
			{
				// Check if it is a child control first
				var control = ControlsManager.FindControlByName(controlName.ControlName, controlName.ParentControlName);
				if (control is null)
				{
					// Check if it is not really a child control
					control = ControlsManager.FindControlByName(controlName.ControlName);
				}
				if (control is null) continue;
				control.ShowControl = controlName.ShowControl;
				control.ShowControlSetByRule = true;
				LogInformation($"Within {nameof(SetShow)} method, the value of enabled for control {control.ValueName} is {control.EnableControl}", LogLevel.Warning);
				ControlVisibilityStateChanged(new ControlVisibilityStateChangedArgs
				{
					StepID = control.StepID,
					ValueName = control.ValueName,
					IsVisible = controlName.ShowControl,
					IsEnabled = control.EnableControl
				});
			}
		}

		private void SetEnable(IEnumerable<EnableControlEventArgs> enableControlNames)
		{
			foreach (var controlName in enableControlNames)
			{
				// Check if it is a child control first
				var control = ControlsManager.FindControlByName(controlName.ControlName, controlName.ParentControlName);
				if (control is null)
				{
					// Check if it is not really a child control
					control = ControlsManager.FindControlByName(controlName.ControlName);
				}
				if (control is null) continue;
				LogInformation($"Setting the value of enabled for control {control.ValueName} from {control.EnableControl} to {controlName.EnableControl}", LogLevel.Warning);
				control.EnableControl = controlName.EnableControl;
				ControlVisibilityStateChanged(new ControlVisibilityStateChangedArgs
				{
					StepID = control.StepID,
					ValueName = control.ValueName,
					IsVisible = control.ShowControl,
					IsEnabled = controlName.EnableControl
				});
			}
		}

		private void AddToInvalidOptionsCollection(InvalidOption invalidOption)
		{
			var foundItem = InvalidOptions.FirstOrDefault(i => i.ControlName == invalidOption.ControlName);
			if (foundItem is not null)
			{
				// This control already has a message so overwrite it with the new one.
				InvalidOptions.Remove(foundItem);
			}

			InvalidOptions.Add(invalidOption);
		}

		bool _hasRunAllRules = false;
		private void RunAllRules(string optionName)
		{
			_hasRunAllRules = true;
			LogInformation($"Entered {nameof(RunAllRules)} ({optionName ?? "NULL"}) method.", LogLevel.Warning);
			if (!string.IsNullOrWhiteSpace(optionName))
			{
				RunOnSelectRules(optionName);
			}
			if (IsSpecialOverride && !IsSpecialModel && !IsLight)
			{
				var basePriceControl = ControlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
				// force running validation rules on this control even when special
				var result = basePriceControl?.RunValidation(DependencyVariablesManager.DependencyVariables.ConvertToDynamic(_logInformation ? _logger : null)) ?? false;

				if (result)
				{
					PricingManager.ClearPricingVariables(1);
					InvalidOptions.Clear();
					if (IsSelectWorksItem) return;
					if (PricingManager.CalculatePricingForSpecialOverride(ControlsManager))
					{
						LogInformation($"{nameof(RunAllRules)} - Base Price is valid");
						_setValidStateFromOptions = true;
						return;
					}
				}
				InvalidOptions.Add(new InvalidOption() { ControlName = basePriceControl.ValueName, ControlUIDescription = basePriceControl.UIDescription, Message = "Base Price is invalid or missing and must be entered!" });
				LogInformation($"{nameof(RunAllRules)} - Base Price is invalid or missing and must be entered!");
				_setValidStateFromOptions = false;

				////validate base price for special override
				//PricingManager.ClearPricingVariables(1);
				//InvalidOptions.Clear();
				//if (!PricingManager.CalculatePricingForSpecialOverride(ControlsManager))
				//{
				//    LogInformation($"{nameof(RunAllRules)} - Base Price is invalid!");
				//    //var basePriceControl = ControlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
				//    if (basePriceControl != null)
				//    {
				//        InvalidOptions.Add(new InvalidOption() { ControlName = basePriceControl.ValueName, ControlUIDescription = basePriceControl.UIDescription, Message = "Base price must be entered" });
				//        LogInformation($"{nameof(RunAllRules)} - added 'base price' in InvalidOptions collection");
				//    }
				//    _setValidStateFromOptions = false;
				//}
				//else
				//{
				//    LogInformation($"{nameof(RunAllRules)} - Base Price is valid");
				//    _setValidStateFromOptions = true;
				//}
				return;
			}

			//validate overall size before running section rules
			var isSizingValid = true;
			if (OverallMaxMin is not null && !IsLight)
			{
				LogInformation("Checking if overall size is valid.");
				OverallValidationErrors.Clear();
				_setValidStateForOverall = true;
				var isSizingValidResult = OverallMaxMin.IsSizeValid(DependencyVariablesManager.DependencyVariables, _logInformation ? _logger : null);
				isSizingValid = isSizingValidResult.Item1;
				if (!string.IsNullOrWhiteSpace(isSizingValidResult.Item2) && !isSizingValid)
				{
					// Assume validation error
					OverallValidationErrors.Add(isSizingValidResult.Item2);
					_setValidStateForOverall = false;
				}
			}

			if (!IsLight)
			{
				SectioningManager.RunSectionRules(DependencyVariablesManager, ControlsManager, OverallMaxMin, SectionMaxMin, isSizingValid, _logger);
				RunValidationProcedure();
				ControlsManager.SetAllControlsToWithinValidationProcedure(true);
				ControlsManager.SetDefaultValues(DependencyVariablesManager);
				if (!ControlsManager.IsInitializing) ControlsManager.SetMandatoryOptions(DependencyVariablesManager);
				ControlsManager.SetAllControlsToWithinValidationProcedure(false);
			}
			else
			{
				ControlsManager.SetShowStates(DependencyVariablesManager);
			}

			if (AreOptionsValid && _isSelectWorksItem)//remove SelectWorks indication if item is valid
			{
				IsSelectWorksItem = false;
			}
		}

		private void ApplyRuleChanges(bool runRulesAgain = true)
		{
			LogInformation($"Entered {nameof(ApplyRuleChanges)} method.");
			// Apply any selection list changes that are needed
			foreach (var control in UISelect)
			{
				control.ApplySelectOptionsChanges();
			}

			if (_valuesToChange.Count == 0)
			{
				return;
			}

			List<ValueChangeEventArgs> localValuesToChange = new();

			// Get a local copy of the main lists so we can clear the main lists for more rule runs
			if (_valuesToChange.Count > 0)
			{
				localValuesToChange = (from value in _valuesToChange
									   select value)
					.Select(v => new ValueChangeEventArgs
					{
						OptionName = v.OptionName,
						OptionValue = v.OptionValue,
						RunRules = v.RunRules,
						StepId = v.StepId
					}).ToList();
			}

			_valuesToChange.Clear();

			foreach (var value in localValuesToChange)
			{
				// Change the value, turning off the ability to run rules.
				var optionName = value.OptionName;
				var control = ControlsManager.FindControlByName(optionName);
				if (control is not null && !Equals(control.SelectedValue, value.OptionValue))
				{
					// The control is found, but for some reason it's value was not set yet.
					control.RunRulesOnSelectionValueSet = false;
					//control.SelectedValue = value.OptionValue;
					control.SelectedValueInternal = value.OptionValue;
				}
				else
				{
					// Either it is not a control variable, or the control is already set but the validation process didn't update the dependency variables.
					DependencyVariablesManager.AddValueToDependencyVariable(value.OptionName, value.OptionValue, value.StepId);
				}

				RunOnSelectRules(optionName);
				SectioningManager.RunSectionRules(DependencyVariablesManager, ControlsManager, OverallMaxMin, SectionMaxMin, AreOptionsValid, _logger);
			}

			if (!runRulesAgain) return;
			// Run all the rules now that all the changes have been applied
			RunAllRules(string.Empty);

			// Just in case the rules that ran require more changes
			ApplyRuleChanges();
		}

		public void ValidateSavedConfiguration()
		{
			// Do not apply default values (unless required) during this testing phase
			ControlsManager.SetCanApplyDefaultValues(false);
			if (!_isSpecialOverride)
			{
				RunAllRules(string.Empty);
				ApplyRuleChanges();
			}
			if (!ProPricingMode && !IsLight) InternalRunPricing();
			// Reset back to true
			ControlsManager.SetCanApplyDefaultValues(true);
		}

		//private bool changedHeight = false;

		private void ValueChangeEventMethod(ValueChangeEventArgs e)
		{
			LogInformation($"Entered {nameof(ValueChangeEventMethod)} event method for {e.OptionName}");
			var methodStart = DateTime.Now;
			//_changedAccessoryNames.Add(e.OptionName);
			if (e.IsFromCustomPaintCodeValidation)
			{
				// We only call this event so the client can be notified of the change. Internally, this will occur later in the process already.
				return;
			}

			if (ProSelectionMode && !_isValidatingOnCommand)
			{
				// Still run the current selected control's  on select rules
				RunOnSelectRules(e.OptionName);
				// Also add the selected value to the dependency variables
				DependencyVariablesManager.AddValueToDependencyVariable(e.OptionName, e.OptionValue, e.StepId);
				// And make sure to still run the show state validation rules on each accessory, so the options are still limited when needed
				ControlsManager.SetAllControlsToWithinValidationProcedure(true);
				ControlsManager.SetShowStates(DependencyVariablesManager);
				ControlsManager.SetDefaultValues(DependencyVariablesManager);
				ControlsManager.SetMandatoryOptions(DependencyVariablesManager);
				ControlsManager.SetAllControlsToWithinValidationProcedure(false);
				return;
			}

			if (e.IsFromValidationProcedure)
			{
				// Just add to the to-do list for when the validation procedure is done
				if (e.StepId > -1)
				{
					// Check if this value should be set
					if (!DependencyVariablesManager.CanVariableBeUpdated(e.OptionName, e.StepId)) return;
				}
				_valuesToChange.Add(e);
				if (e.ApplyChangesWhenDone) ApplyRuleChanges(false);
				return;
			}

			LogInformation($"Running validation procedure steps within OnValueChange for {e.OptionName}.");

			DependencyVariablesManager.AddValueToDependencyVariable(e.OptionName, e.OptionValue, e.StepId);
			if (e.RunRules)
			{
				LogInformation($"Running all rules within {nameof(ValueChangeEventMethod)} for {e.OptionName} being changed to {e.OptionValue?.ToString() ?? "NULL"}", LogLevel.Warning);
				RunAllRules(e.OptionName);

				ApplyRuleChanges();
			}

			var methodEnd = DateTime.Now;
			LogInformation($"Finished {nameof(ValueChangeEventMethod)} event method for {e.OptionName} :: Took {(methodEnd - methodStart).TotalSeconds} seconds to complete.");
		}

		public void RunValidation()
		{
			try
			{
				_isValidatingOnCommand = true;
				RunAllRules(string.Empty);
				ApplyRuleChanges();
			}
			catch (Exception ex)
			{
				// Do nothing but log it for now. Need to see what kinds of issues we get.
				LogInformation($"Error running the validation process.\n{ex}", LogLevel.Error);
			}
			finally
			{
				_isValidatingOnCommand = false;
			}
		}

		public void RunPricing(bool isLight = false)
		{
			if (!isLight)
			{
				SectioningManager ??= new SectioningManager();
				PricingManager ??= new PricingManager();
				IsLight = isLight;
				if (!IsLight)
				{
					SetupEvents();
				}
				RunValidation();
				InternalRunPricing();
			}
		}
		private void InternalRunPricing()
		{
			LogInformation($"Entering {nameof(InternalRunPricing)}");

			PricingManager.DependencyVariablesManager = DependencyVariablesManager;
			if (IsSelectWorksItem)
			{
				GeneralMessages.Add("Uncheck Special to price item");
				PricingManager.ClearPricingVariables(ModelMultiplier);
			}
			else if (IsSpecialOverride && !IsSpecialModel)
			{
				PricingManager.ClearPricingVariables(ModelMultiplier);
				InvalidOptions.Clear();
				if (!PricingManager.CalculatePricingForSpecialOverride(ControlsManager))
				{
					LogInformation($"{nameof(InternalRunPricing)} - Base Price is invalid!");

					var basePriceControl = ControlsManager.FindControlByName(ControlsManager.SpecialBasePriceControl);
					if (basePriceControl != null)
					{
						InvalidOptions.Add(new InvalidOption() { ControlName = basePriceControl.ValueName, ControlUIDescription = basePriceControl.UIDescription, Message = "Base price must be entered" });
						LogInformation($"{nameof(InternalRunPricing)} - added 'base price' in InvalidOptions collection");
					}

					_setValidStateFromOptions = false;
				}
				else
				{
					LogInformation($"{nameof(InternalRunPricing)} - Base Price is valid");
					_setValidStateFromOptions = true;
				}
			}
			else if (AreOptionsValid)//includes regular models and special model (special model has IsSpecialOverride = true and IsSpecialModel = true)
			{
				LogInformation("Running pricing procedure.");

				//todo: delete time tracking
				Stopwatch entirePricing = new();
				entirePricing.Restart();

				PricingManager.CalculatePricing(ControlsManager, OverallMaxMin, ModelMultiplier, _logger, _logInformation);
				if (_valuesToChange?.Count > 0)
				{
					ApplyRuleChanges(false);
				}

				foreach (var uiOption in UISelect.Where(o => o.ShowControl))
				{
					if (AreOptionsValid) _setValidStateFromOptions = uiOption.ValidationErrors.Count <= 0;
					if (uiOption.HasDependentControls && AreOptionsValid)
					{
						foreach (var dependentControl in uiOption.DependentControls.Where(d => d.ShowControl))
						{
							if (AreOptionsValid) _setValidStateFromOptions = dependentControl.ValidationErrors.Count <= 0;
							SetErrorState(dependentControl);
						}
					}
					SetErrorState(uiOption);
				}
				var printSqFt = DependencyVariablesManager.GetValueFromDependencyVariable("printSqFt");
				if (printSqFt is bool value && value)
				{
					var newNames = new List<string>() { "squareFootage", "squareMillimeter" };
					DependencyVariablesManager.AddDependentVariableNames(newNames);
					DependencyVariablesManager.AddValueToDependencyVariable("squareFootage", SquareFootage, 0);
					DependencyVariablesManager.AddValueToDependencyVariable("squareMillimeter", SquareMillimeter, 0);
				}
				entirePricing.Stop();
				LogInformation($"Timing - Entire pricing time = {entirePricing.Elapsed.Milliseconds}");

				if (!AreOptionsValid) PricingManager.ClearPricingVariables(ModelMultiplier);
			}
			else
			{
				PricingManager.ClearPricingVariables(ModelMultiplier);
			}
		}





		public void AddTestValidationControl(UIOption validationTestingOption, string controlToReplace = null)
		{
			ControlsManager.AddControl(validationTestingOption);
			if (!string.IsNullOrWhiteSpace(controlToReplace))
			{
				ControlsManager.RemoveControl(controlToReplace);
			}
		}

		public string TestValidationTime(string validationOptionName, Dictionary<string, object> variables)
		{
			var validationTestingOption = ControlsManager.FindControlByName(validationOptionName);
			if (validationTestingOption is null) return $"Unable to locate validation test option named {validationOptionName ?? "null"}.";

			var dynamicVariables = variables.ConvertToDynamic();
			var timeString = new StringBuilder();
			timeString.AppendLine("Running Validation Time Test");
			timeString.AppendLine("");
			//timeString.AppendLine($"First time running validation against {validationTestingOption.ValidationJsonRules.Count} json rules.");
			timeString.AppendLine($"Second time running validation against {validationTestingOption.ValidationMatrices.Count} json matrices.");

			var testWatch = new Stopwatch();
			validationTestingOption.RunValidationMatrices = true;
			testWatch.Start();
			validationTestingOption.RunValidation(dynamicVariables);
			testWatch.Stop();
			//timeString.AppendLine($"Running the json rules took {testWatch.ElapsedMilliseconds} milliseconds.");
			timeString.AppendLine($"Running the matrices took {testWatch.ElapsedMilliseconds} milliseconds.");
			if (validationTestingOption.ValidationErrors?.Count > 0)
			{
				timeString.AppendLine($"Validation result is an invalid option: Validation errors count of {validationTestingOption.ValidationErrors.Count}.");
				timeString.AppendLine("                                         Validation errors are");
				timeString.AppendLine(String.Join(',', validationTestingOption.ValidationErrors.Select(m => m.Message)));
			}
			else
			{
				timeString.AppendLine("Validation results in a valid option.");
			}

			timeString.AppendLine("");
			//timeString.AppendLine($"Second time running validation against {validationTestingOption.ValidationMatrices.Count} json matrices.");
			timeString.AppendLine($"First time running validation against {validationTestingOption.ValidationJsonRules.Count} json rules.");

			validationTestingOption.RunValidationMatrices = false;
			testWatch.Restart();
			validationTestingOption.RunValidation(dynamicVariables);
			testWatch.Stop();
			//timeString.AppendLine($"Running the matrices took {testWatch.ElapsedMilliseconds} milliseconds.");
			timeString.AppendLine($"Running the json rules took {testWatch.ElapsedMilliseconds} milliseconds.");
			if (validationTestingOption.ValidationErrors?.Count > 0)
			{
				timeString.AppendLine($"Validation result is an invalid option: Validation errors count of {validationTestingOption.ValidationErrors.Count}.");
				timeString.AppendLine("                                         Validation errors are");
				timeString.AppendLine(String.Join(',', validationTestingOption.ValidationErrors.Select(m => m.Message)));
			}
			else
			{
				timeString.AppendLine("Validation results in a valid option.");
			}

			return timeString.ToString();
		}

		private void SetValuesFromOtherEngine(Dictionary<string, object> otherValues)
		{
			var userOptions = new List<string>();
			foreach (var control in ControlsManager.FullControlList)
			{
				if (!otherValues.ContainsKey(control)) continue;

				// Update the control to the selected value
				userOptions.Add(control);
				ControlsManager.ResetUserSetStatus(userOptions, control);
				var uiControl = ControlsManager.FindControlByName(control);
				uiControl.SelectedValueInternal = otherValues[control];
				uiControl.WasUserSet = true;

				// Remove it from the list since it has been set now
				otherValues.Remove(control);
			}

			if (otherValues.Count == 0) return;

			// Any remaining variables will just be added to the list
			foreach (var perfVariable in otherValues)
			{
				if (perfVariable.Key.Equals("som", StringComparison.OrdinalIgnoreCase))
				{
					SystemOfMeasure = perfVariable.Value.ToString();
					continue;
				}
				if (perfVariable.Key.Equals("Metric Units", StringComparison.OrdinalIgnoreCase))
				{
					SystemOfMeasure = (bool)perfVariable.Value ? "metric" : "imperial";
					continue;
				}
				DependencyVariablesManager.AddValueToDependencyVariable(perfVariable.Key, perfVariable.Value, 0);
			}
		}

		private void SetTokenValue(NeedTokenEventArgs eventArgs)
		{
			var control = ControlsManager.FindControlByName(_needTokenControl);
			_needTokenControl = "";
			if (control is null) return;
			control.BearerToken = eventArgs.BearerToken;
			control.ClientsApiBaseAddressUrl = eventArgs.ClientsApiBaseAddressUrl;
		}

		private void UpdateOptionStrings()
		{
			var (uiOptionsSelectedValuesAsString, uiOptionsSelectedValuesWithStepIdAsString) = _optionsStringGenerator.GetConcatenatedUiOptionsSelectedValuesAsString(this);
			var updatedAtLeastOneString = false;
			// Removed the check for null or empty string since the updated values could be due to no options available
			if (!string.Equals(UiOptionsSelectedValuesAsString, uiOptionsSelectedValuesAsString))
			{
				UiOptionsSelectedValuesAsString = uiOptionsSelectedValuesAsString;
				updatedAtLeastOneString = true;
			}

			if (!string.Equals(UiOptionsSelectedValuesWithStepIdAsString, uiOptionsSelectedValuesWithStepIdAsString))
			{
				UiOptionsSelectedValuesWithStepIdAsString = uiOptionsSelectedValuesWithStepIdAsString;
				updatedAtLeastOneString = true;
			}

			if (updatedAtLeastOneString) OptionsStringsChanged();
		}

		internal string GetDimensionsText()
		{
			//possible combinations for dimensions:
			//null, null
			//Diameter, null
			//Length, null
			//Length, Slots 
			//Length, Width
			//Width, null
			//Width, Height 

			var width = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Width.DisplayName);
			var height = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Height.DisplayName);
			var length = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Length.DisplayName);
			var slots = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Slots.DisplayName);
			var diameter = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Diameter.DisplayName);

			if (diameter is not null)
				return SystemOfMeasure.ToDimensionsText(diameter, null);

			if (slots is not null)
				return SystemOfMeasure.ToDimensionsText(length, slots, true);

			if (length is not null)
				return SystemOfMeasure.ToDimensionsText(length, width);

			return SystemOfMeasure.ToDimensionsText(width, height);
		}

		internal void CleanDimensions()
		{
			// For now, just remove the diameter dimension if it is not needed, usually due to swapping a transition accessory between round and oval/square/rect
			var diameter = DependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.Diameter.DisplayName);
			// If no diameter value, just return out //removed 42224 bc engine would removed only diameter first then never remove any other size related to diameter even if it were set.
			//if (diameter is null) return;

			var transition = DependencyVariablesManager.GetValueFromDependencyVariable("Transitn");
			// If there is not transition accessory, just return out since the diameter is only optional when it is due to a transition value
			if (transition is null) return;

			// If the transition is set to "CR" which is for round, then return since we need to keep diameter
			if (transition.ToString().Equals("CR", StringComparison.OrdinalIgnoreCase)) return;

			// Otherwise assume it is a two dimensional model, so remove the diameter dimension from the variables
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.Diameter.DisplayName);
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.MaxDiameter.DisplayName);
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.MinDiameter.DisplayName);
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.MaxSectionSizeDiamter.DisplayName);
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.MinSectionSizeDiamter.DisplayName);
			DependencyVariablesManager.DependencyVariables.Remove(SizingEnumeration.SectionSizeDiameter.DisplayName);
		}

		internal void LogInformation(string message, LogLevel logLevel = LogLevel.Information)
		{
			if (_logInformation)
			{
				_logger?.Log(logLevel, message);
			}
		}

		private void PricingVariablesChanged(PricingChangedEventArgs e) =>
			PricingVariablesChangedEvent?.Invoke(this, e);

		private void ControlVisibilityStateChanged(ControlVisibilityStateChangedArgs e) =>
			ControlVisibilityChangedEvent?.Invoke(this, e);

		private void OptionsStringsChanged() => OptionsStringsChangedEvent?.Invoke(this, EventArgs.Empty);

		//private void NeedToken(NeedTokenEventArgs e) => NeedTokenEvent?.Invoke(this, e);

		private void NeedCustomPaintCodes() => NeedCustomPaintCodesEvent?.Invoke(this, EventArgs.Empty);

		private void SaveCustomPaintCodes(SaveCustomPaintCodesEventArgs e) => SaveCustomPaintCodesEvent?.Invoke(this, e);

		public event EventHandler<PricingChangedEventArgs> PricingVariablesChangedEvent;
		public event EventHandler<ControlVisibilityStateChangedArgs> ControlVisibilityChangedEvent;
		public event EventHandler OptionsStringsChangedEvent;
		//public event EventHandler<NeedTokenEventArgs> NeedTokenEvent;
		public event EventHandler NeedCustomPaintCodesEvent;
		public event EventHandler<SaveCustomPaintCodesEventArgs> SaveCustomPaintCodesEvent;

		private void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
				ControlsManager?.Dispose();
				CleanupEvents();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_isDisposed = true;
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~UnitSelectionAndPricingEngine()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}