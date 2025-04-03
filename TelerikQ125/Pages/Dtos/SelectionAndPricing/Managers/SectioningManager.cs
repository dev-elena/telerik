using Microsoft.Extensions.Logging;

using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions;

using System;
using System.Linq;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing
{
    public sealed class SectioningManager
    {
        //private UnitSelectionAndPricingEngine _unitSelectionAndPricingEngine;
        private DependencyVariablesManager _dependencyVariablesManager;
        private ControlsManager _controlsManager;
        private bool isChangeUnitSize = false;
        internal void RunSectionRules(DependencyVariablesManager dependencyVariablesManager,
            ControlsManager controlsManager, SizingLimits overallMaxMin, SizingLimits sectionMaxMin, bool isUnitValid = true,
            ILogger logger = null)
        {
            _dependencyVariablesManager = dependencyVariablesManager;
            _controlsManager = controlsManager;
            if (overallMaxMin is null || sectionMaxMin is null) return;

            string[] fsdWithChangeunitSize = { "1213", "1213M", "1213SS", "1213VB", "1223", "1223-3", "1223M", "1223M-3", "1223SS", "1223SS-3", "1223VB", "1263", "1273", "1283", "1203", "1203-3", "1203SS", "1203SS-3", "D1203", "D1203-3", "D1203SS", "D1203SS-3" };
            if (_dependencyVariablesManager.DependencyVariables.TryGetValue("model", out var model))
            {
                if (fsdWithChangeunitSize.Contains(model))
                {
                    isChangeUnitSize = true;
                }
            }

            switch (overallMaxMin.MultiSectionCalculationRuleCode?.ToUpper())
            {
                case "1A":
                    // Only section based on single dimention
                    RunSectionRule1A();
                    break;
                case "2A":
                    // Section based on dimension 1 then dimension 2
                    RunSectionRule2A();
                    break;
                case "3A":
                    RunSectionRule3A(logger);
                    break;
                case "2B":
                case "2C":

                case "3B":
                    // Need to get more information, but for now just use 2A
                    RunSectionRule2A();
                    break;
            }

            // Set the section text control value
            var sectionControl = _controlsManager.FindControlByName(SizingEnumeration.SectionText.DisplayName);
            if (sectionControl is null || !isUnitValid) return;
            var sectionValue = "";
            var isSingleSeciton = false;
            var totalNumSections = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.NumberOfSections.DisplayName);
            if (totalNumSections is null || (int.TryParse(totalNumSections.ToString(), out var numSections) && numSections <= 1)) 
            {
                isSingleSeciton = true;
                // clear the section text if it is not needed
                if (sectionControl.SelectedValueInternal.NullableEquals("", forceNullMatch: false) != true)
                {
                    sectionControl.SelectedValueInternal = "";
                }
                return; 
            }

            var sectH = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionSizeHigh.DisplayName);
            var sectW = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionSizeWide.DisplayName);

            var sectL = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionSizeLength.DisplayName);
            var sectD = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionSizeDiameter.DisplayName);
            var sectS = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SectionSizeSlots.DisplayName);
            var sectSlvW = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SleeveSectionWidth.DisplayName);
            var sectSlvH = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.SleeveSectionHeight.DisplayName);
            var numSectH = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.NumberSectionsHorizontal.DisplayName);
            var numSectV = _dependencyVariablesManager.GetValueFromDependencyVariable(SizingEnumeration.NumberSectionsVertical.DisplayName);
            if ((sectH is null && sectW is null && sectL is null && sectD is null && sectS is null) || numSectV is null || numSectH is null) return;

            // According to the database, the only combinations of unit size 1 and unit size 2 are
            // Length and Slots
            // Length and Width
            // Width and Height
            sectionControl.RunRulesOnSelectionValueSet = false;
            if (!isSingleSeciton)
            {
                if (sectW is not null && sectH is not null)
                {
                    sectionValue = $"{numSectH.ToUSCultureDouble().ToPrecision()} @ {sectW.ToUSCultureDouble().ToPrecision()} W x {numSectV.ToUSCultureDouble().ToPrecision()} @ {sectH.ToUSCultureDouble().ToPrecision()} H";
                }
                else if (sectL is not null && sectW is not null)
                {
                    sectionValue = $"{numSectH.ToUSCultureDouble().ToPrecision()} @ {sectL.ToUSCultureDouble().ToPrecision()} L x {numSectV.ToUSCultureDouble().ToPrecision()} @ {sectW.ToUSCultureDouble().ToPrecision()} W";
                }
                else if (sectL is not null && sectS is not null)
                {
                    sectionValue = $"{numSectH.ToUSCultureDouble().ToPrecision()} @ {sectL.ToUSCultureDouble().ToPrecision()} L x {numSectV.ToUSCultureDouble().ToPrecision()} @ {sectS.ToUSCultureDouble().ToPrecision()} S";
                }
                else
                {
                    sectionControl.RunRulesOnSelectionValueSet = false;
                }
                if (sectionControl.SelectedValueInternal.NullableEquals(sectionValue, forceNullMatch: false) != true)
                {
                    sectionControl.SelectedValueInternal = sectionValue;
                }
            }
        }

        private void RunSectionRule1A()
        {
            // Always one section high
            _dependencyVariablesManager.AddValueToDependencyVariable(SizingEnumeration.SectionSizeHigh.DisplayName, 1, 0);

            // Calculate the section width size
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Width.DisplayName, out var width);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Height.DisplayName, out var height);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Length.DisplayName, out var length);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Diameter.DisplayName, out var diameter);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Slots.DisplayName, out var slots);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWide.DisplayName, out var maxSectWidth);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHigh.DisplayName, out var maxSectHeight);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeLength.DisplayName, out var maxSectLength);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeDiamter.DisplayName, out var maxSectDiameter);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeSlots.DisplayName, out var maxSectSlots);

            if ((width is null && height is null && length is null && diameter is null && slots is null) ||
                (maxSectWidth is null && maxSectHeight is null && maxSectLength is null && maxSectDiameter is null && maxSectSlots is null)) return; // Has not been set yet, so cannot perform this operation

            int sectCount = 0;
            object sizeVar = null;
            SizingEnumeration sizing = null;
            if (width is not null && maxSectWidth is not null)
            {
                sizeVar = width;
                sizing = SizingEnumeration.SectionSizeWide;
                sectCount = Convert.ToInt32(Math.Ceiling(width.ToUSCultureDouble() / maxSectWidth.ToUSCultureDouble()));
            }
            else if (height is not null && maxSectHeight is not null)
            {
                sizeVar = height;
                sizing = SizingEnumeration.SectionSizeHigh;
                sectCount = Convert.ToInt32(Math.Ceiling(height.ToUSCultureDouble() / maxSectHeight.ToUSCultureDouble()));
            }
            else if (length is not null && maxSectLength is not null)
            {
                sizeVar = length;
                sizing = SizingEnumeration.SectionSizeLength;
                sectCount = Convert.ToInt32(Math.Ceiling(length.ToUSCultureDouble() / maxSectLength.ToUSCultureDouble()));
            }
            else if (diameter is not null && maxSectDiameter is not null)
            {
                sizeVar = diameter;
                sizing = SizingEnumeration.SectionSizeDiameter;
                sectCount = Convert.ToInt32(Math.Ceiling(diameter.ToUSCultureDouble() / maxSectDiameter.ToUSCultureDouble()));
            }
            else if (slots is not null && maxSectSlots is not null)
            {
                sizeVar = slots;
                sizing = SizingEnumeration.SectionSizeSlots;
                sectCount = Convert.ToInt32(Math.Ceiling(slots.ToUSCultureDouble() / maxSectSlots.ToUSCultureDouble()));
            }

            if (sizeVar is null || sectCount == 0 || sizing is null) return;

            _dependencyVariablesManager.AddValueToDependencyVariable(sizing.DisplayName, sizeVar.ToUSCultureDouble() / sectCount, 0);
            _dependencyVariablesManager.AddValueToDependencyVariable(SizingEnumeration.NumberOfSections.DisplayName, sectCount, 0);
        }

        private void RunSectionRule2A(bool? useARules = null)
        {
            // Calculate the section width size first, then the section height size
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Width.DisplayName, out var width);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Height.DisplayName, out var height);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Length.DisplayName, out var length);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Diameter.DisplayName, out var diameter);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Slots.DisplayName, out var slots);
            _dependencyVariablesManager.DependencyVariables.TryGetValue("overallSizeReduce", out var overallSizeReduce);


            object maxSectWidth;
            object maxSectHeight;
            if (useARules != null)
            {
                if ((bool)useARules)
                {
                    _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWideA.DisplayName, out var maxSectWidthTemp);
                    _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHighA.DisplayName, out var maxSectHeightTemp);
                    maxSectWidth = maxSectWidthTemp;
                    maxSectHeight = maxSectHeightTemp;
                }
                else
                {
                    _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWideB.DisplayName, out var maxSectWidthTemp);
                    _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHighB.DisplayName, out var maxSectHeightTemp);
                    maxSectWidth = maxSectWidthTemp;
                    maxSectHeight = maxSectHeightTemp;
                }
            }
            else
            {
                _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWide.DisplayName, out var maxSectWidthTemp);
                _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHigh.DisplayName, out var maxSectHeightTemp);
                maxSectWidth = maxSectWidthTemp;
                maxSectHeight = maxSectHeightTemp;
            }
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeLength.DisplayName, out var maxSectLength);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeDiamter.DisplayName, out var maxSectDiameter);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeSlots.DisplayName, out var maxSectSlots);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSleeveSectionWidth.DisplayName, out var maxSectSlvW);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSleeveSectionHeight.DisplayName, out var maxSectSlvH);


            if ((width is null && height is null && length is null && diameter is null && slots is null) ||
                (maxSectWidth is null && maxSectHeight is null && maxSectLength is null && maxSectDiameter is null && maxSectSlots is null)) return;
            if (_dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.NumberOfSections.DisplayName, out var numSects))
            {
                double sizeAddition = 0.0;
                if (isChangeUnitSize && Convert.ToInt32(numSects) > 1) //only add 2" to nomial size if this is a change size model and it's also mutlisection.
                {
                    if (_dependencyVariablesManager.DependencyVariables.TryGetValue("som", out var som))
                    {
                        if (som.ToString().ToLower() == "metric")
                        {
                            sizeAddition = 50.8; //additional 2 inches for change size in metric
                        }
                        else { sizeAddition = 2; }
                    }
                    if (maxSectWidth is not null && maxSectHeight is not null && width is not null && height is not null)
                    {
                        width = Convert.ToInt32(width) + sizeAddition;
                        height = Convert.ToInt32(height) + sizeAddition;
                        maxSectWidth = Convert.ToInt32(maxSectWidth) + sizeAddition; //use type A single section max/min sizes. per price page 1210-1and 1201-4, which is +2"
                        maxSectHeight = Convert.ToInt32(maxSectHeight) + sizeAddition; //use type A single section max/min sizes. per price page 1210-1and 1201-4, which is +2"
                    }
                    else if (maxSectDiameter is not null && diameter is not null && width is not null && height is not null && maxSectHeight is not null)
                    {
                        diameter = Convert.ToInt32(diameter) + sizeAddition;
                        maxSectDiameter = Convert.ToInt32(maxSectDiameter) + sizeAddition;
                        maxSectHeight = Convert.ToInt32(maxSectHeight) + sizeAddition;
                        width = Convert.ToInt32(width) + sizeAddition;
                        height = Convert.ToInt32(height) + sizeAddition;
                    }
                }
            }

            // According to the database, the only combinations of unit size 1 and unit size 2 are
            // Length and Slots
            // Length and Width
            // Width and Height

            int sectCount1 = 0;
            int sectCount2 = 0;
            object sizeVar1 = null;
            object sizeVar2 = null;
            SizingEnumeration sizing1 = null;
            SizingEnumeration sizing2 = null;

            double reduceOverallSizeValue = 0;
            if (overallSizeReduce != null && Double.TryParse(overallSizeReduce.ToString(), out _))
            {
                reduceOverallSizeValue = overallSizeReduce.ToUSCultureDouble();
            }

            if (width is not null && maxSectSlvW is not null && height is not null && maxSectSlvH is not null)
            {
                sizeVar1 = width;
                sizeVar2 = height;
                sizing1 = SizingEnumeration.SleeveSectionWidth;
                sizing2 = SizingEnumeration.SleeveSectionHeight;
                //Brad said we don't need to apply reduceOverallSizeValue (AD_SizeChangeType = 'IS') to sleeves, and also I didn't find it in Xin rapp code
                sectCount1 = Convert.ToInt32(Math.Ceiling(width.ToUSCultureDouble() / maxSectSlvW.ToUSCultureDouble()));
                sectCount2 = Convert.ToInt32(Math.Ceiling(height.ToUSCultureDouble() / maxSectSlvH.ToUSCultureDouble()));
                _dependencyVariablesManager.AddValueToDependencyVariable(sizing1.DisplayName, sizeVar1.ToUSCultureDouble() / sectCount1, 0);
                _dependencyVariablesManager.AddValueToDependencyVariable(sizing2.DisplayName, sizeVar2.ToUSCultureDouble() / sectCount2, 0);
            }

            if (width is not null && maxSectWidth is not null && height is not null && maxSectHeight is not null)
            {
                sizeVar1 = width;
                sizeVar2 = height;
                sizing1 = SizingEnumeration.SectionSizeWide;
                sizing2 = SizingEnumeration.SectionSizeHigh;
                sectCount1 = Convert.ToInt32(Math.Ceiling((width.ToUSCultureDouble() - reduceOverallSizeValue) / maxSectWidth.ToUSCultureDouble()));
                sectCount2 = Convert.ToInt32(Math.Ceiling((height.ToUSCultureDouble() - reduceOverallSizeValue) / maxSectHeight.ToUSCultureDouble()));
            }
            else if (diameter is not null && maxSectDiameter is not null && maxSectHeight is not null && height is not null)
            {
                // Assume transition, so use diameter limits as width and height
                sizeVar1 = diameter;
                sizeVar2 = height;
                sizing1 = SizingEnumeration.SectionSizeWide;
                sizing2 = SizingEnumeration.SectionSizeHigh;
                sectCount1 = Convert.ToInt32(Math.Ceiling(diameter.ToUSCultureDouble() / maxSectDiameter.ToUSCultureDouble()));
                sectCount2 = Convert.ToInt32(Math.Ceiling(diameter.ToUSCultureDouble() / maxSectHeight.ToUSCultureDouble()));
            }
            else if (length is not null && maxSectLength is not null && width is not null && maxSectWidth is not null)
            {
                sizeVar1 = length;
                sizeVar2 = width;
                sizing1 = SizingEnumeration.SectionSizeLength;
                sizing2 = SizingEnumeration.SectionSizeWide;
                sectCount1 = Convert.ToInt32(Math.Ceiling(length.ToUSCultureDouble() / maxSectLength.ToUSCultureDouble()));
                sectCount2 = Convert.ToInt32(Math.Ceiling(width.ToUSCultureDouble() / maxSectWidth.ToUSCultureDouble()));
            }
            else if (length is not null && maxSectLength is not null && slots is not null && maxSectSlots is not null)
            {
                sizeVar1 = length;
                sizeVar2 = slots;
                sizing1 = SizingEnumeration.SectionSizeLength;
                sizing2 = SizingEnumeration.SectionSizeSlots;
                sectCount1 = Convert.ToInt32(Math.Ceiling(length.ToUSCultureDouble() / maxSectLength.ToUSCultureDouble()));
                sectCount2 = Convert.ToInt32(Math.Ceiling(slots.ToUSCultureDouble() / maxSectSlots.ToUSCultureDouble()));
            }

            if (sizeVar1 is null || sectCount1 == 0 || sizing1 is null || sizeVar2 is null || sectCount2 == 0 || sizing2 is null) return;

            _dependencyVariablesManager.AddValueToDependencyVariable(sizing1.DisplayName, sizeVar1.ToUSCultureDouble() / sectCount1, 0);
            _dependencyVariablesManager.AddValueToDependencyVariable(sizing2.DisplayName, sizeVar2.ToUSCultureDouble() / sectCount2, 0);

            // Then set the number of sections in total
            _dependencyVariablesManager.AddValueToDependencyVariable(SizingEnumeration.NumberOfSections.DisplayName, sectCount1 * sectCount2, 0);
            _dependencyVariablesManager.AddValueToDependencyVariable(SizingEnumeration.NumberSectionsHorizontal.DisplayName, sectCount1, 0);
            _dependencyVariablesManager.AddValueToDependencyVariable(SizingEnumeration.NumberSectionsVertical.DisplayName, sectCount2, 0);
        }
        private void RunSectionRule3A(ILogger logger = null)
        {
            //According to Xin, this is a 3 step section calculation. You have a possibiliy of 2 different max sect sizes where maxSectW and maxSectH
            //can be swapped. You must check 3 conditions in order. 1) Smallest number of sections 2) More square 3) Taller then Wider. Use the section
            //sizes that are the first to validate a condition

            // Calculate the section width size first, then the section height size
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Width.DisplayName, out var width);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.Height.DisplayName, out var height);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWideA.DisplayName, out var maxSectWidthA);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHighA.DisplayName, out var maxSectHeightA);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeWideB.DisplayName, out var maxSectWidthB);
            _dependencyVariablesManager.DependencyVariables.TryGetValue(SizingEnumeration.MaxSectionSizeHighB.DisplayName, out var maxSectHeightB);

            bool useARules;

            if (maxSectWidthA is null || maxSectHeightA is null || maxSectWidthB is null || maxSectHeightB is null) RunSectionRule2A();
            else
            {
                int sectCount1A;
                int sectCount2A;
                int sectCount1B;
                int sectCount2B;

                if (width is not null && height is not null && Double.TryParse(width.ToString(), out _) && Double.TryParse(height.ToString(), out _)
                    && Double.TryParse(maxSectWidthA.ToString(), out _) && Double.TryParse(maxSectHeightA.ToString(), out _)
                    && Double.TryParse(maxSectWidthB.ToString(), out _) && Double.TryParse(maxSectHeightB.ToString(), out _))
                {
                    logger?.LogInformation($"RunSectionRule3A: width: {width},height: {height},maxSectWidthA: {maxSectWidthA},maxSectHeightA: {maxSectHeightA},maxSectWidthB: {maxSectWidthB},maxSectWidthB: {maxSectWidthB},maxSectHeightB: {maxSectHeightB} ", LogLevel.Warning);
                    sectCount1A = Convert.ToInt32(Math.Ceiling(width.ToUSCultureDouble() / maxSectWidthA.ToUSCultureDouble()));
                    sectCount2A = Convert.ToInt32(Math.Ceiling(height.ToUSCultureDouble() / maxSectHeightA.ToUSCultureDouble()));
                    sectCount1B = Convert.ToInt32(Math.Ceiling(width.ToUSCultureDouble() / maxSectWidthB.ToUSCultureDouble()));
                    sectCount2B = Convert.ToInt32(Math.Ceiling(height.ToUSCultureDouble() / maxSectHeightB.ToUSCultureDouble()));
                    //Check 1st rule - which has less sections
                    int totalSectA = sectCount1A * sectCount2A;
                    int totalSectB = sectCount1B * sectCount2B;
                    if (totalSectA < totalSectB) // A has less sections
                    {
                        useARules = true;
                        RunSectionRule2A(useARules);
                    }
                    else if (totalSectB < totalSectA) //B has less sections
                    {
                        useARules = false;
                        RunSectionRule2A(useARules);
                    }
                    else if (totalSectB == totalSectA)// A andB have same number of sections, move on to the next rule
                    {
                        // Check 2nd rule - which section is more square than the other
                        //calc sections if using A measurments
                        double tempSectWA;
                        if (sectCount1A > 1)
                        {
                            tempSectWA = Math.Round(width.ToUSCultureDouble() / sectCount1A, 2);
                        }
                        else tempSectWA = width.ToUSCultureDouble();
                        logger?.LogInformation($"RunSectionRule3A: tempSectWA: {tempSectWA}", LogLevel.Warning);

                        double tempSectWB;
                        if (sectCount1B > 1)
                        {
                            tempSectWB = Math.Round(width.ToUSCultureDouble() / sectCount1B, 2);
                        }
                        else tempSectWB = width.ToUSCultureDouble();
                        logger?.LogInformation($"RunSectionRule3A: tempSectWB: {tempSectWB}", LogLevel.Warning);

                        double tempSectHA;
                        if (sectCount2A > 1)
                        {
                            tempSectHA = Math.Round(height.ToUSCultureDouble() / sectCount2A, 2);
                        }
                        else tempSectHA = height.ToUSCultureDouble();
                        logger?.LogInformation($"RunSectionRule3A: tempSectHA: {tempSectHA}", LogLevel.Warning);

                        double tempSectHB;
                        if (sectCount2B > 1)
                        {
                            tempSectHB = Math.Round(height.ToUSCultureDouble() / sectCount2B, 2);
                        }
                        else tempSectHB = height.ToUSCultureDouble();
                        logger?.LogInformation($"RunSectionRule3A: tempSectHB: {tempSectHB}", LogLevel.Warning);

                        double diffA = Math.Abs(tempSectWA - tempSectHA);
                        double diffB = Math.Abs(tempSectWB - tempSectHB);
                        //double diffA = Math.Abs((double)maxSectWidthA - (double)maxSectHeightA); //max 72 x 120
                        //double diffB = Math.Abs((double)maxSectWidthB - (double)maxSectHeightB); //mac 120 x 72
                        logger?.LogInformation($"RunSectionRule3A: diffA: {diffA}, diffB: {diffB}", LogLevel.Warning);

                        if (diffA < diffB) //A is more square
                        {
                            useARules = true;
                            RunSectionRule2A(useARules);
                        }
                        else if (diffA > diffB) //B is more square
                        {
                            useARules = false;
                            RunSectionRule2A(useARules);
                        }
                        else if (diffA == diffB) //both have the same "squareness"
                        {
                            //Check last rule - which section is taller than wider
                            if (maxSectHeightA.ToUSCultureDouble() > maxSectWidthA.ToUSCultureDouble()) //A is taller than wider
                            {
                                useARules = true;
                                RunSectionRule2A(useARules);
                            }
                            else if (maxSectHeightB.ToUSCultureDouble() > maxSectWidthB.ToUSCultureDouble()) //B is taller than wider
                            {
                                useARules = false;
                                RunSectionRule2A(useARules);
                            }
                            else
                            {
                                //no rules satisfied requirement,return some sort of error or maybe run regular 2A rules??
                                RunSectionRule2A();
                            }

                        }

                    }
                }
                else
                {
                    return; //width and height are not set yet
                }
            }




        }
    }
}