using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;
using telerik_Q1_25.Pages.Dtos;
using TelerikQ125.Pages.Dtos;

namespace Selection_Pricing_Tester.SelectionAndPricing.Engine.Extensions
{

    public class SelectWorksEngineStateParser 
    {
        protected readonly string _uiOptionsSeparator;
        protected readonly string _stepIdAndValueNameSeparator;
        protected readonly string _uiOptionAndSelectedValueSeparator;
        protected readonly string _dependentControlsSeparator;

        public SelectWorksEngineStateParser(string uiOptionsSeparator = " - "
            , string stepIdAndValueNameSeparator = "."
            , string uiOptionAndSelectedValueSeparator = ":"
            , string dependentControlsSeparator = ";")
        {
            _uiOptionsSeparator = uiOptionsSeparator;
            _stepIdAndValueNameSeparator = stepIdAndValueNameSeparator;
            _uiOptionAndSelectedValueSeparator = uiOptionAndSelectedValueSeparator;
            _dependentControlsSeparator = dependentControlsSeparator;
        }

        //public (string UiOptionsSelectedValuesAsString, string UiOptionsSelectedValuesWithStepIdAsString) GetConcatenatedUiOptionsSelectedValuesAsString(SelectWorksEngine pricingEngine)
        //{
            
        //    if (pricingEngine?.UISelect is null)
        //        return (string.Empty, string.Empty);

        //    pricingEngine.LogInformation($"Entered {nameof(GetConcatenatedUiOptionsSelectedValuesAsString)} method.");

        //    var uiOptionsSelectedValuesStringBuilder = new StringBuilder();
        //    var uiOptionsSelectedValuesWithStepIdStringBuilder = new StringBuilder();

        //    //commented off because this logic was moved to ToExtraAccessoryDetails extension - delete later
        //    //var uiOptionsSelectedValuesDetailsStringBuilder = new StringBuilder();

        //    foreach (var uiOption in pricingEngine.UISelect)
        //    {
        //        pricingEngine.LogInformation($"Getting value for control {uiOption.UIDescription}");
        //        // For controls that do not have a visible UI, there should be no value to add to the option string, so make sure the default value of "-1-" does not get added
        //        if (uiOption.UIControlType != UiControlTypeEnumeration.Group.DisplayName &&
        //            uiOption.UIControlType != UiControlTypeEnumeration.DepGroup.DisplayName &&
        //            uiOption.UIControlType != UiControlTypeEnumeration.NotApplicable.DisplayName &&
        //            uiOption.UIControlType != UiControlTypeEnumeration.HigharchicalDisplay.DisplayName &&
        //            uiOption.UIControlType != UiControlTypeEnumeration.GridFormat.DisplayName &&
        //            uiOption.UIControlType != UiControlTypeEnumeration.Button.DisplayName)
        //        {
        //            if (uiOption.PrintSelectedOptionCode())
        //            {
        //                pricingEngine.LogInformation($"Adding {uiOption.SelectedValue} to the main string.");
        //                var stringValue = uiOption.SelectedValue?.ToString() ?? string.Empty;
        //                if (!uiOption.ShowOptionCodes)
        //                {
        //                    if (uiOption.UIControlType == UiControlTypeEnumeration.CustomVariant.DisplayName)
        //                    {
        //                        // Add the custom accessory value to the option string
        //                        stringValue = uiOption.CustomVarValue?.ToString() ?? uiOption.SelectedValue?.ToString() ?? string.Empty;
        //                    }
        //                    else
        //                    {
        //                        stringValue = uiOption.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == uiOption.SelectedValue?.ToString())?.OptionDescription;
        //                    }
        //                }
        //                if (!string.IsNullOrWhiteSpace(stringValue))
        //                    uiOptionsSelectedValuesStringBuilder.Append(stringValue).Append(_uiOptionsSeparator);
        //            }
        //        }

        //        //commented off because this logic was moved to ToExtraAccessoryDetails extension - delete later
        //        //if (uiOption.PrintSelectedOptionDescription())
        //        //{
        //        //    pricingEngine.LogInformation($"Adding {uiOption.SelectedValue} to the detailed string.");
        //        //    uiOptionsSelectedValuesDetailsStringBuilder.Append('\n');
        //        //    uiOptionsSelectedValuesDetailsStringBuilder.Append(uiOption.UISelectOptionsFullList.First(o => o.OptionValue == uiOption.SelectedValue.ToString())?.OptionDescription);

        //        //    if (uiOption.HasDependentControls)
        //        //    {
        //        //        foreach (var uiOptionDependentControl in uiOption.DependentControls)
        //        //        {
        //        //            pricingEngine.LogInformation($"Adding {uiOptionDependentControl.SelectedValue} to the detailed string.");
        //        //            if (uiOptionDependentControl.SelectedValue is null) continue;
        //        //            uiOptionsSelectedValuesDetailsStringBuilder.Append(", " + uiOptionDependentControl.UIDescription + '=' + uiOptionDependentControl.SelectedValue);
        //        //        }
        //        //    }
        //        //}

        //        pricingEngine.LogInformation($"Calling {nameof(GetDependentControlsSelectedValues)} method to add any dependent control values to the main string.");
        //        GetDependentControlsSelectedValues(ref uiOptionsSelectedValuesStringBuilder, uiOption);
        //        pricingEngine.LogInformation($"Calling {nameof(GetSelectedValuesWithIds)} method to add any dependent control values to the step id string.");
        //        GetSelectedValuesWithIds(ref uiOptionsSelectedValuesWithStepIdStringBuilder, uiOption);
        //    }

        //    var uiOptionsSelectedValuesAsString = uiOptionsSelectedValuesStringBuilder.Length > 2
        //        ? uiOptionsSelectedValuesStringBuilder.ToString()[0..^3]
        //        : string.Empty;

        //    //commented off because this logic was moved to ToExtraAccessoryDetails extension - delete later
        //    //if (!string.IsNullOrWhiteSpace(uiOptionsSelectedValuesAsString) && uiOptionsSelectedValuesDetailsStringBuilder.Length > 2)
        //    //{
        //    //    pricingEngine.LogInformation($"Adding details string to the main string.");
        //    //    uiOptionsSelectedValuesAsString += uiOptionsSelectedValuesDetailsStringBuilder.ToString();
        //    //}

        //    var uiOptionsSelectedValuesWithStepIdAsString = uiOptionsSelectedValuesWithStepIdStringBuilder.Length > 2
        //        ? uiOptionsSelectedValuesWithStepIdStringBuilder.ToString()[0..^3]
        //        : string.Empty;

        //    return (uiOptionsSelectedValuesAsString, uiOptionsSelectedValuesWithStepIdAsString);
        //}

        private void GetDependentControlsSelectedValues(ref StringBuilder stringBuilder, UIOption uiOption)
        {
            foreach (var dependentUiOption in uiOption.DependentControls)
            {
                if (uiOption.UIControlType == UiControlTypeEnumeration.Group.DisplayName ||
                    uiOption.UIControlType == UiControlTypeEnumeration.DepGroup.DisplayName ||
                    uiOption.UIControlType == UiControlTypeEnumeration.NotApplicable.DisplayName ||
                    uiOption.UIControlType == UiControlTypeEnumeration.HigharchicalDisplay.DisplayName ||
                    uiOption.UIControlType == UiControlTypeEnumeration.GridFormat.DisplayName ||
                    uiOption.UIControlType == UiControlTypeEnumeration.Button.DisplayName) continue;

                if (!dependentUiOption.PrintSelectedOptionCode()) continue;

                var stringValue = dependentUiOption.SelectedValue?.ToString() ?? string.Empty;
                if (!dependentUiOption.ShowOptionCodes)
                    stringValue = dependentUiOption.UISelectOptionsFullList.FirstOrDefault(o => o.OptionValue == dependentUiOption.SelectedValue?.ToString())?.OptionDescription;
                if (!string.IsNullOrWhiteSpace(stringValue))
                    stringBuilder.Append(stringValue).Append(_uiOptionsSeparator);
            }
        }

        private void GetSelectedValuesWithIds(ref StringBuilder stringBuilder, UIOption uiOption)
        {
            var dependentUiOptionsSelectedValuesAsString = string.Concat(uiOption
                .DependentControls
                .Select(d => $"{_dependentControlsSeparator}{d.ValueName}{_uiOptionAndSelectedValueSeparator}{d.SelectedValue?.ToString()}"));
            stringBuilder.Append(uiOption.StepID)
                .Append(_stepIdAndValueNameSeparator)
                .Append(uiOption.ValueName)
                .Append(_uiOptionAndSelectedValueSeparator)
                .Append(uiOption.SelectedValue?.ToString())
                .Append(dependentUiOptionsSelectedValuesAsString)
                .Append(_uiOptionsSeparator);
        }

    //    public List<IApiResultMessageModel> RebuildOptionsState(SelectWorksEngine pricingEngine, string optionsStateString)
    //    {
    //        var apiResultMessages = new List<IApiResultMessageModel>();

    //        if (string.IsNullOrWhiteSpace(optionsStateString))
    //            return apiResultMessages.AddInformationMessage("Please input the desired options state string below and try again.", nameof(RebuildOptionsState));

    //        foreach (var parentOptionAndDependentControlsString in (string[])optionsStateString.Split(_uiOptionsSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    //            apiResultMessages.AddRange(RebuildParentOptionState(pricingEngine, parentOptionAndDependentControlsString));

    //        return apiResultMessages;
    //    }

    //    public List<IApiResultMessageModel> RebuildParentOptionState(SelectWorksEngine pricingEngine, string parentOptionAndDependentControlsString)
    //    {
    //        // 0.Nomi_Size:;Width:25;Height:50 - 1.Frame:CH - 11.Shape:STD;H1:;W1: - 17.FMCtrlDmp:None
    //        var apiResultMessages = new List<IApiResultMessageModel>();
    //        if (string.IsNullOrWhiteSpace(parentOptionAndDependentControlsString))
    //            return apiResultMessages;

    //        var parentOptionAndDependentControlsStringArray = parentOptionAndDependentControlsString.Split(_dependentControlsSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    //        if (parentOptionAndDependentControlsStringArray.Length.Equals(0))
    //            return apiResultMessages;

    //        var parentOptionAndSelectedValueString = parentOptionAndDependentControlsStringArray[0];
    //        var parentOptionAndSelectedValueStringArray = parentOptionAndSelectedValueString.Split(_uiOptionAndSelectedValueSeparator, StringSplitOptions.TrimEntries);
    //        if (!parentOptionAndSelectedValueStringArray.Length.Equals(2))
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {parentOptionAndSelectedValueString}, because it is malformed. Either missing StepId.ValueName and/or SelectedValue.", nameof(RebuildParentOptionState));

    //        var parentOptionString = parentOptionAndSelectedValueStringArray[0];
    //        var parentOptionSelectedValueString = parentOptionAndSelectedValueStringArray[1];
    //        var stepIdAndValueNameStringArray = parentOptionString.Split(_stepIdAndValueNameSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    //        if (!stepIdAndValueNameStringArray.Length.Equals(2))
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {parentOptionString}, because it is malformed. Either missing StepId and/or ValueName.", nameof(RebuildParentOptionState));

    //        if (!int.TryParse(stepIdAndValueNameStringArray[0], out int stepId))
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {parentOptionString}, because StepID is not an integer.", nameof(RebuildParentOptionState));

    //        var parentUiOption = pricingEngine.UISelect.FirstOrDefault(item => item.StepID.Equals(stepId) && string.Equals(item.ValueName, stepIdAndValueNameStringArray[1], StringComparison.OrdinalIgnoreCase));
    //        if (parentUiOption is null)
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {parentOptionString}, because it cannot be found in this set of Options.", nameof(RebuildParentOptionState));

    //        parentUiOption.SelectedValue = BaseEnumeration
    //            .FromDisplayName<UiControlTypeEnumeration>(parentUiOption.UIControlType)
    //            .GetSelectedValueFromStringAsObject(parentOptionSelectedValueString);

    //        foreach (var dependentControlAndSelectedValueString in parentOptionAndDependentControlsStringArray.Skip(1))
    //            apiResultMessages.AddRange(RebuildDependentControlState(parentUiOption, dependentControlAndSelectedValueString));

    //        return apiResultMessages;
    //    }

    //    public List<IApiResultMessageModel> RebuildDependentControlState(UIOption parentUiOption, string dependentControlAndSelectedValueString)
    //    {
    //        var apiResultMessages = new List<IApiResultMessageModel>();
    //        if (string.IsNullOrWhiteSpace(dependentControlAndSelectedValueString))
    //            return apiResultMessages;

    //        var optionAndSelectedValueStringArray = dependentControlAndSelectedValueString.Split(_uiOptionAndSelectedValueSeparator, StringSplitOptions.TrimEntries);
    //        if (!optionAndSelectedValueStringArray.Length.Equals(2))
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {dependentControlAndSelectedValueString}, because it is malformed. Either missing ValueName and/or SelectedValue.", nameof(RebuildDependentControlState));

    //        var valueNameString = optionAndSelectedValueStringArray[0];
    //        var selectedValueString = optionAndSelectedValueStringArray[1];

    //        var dependentControlUiOption = parentUiOption.DependentControls.FirstOrDefault(item => string.Equals(item.ValueName, valueNameString, StringComparison.OrdinalIgnoreCase));
    //        if (dependentControlUiOption is null)
    //            return apiResultMessages.AddWarningMessage($"Cannot process: {dependentControlAndSelectedValueString}, because it cannot be found as a DependentControl of {parentUiOption.StepID}.{parentUiOption.ValueName}.", nameof(RebuildDependentControlState));

    //        dependentControlUiOption.SelectedValue = BaseEnumeration
    //            .FromDisplayName<UiControlTypeEnumeration>(dependentControlUiOption.UIControlType)
    //            .GetSelectedValueFromStringAsObject(selectedValueString);

    //        return apiResultMessages;
    //    }
    //
    }

    public interface ISelectWorksEngineStateParser
    {
        //(string UiOptionsSelectedValuesAsString, string UiOptionsSelectedValuesWithStepIdAsString)
        //    GetConcatenatedUiOptionsSelectedValuesAsString(SelectWorksEngine pricingEngine);

        //List<IApiResultMessageModel> RebuildOptionsState(SelectWorksEngine pricingEngine, string optionsStateString);

        //List<IApiResultMessageModel> RebuildParentOptionState(SelectWorksEngine pricingEngine, string parentOptionAndDependentControlsString);

        List<IApiResultMessageModel> RebuildDependentControlState(UIOption parentUiOption, string dependentControlString);
    }
}
