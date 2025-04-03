using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Enumerations;


namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class JsonLogicExtensions
    {
        /// <summary>
        /// Add Nailor specific custom operators for JsonLogic.
        /// </summary>
        /// <remarks>
        /// <para>Custom operations are:</para>
        /// <para>
        ///     roundup:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Rounds up the double value to the nearest whole number.
        ///         </item>
        ///         <item>
        ///             Returns - Rounded up whole number as type Double.
        ///         </item>
        ///         <item>
        ///             Note - Only the first argument for this operation is rounded up.
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     mlook:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Perform a lookup in a price matrix (2D array) based on provided column and row index values.
        ///         </item>
        ///         <item>
        ///             Returns - Resulting value from the matrix lookup as type nullable int.
        ///         </item>
        ///         <item>
        ///             Note - The first argument is the column index, the second argument is the row index, and the third argument is the price matrix.
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     pwr:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Raise a value to a specific power.
        ///         </item>
        ///         <item>
        ///             Returns - The power multiplied value as type Double.
        ///         </item>
        ///         <item>
        ///             Note - The first argument is the Double value to multiply, and the second argument is the power value to apply.
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     sqrt:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Find the square root of a number.
        ///         </item>
        ///         <item>
        ///             Returns - The square root value as type Double.
        ///         </item>
        ///         <item>
        ///             Note - Only the first argument for this operation will be utilized.
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     setvar:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Set the value of a variable to that of another variable's value.
        ///         </item>
        ///         <item>
        ///             Returns - The updated dynamic object of key and value pairs.
        ///         </item>
        ///         <item>
        ///             Note -
        ///             <list type="number">
        ///                 <item>
        ///                     The first argument needs to be just a string containing the name of the variable to have its value replaced.
        ///                 </item>
        ///                 <item>
        ///                     The second argument needs to be just a string containing the name of the variable whose value will used.
        ///                 </item>
        ///                 <item>
        ///                     If either variable name is not found in the data, no adjustment is done.
        ///                 </item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     setval:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Set the value of a variable to a new value.
        ///         </item>
        ///         <item>
        ///             Returns - The updated dynamic object of key and value pairs.
        ///         </item>
        ///         <item>
        ///             Note -
        ///             <list type="number">
        ///                 <item>
        ///                     The first argument needs to be just a string containing the name of the variable to have its value replaced.
        ///                 </item>
        ///                 <item>
        ///                     The second argument needs to be the actual value to be used.
        ///                 </item>
        ///                 <item>
        ///                     If the first argument variable name is not found in the data, no adjustment is done.
        ///                 </item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </para>
        /// <para>
        ///     setcurvepoly:
        ///     <list type="bullet">
        ///         <item>
        ///             Operation - Add a polynomial curve to the dictionary.
        ///         </item>
        ///         <item>
        ///             Returns - The updated dynamic object of key and value pairs.
        ///         </item>
        ///         <item>
        ///             Note -
        ///             <list type="number">
        ///                 <item>
        ///                     The first argument needs to be a comma delimited string of the curve coefficients, with the constant being first, coeff1 next, coeff2, etc...
        ///                 </item>
        ///                 <item>
        ///                     The second argument needs to be the curve type (AirflowCurveTypeEnumeration) by numeric value (as string) or name.
        ///                 </item>
        ///                 <item>
        ///                     The third argument (optional) needs to be the motor voltage (MotorVoltageEnumeration) by numeric value (as string) or name, or null.
        ///                 </item>
        ///                 <item>
        ///                     The fourth argument (optional) needs to be the number of coil rows (as int), or null if not applicable.
        ///                 </item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </para>
        ///
        /// </remarks>
        /// <param name="evalOps">
        /// EvaluateOperators class to contain the custom operators.
        /// This class will be used to initialize the JsonLogic Evaluator.
        /// </param>
        public static void AddCustomNailorOperators(this EvaluateOperators evalOps)
        {
            evalOps.AddOperator("setmeasure", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var arg1 = p.Apply(args[1], data);
                var arg2 = p.Apply(args[2], data);

                var controlName = arg0?.ToString();
                var value = arg1?.ToString();
                var measureValue = arg2?.ToString();

               

                if (!string.IsNullOrEmpty(measureValue))
                {
                    var dataDictionary = data.ConvertToDictionary();
                    dataDictionary["setPostText"] = measureValue;
                    return dataDictionary.ConvertToDynamic();
                }

                return null;

                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(controlName) && !string.IsNullOrEmpty(measureValue))
                {
                    var valueCompnents = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var newValue = value;
                    if (valueCompnents != null && valueCompnents.Length > 0)
                    {
                        newValue = valueCompnents[0] + measureValue;
                    }
                    else
                    {
                        newValue = value + measureValue;
                    }
                    var dataDictionary = data.ConvertToDictionary();
                    dataDictionary[controlName] = newValue;
                    return dataDictionary.ConvertToDynamic();
                }
                return null;
            });
            evalOps.AddOperator("convert", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var arg1 = p.Apply(args[1], data);
                var arg2 = p.Apply(args[2], data);
                var controlName = args[0].SelectToken("var").ToString();
                
                if (!double.TryParse(arg0?.ToString(), out var inputValue)) return null;
                var inputUnits = arg1?.ToString();
                var outputUnits = arg2?.ToString();              
                double newValue = inputValue;
                bool converted = false;
                if (string.IsNullOrEmpty(inputUnits) || string.IsNullOrEmpty(outputUnits)|| controlName == null)
                {
                    return null;
                }
                if (inputUnits.Contains("fahrenheit", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("celsius", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = (inputValue - 32.0) * (5.0 / 9.0);
                    converted= true;
                }
                else if (inputUnits.Contains("celsius", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("fahrenheit", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = (inputValue * (9.0 / 5.0)) + 32.0;
                    converted = true;
                }
                else if(inputUnits.Contains("feet", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("meters", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue / 3.2808399;
                    converted = true;
                }
                else if (inputUnits.Contains("meters", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("feet",StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue * 3.2808399;
                    converted = true;
                }
                else if (inputUnits.Contains("lps", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("cfm", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue * 2.1188799727597;
                    converted = true;
                }
                else if (inputUnits.Contains("cfm", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("lps", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue / 2.1188799727597;
                    converted = true;
                }
                else if (inputUnits.Contains("pa", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("inwg", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue * 0.0040146307866177;
                    converted = true;
                }
                else if (inputUnits.Contains("inwg", StringComparison.OrdinalIgnoreCase) && outputUnits.Contains("pa", StringComparison.OrdinalIgnoreCase))
                {
                    newValue = inputValue / 0.0040146307866177;
                    converted = true;
                }

                if(converted)
                {
                    var dataDictionary = data.ConvertToDictionary();
                    dataDictionary["conversionRequest"] = string.Format("{0},{1},{2}",controlName,newValue.ToString(),outputUnits.ToString());
                    return dataDictionary.ConvertToDynamic();       
                }
                return null;
            });
            evalOps.AddOperator("isnumericgt0", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var value = arg0?.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    if (value.All(char.IsNumber))
                    {
                        double dValue = 0;
                        double.TryParse(value?.ToString(), out dValue);
                        if (dValue > 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
            evalOps.AddOperator("isnumeric", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var value = arg0?.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    return value.All(char.IsNumber);
                }
                else
                {
                    return false;
                }
            });
            evalOps.AddOperator("validationerror", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var message = arg0?.ToString();
                var dataDictionary = data.ConvertToDictionary();

                if (!string.IsNullOrEmpty(message))
                {
                    dataDictionary["validationError"] = message;
                    return dataDictionary.ConvertToDynamic();       
                }
                else
                {
                    return null;
                }
            });
            evalOps.AddOperator("generalMessage", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var message = arg0?.ToString();
                var dataDictionary = data.ConvertToDictionary();

                if (!string.IsNullOrEmpty(message))
                {
                    dataDictionary["generalMessage"] = message;
                    return dataDictionary.ConvertToDynamic();
                }
                else
                {
                    return null;
                }
            });
            evalOps.AddOperator("changelabel", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data);
                var arg1 = p.Apply(args[1], data);
                var componentName = arg0?.ToString();
                var label = arg1?.ToString();
                var dataDictionary = data.ConvertToDictionary();

                if (dataDictionary.TryGetValue(componentName, out var componentNameValue))
                {
                    if (componentName != null && label != null)
                    {
                        dataDictionary["changeDescription"] = string.Format("{0},{1}",componentName,label);
                        return dataDictionary.ConvertToDynamic();
                    }
                }
                return null;
            });
            evalOps.AddOperator("calclat", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data); 
                var arg1 = p.Apply(args[1], data); 
                var arg2 = p.Apply(args[2], data); 
                var arg3 = p.Apply(args[3], data);

                var som = arg0?.ToString();
                if (string.IsNullOrEmpty(som)) return null;
                if (!double.TryParse(arg1?.ToString(), out var heatingAF)) return null;
                if (!double.TryParse(arg2?.ToString(), out var kW)) return null;
                if (!double.TryParse(arg3?.ToString(), out var eAT)) return null;
                             
                if (som.Contains("imperial"))
                {
                    return Math.Round(kW / (0.000317982 * heatingAF) + eAT, 1);
                }
                else if (som.Contains("metric"))
                {
                    return Math.Round((kW * 1000) / (1.23 * heatingAF) + eAT, 1);
                }
                else
                {
                    return null;
                }

            });
            evalOps.AddOperator("calckw", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data); 
                var arg1 = p.Apply(args[1], data); 
                var arg2 = p.Apply(args[2], data);
                var arg3 = p.Apply(args[3], data); 

                var som = arg0?.ToString();
                if (string.IsNullOrEmpty(som)) return null;
                if (!double.TryParse(arg1?.ToString(), out var eHeatingAF)) return null;
                if (!double.TryParse(arg2?.ToString(), out var lAT)) return null;
                if (!double.TryParse(arg3?.ToString(), out var eAT)) return null;

                if (som.Contains("imperial"))
                {
                    return Math.Round(0.000317982 * eHeatingAF * (lAT - eAT), 2);
                }
                else if(som.Contains("metric"))
                {
                    return Math.Round((1.23 * eHeatingAF * (lAT - eAT)) / 1000, 1);
                }
                else
                {
                    return null;
                }
            });
            evalOps.AddOperator("phaserule", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data); // Electrical Connections
                var arg1 = p.Apply(args[1], data); // Chassis Voltage
                var arg2 = p.Apply(args[2], data); // Motor Voltage
                var arg3 = p.Apply(args[3], data); // Phase

                var eCon = arg0?.ToString();
                if (string.IsNullOrEmpty(eCon)) return null;
                if (!int.TryParse(arg1?.ToString(), out var voltage)) return null;
                if (!int.TryParse(arg2?.ToString(), out var motorVoltage)) return null;
                if (!int.TryParse(arg3?.ToString(), out var phase)) return null;

                 
                if (eCon.Contains("SinglePoint",StringComparison.OrdinalIgnoreCase))
                {
                    if (motorVoltage == 120)
                    {
                        if (voltage == 120 || voltage == 240)
                        {
                            if (phase == 1)
                                return true;
                            else
                            {
                                return false;
                            }         
                        }
                        else if (voltage == 208)
                        {
                            if (phase == 1 || phase == 3)
                                return true;
                            else
                                return false;
                        }
                    }
                    else if (motorVoltage == 208)
                    {
                        if (voltage == 208)
                        {
                            if (phase == 1 || phase == 3)
                                return true;
                            else
                                return false;
                        }
                    }
                    else if (motorVoltage == 240)
                    {
                        if (voltage == 240)
                        {
                            if (phase == 1 || phase == 3)
                                return true;
                            else
                                return false;
                        }
                    }
                    else if (motorVoltage == 277)
                    {
                        if (voltage == 277)
                        {
                            if (phase == 1)
                                return true;
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else if (motorVoltage == 480)
                    {
                        if (voltage == 480)
                        {
                            if (phase == 1 || phase == 3)
                                return true;
                            else
                                return false;
                            
                        }
                    }
                }
                else if (eCon.Contains("DualPoint",StringComparison.OrdinalIgnoreCase))
                {
                    if (voltage == 120 || voltage == 277 || voltage == 347)
                    {
                        if (phase == 1)
                            return true;
                        else
                        {
                            return false;
                        }
                    }
                    if (voltage == 208 || voltage == 240 || voltage == 480)
                    {
                        if (phase == 1 || phase == 3)
                            return true;
                        else
                            return false;
                    }
                    if(voltage == 575 || voltage == 600)
                    {
                        if (phase == 1 || phase == 3)
                            return true;
                        else
                            return false;
                    }
                }

                    
                    return null;
            });
            evalOps.AddOperator("voltagerule", (p, args, data) =>
            {
                var arg0 = p.Apply(args[0], data); // Electrical connection
                var arg1 = p.Apply(args[1], data); // Chassis Voltage
                var arg2 = p.Apply(args[2], data); // Motor Voltage
                var arg3 = p.Apply(args[3], data); // Phase

                var eCon = arg0?.ToString();
                if (string.IsNullOrEmpty(eCon)) return null;
                if (!int.TryParse(arg1?.ToString(), out var voltage)) return null;
                if (!int.TryParse(arg2?.ToString(), out var motorVoltage)) return null;
                if (!int.TryParse(arg3?.ToString(), out var phase)) return null;

               
                if (eCon.Contains("SinglePoint", StringComparison.OrdinalIgnoreCase))
                {
                    if (phase == 1)
                    {
                        if (motorVoltage == 120)
                        {
                            if (voltage == 120 || voltage == 208 | voltage == 240)
                                return true;
                            else
                                return false;
                        }
                        else if (motorVoltage == 208)
                        {
                            if (voltage == 208)
                                return true;
                            else
                                return false;
                        }
                        else if (motorVoltage == 240)
                        {
                            if (voltage == 240)
                                return true;
                            else
                                return false;
                           
                        }
                        else if (motorVoltage == 277)
                        {
                            if (voltage == 277 || voltage == 480 )
                                return true;
                            else
                                return false;
                        }
                    }
                    else if (phase == 3)
                    {
                        if (motorVoltage == 120 || motorVoltage == 208)
                        {
                            return new List<int> { 208 };
                        }
                        else if (motorVoltage == 208)
                        {
                            if (voltage == 208)
                                return true;
                            else
                                return false;
                        }
                        else if (motorVoltage == 240)
                        {
                            if (voltage == 240)
                                return true;
                            else
                                return false;
                        }
                        else if (motorVoltage == 277)
                        {
                            if (voltage == 480)
                                return true;
                            else
                                return false;
                        }
                    }
                }
                else if (eCon.Contains("DualPoint", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return null;
            });

            evalOps.AddOperator("roundup", (p, args, data) =>
            {
                var value = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                return Math.Ceiling(value);
            });

            evalOps.AddOperator("rounduptoeven", (p, args, data) =>
            {
                var value = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                value = Math.Ceiling(value);
                if (value % 2 != 0) value++;
                return value;
            });

            evalOps.AddOperator("rounddown", (p, args, data) =>
            {
                var value = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                return Math.Floor(value);
            });

            evalOps.AddOperator("rnd", (p, args, data) =>
            {
                var value = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                //var decimals = Convert.ToInt32(args[1]);
                var decimals = Convert.ToInt32(p.Apply(args[1], data)); //changed so we can use a rule to provide how many decimal places the number should be rounded to. for ex, we may want to round to 2 places if imperial but 0 if metric
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            });

            evalOps.AddOperator("makereadonly", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                dataDictionary["makereadonly"] = p.Apply(args[0], data);
                return dataDictionary.ConvertToDynamic();
            });

            evalOps.AddOperator("setvisible", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                dataDictionary["setvisible"] = p.Apply(args[0], data);
                return dataDictionary.ConvertToDynamic();
            });

            evalOps.AddOperator("mlook", (p, args, data) =>
            {
                var matId = ((JProperty)((JObject)args[0]).First).Value.ToString();
                var cIndexList = p.Apply(args[1], data) as List<IndexLookup>;
                var rIndexList = p.Apply(args[2], data) as List<IndexLookup>;
                var matrixList = p.Apply(args[3], data) as List<PriceMatrix>;
                var isValidationLookup = false;
                if (args.Length >= 5)
                {
                    if (bool.TryParse(p.Apply(args[4], data)?.ToString(), out var boolValue))
                        isValidationLookup = boolValue;
                }

                var dataDictionary = data.ConvertToDictionary();

                if (matId.Contains("var", StringComparison.OrdinalIgnoreCase))
                {
                    // Lookup a variable from the dependency list
                    var variable = matId.Split(':')[1].Replace(Environment.NewLine, "").Replace("\"", "").Replace("}", "").Trim();
                    var value = dataDictionary.GetValueOrDefault(variable);
                    if (value is not null)
                    {
                        matId = value.ToString();
                    }
                }

                var mat = matrixList.Find(m => m.MatrixID == matId);
                if (mat is null)
                    return null;

                var cIndexOp = cIndexList.Find(i => i.MatrixID == matId);
                if (cIndexOp is null)
                    return null;

                var rIndexOp = rIndexList.Find(i => i.MatrixID == matId);
                if (rIndexOp is null)
                    return null;

                int? cIndexVal = 0;
                if (cIndexOp.HasLookupValueName)
                {
                    var lookupValue = dataDictionary.GetDictionaryValue(cIndexOp.LookupValueName);
                    lookupValue ??= dataDictionary.GetDictionaryValue(cIndexOp.LookupValueName, false); // If null, then check if it is just an issue of incorrect casing from the database
                                                                                                        //null lookupValue is allowed
                    cIndexVal = cIndexOp.GetLookupIndex(lookupValue);
                }
                else if (cIndexOp.HasLookupJsonRule)
                {
                    _ = HelperMethodsAndGeneralExtensions.HandleRulesResultType(cIndexOp.LookupJsonRule.RunJsonRule(data), data, out _, out var priceVal);
                    cIndexVal = priceVal is not null ? cIndexOp.GetLookupIndex(priceVal) : null;
                }

                int? rIndexVal = 0;
                if (rIndexOp.HasLookupValueName)
                {
                    var lookupValue = dataDictionary.GetDictionaryValue(rIndexOp.LookupValueName);
                    lookupValue ??= dataDictionary.GetDictionaryValue(rIndexOp.LookupValueName, false); // If null, then check if it is just an issue of incorrect casing from the database
                                                                                                        //null lookupValue is allowed
                    rIndexVal = rIndexOp.GetLookupIndex(lookupValue);
                }
                else if (rIndexOp.HasLookupJsonRule)
                {
                    _ = HelperMethodsAndGeneralExtensions.HandleRulesResultType(rIndexOp.LookupJsonRule.RunJsonRule(data), data, out _, out var priceVal);
                    rIndexVal = priceVal is not null ? rIndexOp.GetLookupIndex(priceVal) : null;
                }

                // Column index or row index should never be null of there is something to lookup.
                // If they are null, that means there was an issue with the lookup rule, 
                // most likely a selection has not been set yet.
                if (cIndexVal is null || rIndexVal is null || cIndexVal < 0 || rIndexVal < 0)
                    return null;

                if (mat.DataMatrix?.First()?.GetType().Equals(typeof(object[])) == true)
                {
                    // 2D array
                    var matrixL = new List<object[]>();
                    for (var i = 0; i < mat.DataMatrix.Length; i++)
                    {
                        matrixL.Add(mat.DataMatrix[i]);
                    }

                    var returnValue = matrixL.ToArray()[(int)rIndexVal][(int)cIndexVal];
                    if (returnValue is not null) return returnValue.ToUSCultureDouble();
                }
                else
                {
                    // 1D array with single values (rIndex should always be 0)
                    return mat.DataMatrix[0][(int)cIndexVal] as double?;
                }
                return null;
            });

            evalOps.AddOperator("pwr", (p, args, data) =>
            {
                var number = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                var powerVal = p.Apply(args[1], data)?.ToUSCultureDouble() ?? 1;

                return Math.Pow(number, powerVal);
            });

            evalOps.AddOperator("sqrt", (p, args, data) =>
            {
                var number = p.Apply(args[0], data)?.ToUSCultureDouble() ?? 0;
                return Math.Sqrt(number);
            });

            evalOps.AddOperator("setvar", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                var valueToSetName = p.Apply(args[0], data) as string;
                var setToValueName = p.Apply(args[1], data) as string;

                if (dataDictionary.TryGetValue(setToValueName, out object setToValue))
                {
                    dataDictionary[valueToSetName] = setToValue;
                }

                return dataDictionary.ConvertToDynamic();
            });

            evalOps.AddOperator("setval", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                var valueToSetName = p.Apply(args[0], data) as string;

                dataDictionary[valueToSetName] = p.Apply(args[1], data);

                return dataDictionary.ConvertToDynamic();
            });

            evalOps.AddOperator("btwn", (p, args, data) =>
            {
                // First argument is the value to check against
                // Second argument is the min value
                // Third argument is the max value
                var valueStr = p.Apply(args[0], data); //as string;
                var minStr = p.Apply(args[1], data); //as string;
                var maxStr = p.Apply(args[2], data); //as string;

                if (!double.TryParse(valueStr?.ToString(), out var value)) return null;
                if (!double.TryParse(minStr?.ToString(), out var min)) return null;
                if (!double.TryParse(maxStr?.ToString(), out var max)) return null;

                var result = (value >= min && value <= max);
                return result;
            });

            evalOps.AddOperator("notin", (p, args, data) =>
            {
                object needle = p.Apply(args[0], data);
                object haystack = p.Apply(args[1], data);
                if (haystack is String) return (haystack as string).IndexOf(needle.ToString()) < 0;

                return !haystack.MakeEnumerable().Any(item => item.EqualTo(needle));
            });

            evalOps.AddOperator("isqty", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                var name = p.Apply(args[0], data) as string;
                dataDictionary.TryGetValue(name, out var qtyValue);

                if (qtyValue is not null)
                {
                    if (qtyValue.IsNumeric())
                    {
                        return qtyValue.ToUSCultureDouble() > 0; //do not consider 0 a valid qty to set to 
                    }
                    else return false;
                }
                else return false;

            });



            evalOps.AddOperator("validateairflow", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                var airflow = p.Apply(args[0], data) as double?;
                var type = p.Apply(args[1], data) as string;
                if (airflow != null)
                {
                    dataDictionary[$"validateairflow-{type}"] = airflow;
                }
                return dataDictionary.ConvertToDynamic();
            });

            evalOps.AddOperator("setactvals", (p, args, data) =>
            {
                var dataDictionary = data.ConvertToDictionary();
                double finalQty = 0;

                //Get selected Width
                var widthstr = p.Apply(args[0], data) as string;
                _ = dataDictionary.TryGetValue(widthstr, out object widthobj);
                double width = widthobj.ToUSCultureDouble();

                //Get selected sect height
                var sectHstr = p.Apply(args[1], data) as string;
                _ = dataDictionary.TryGetValue(sectHstr, out object sectHobj);
                double sectH = sectHobj.ToUSCultureDouble();

                //Get selected sect width
                var sectWstr = p.Apply(args[2], data) as string;
                _ = dataDictionary.TryGetValue(sectHstr, out object sectWobj);
                double sectW = sectWobj.ToUSCultureDouble();

                //Get selected num of sections wide
                var numofsecthorizontalstr = p.Apply(args[3], data) as string;
                _ = dataDictionary.TryGetValue(numofsecthorizontalstr, out object numofsecthorizontalobj);
                double numofsecthorizontal = numofsecthorizontalobj.ToUSCultureDouble();

                //Get selected actator mfg
                var actuatorselbystr = p.Apply(args[4], data) as string;
                _ = dataDictionary.TryGetValue(actuatorselbystr, out object actuatorselbyobj);
                string actuatorselby = (string)actuatorselbyobj;

                //Get Atype string to set data dictionary
                var atypestr = p.Apply(args[5], data) as string;
                //_ = dataDictionary.TryGetValue(atypestr, out object atypeobj);
                //string atype = (string)atypeobj;

                var actRow = p.Apply(args[6], data) as string;

                List<KeyValuePair<string, double>> tempprice = new();
                string finalCode = string.Empty;
                List<Actuators> selectedActuators = new();
                List<Actuators> sortedSelectedActuators = new();
                List<KeyValuePair<string, double>> calculatedPrices = new();
                var actRowSplit = actRow.Split(';');
                foreach (var ar in actRowSplit)
                {
                    var arSplit = ar.Split(',');

                    var unitsqft = arSplit[0].ToUSCultureDouble();
                    var dampersects = arSplit[1].ToUSCultureDouble();
                    var unitsize1 = arSplit[2].ToUSCultureDouble();
                    var unitsize2 = arSplit[3].ToUSCultureDouble();
                    var price = arSplit[4].ToUSCultureDouble();
                    var area = Convert.ToDouble(Math.Round(Convert.ToDouble(width * sectH), 2, MidpointRounding.AwayFromZero) / unitsqft);
                    var sect = Convert.ToDouble(Math.Floor(area));
                    var code = arSplit[5].ToString();
                    var actuatorselbyRule = arSplit[6].ToString();
                    var tempselectedActuators = GetSelectedActuators(dampersects, sectW, sectH, unitsize1, unitsize2, unitsqft, area, sect, numofsecthorizontal, code, price, actuatorselbyRule, actuatorselby);
                    selectedActuators.AddRange(tempselectedActuators);
                }
                if (selectedActuators.Count > 0)
                {
                    if (selectedActuators.Count == 1)
                    {
                        finalCode = selectedActuators[0].Code.ToString();
                    }
                    else //more than 1 applicable actuator, break down further by damper sections and price
                    {
                        foreach (var actCode in selectedActuators)
                        {
                            //calculate number of damper sections required and multiple by section base price to get total cost
                            var actnumber = actCode.DamperSections;
                            var actprice = actCode.Price * actnumber;
                            actCode.CalculatedPrice = actprice;
                            actCode.Qty = actCode.DamperSections * actCode.HorizontalSections;
                            var kvp = new KeyValuePair<string, double>(actCode.Code, actCode.CalculatedPrice);
                            tempprice.Add(kvp);
                        }
                    }
                    //sort by lowest total cost, pick first (lowest price) actuator
                    sortedSelectedActuators = selectedActuators.OrderBy(a => a.CalculatedPrice).ToList();
                    finalCode = sortedSelectedActuators[0].Code;
                    finalQty = sortedSelectedActuators[0].DamperSections;

                }

                if (finalCode == string.Empty)
                {
                    return null;
                }
                else
                {
                    dataDictionary[atypestr] = finalCode;
                    dataDictionary[atypestr + "_Quantity"] = finalQty;
                    //dataDictionary.ConvertToDynamic();
                    return dataDictionary.ConvertToDynamic();
                }
            });

            //The following were used but all condensed to rule above (setactvals), can probably delete soon jk 8/3/23
            //evalOps.AddOperator("setvals", (p, args, data) =>
            //{
            //    var dataDictionary = data.ConvertToDictionary();
            //    for (int i = 0; i < args.Length; i += 2)
            //    {
            //        var valueToSetName = p.Apply(args[i], data) as string;
            //        dataDictionary[valueToSetName] = p.Apply(args[i + 1], data);
            //    }
            //    //var valueToSetName = p.Apply(args[0], data) as string;
            //    //dataDictionary[valueToSetName] = p.Apply(args[1], data);

            //    return dataDictionary.ConvertToDynamic();
            //});
            //    evalOps.AddOperator("selectactuator", (p, args, data) =>
            //    {
            //        var dataDictionary = data.ConvertToDictionary();
            //    return dataDictionary.ConvertToDynamic();
            //});

            //evalOps.AddOperator("selectactuator", (p, args, data) =>
            //{
            //    var dataDictionary = data.ConvertToDictionary();

            //    var actRow = p.Apply(args[4], data) as string;
            //    List<KeyValuePair<string, double>> tempprice = new();
            //    string finalCode = string.Empty;
            //    List<Actuators> selectedActuators = new();
            //    List<Actuators> sortedSelectedActuators = new();
            //    List<KeyValuePair<string, double>> calculatedPrices = new();
            //    var actRowSplit = actRow.Split(';');
            //    foreach (var ar in actRowSplit)
            //    {
            //        var arSplit = ar.Split(',');

            //        var widthstr = p.Apply(args[0], data) as string;
            //        _ = dataDictionary.TryGetValue(widthstr, out object widthobj);
            //        double width = Convert.ToDouble(widthobj);

            //        var sectHstr = p.Apply(args[1], data) as string;
            //        _ = dataDictionary.TryGetValue(sectHstr, out object sectHobj);
            //        double sectH = Convert.ToDouble(sectHobj);

            //        var sectWstr = p.Apply(args[2], data) as string;
            //        _ = dataDictionary.TryGetValue(sectHstr, out object sectWobj);
            //        double sectW = Convert.ToDouble(sectWobj);

            //        var numofsecthorizontalstr = p.Apply(args[3], data) as string;
            //        _ = dataDictionary.TryGetValue(numofsecthorizontalstr, out object numofsecthorizontalobj);
            //        double numofsecthorizontal = Convert.ToDouble(numofsecthorizontalobj);

            //        var unitsqft = Convert.ToDouble(arSplit[0]);
            //        var dampersects = Convert.ToDouble(arSplit[1]);
            //        var unitsize1 = Convert.ToDouble(arSplit[2]);
            //        var unitsize2 = Convert.ToDouble(arSplit[3]);
            //        var price = Convert.ToDouble(arSplit[4]);
            //        var area = Convert.ToDouble(Math.Round((double)(width * sectH), 2, MidpointRounding.AwayFromZero) / unitsqft);
            //        var sect = Convert.ToDouble(Math.Floor(area));
            //        var code = arSplit[5].ToString();
            //        var autoSelectRule = arSplit[6].ToString();
            //        var tempselectedActuators = GetSelectedActuators(dampersects, sectW, sectH, unitsize1, unitsize2, unitsqft, area, sect, numofsecthorizontal, code, price, autoSelectRule);
            //        selectedActuators.AddRange(tempselectedActuators);
            //    }
            //    if (selectedActuators.Count > 0)
            //    {
            //        if (selectedActuators.Count == 1)
            //        {
            //            finalCode = selectedActuators[0].Code.ToString();
            //        }
            //        else
            //        {
            //            foreach (var actCode in selectedActuators)
            //            {
            //                var actnumber = actCode.DamperSections;
            //                var actprice = actCode.Price * actnumber;
            //                actCode.CalculatedPrice = actprice;
            //                actCode.Qty = actCode.DamperSections * actCode.HorizontalSections;
            //                var kvp = new KeyValuePair<string, double>(actCode.Code, actCode.CalculatedPrice);
            //                tempprice.Add(kvp);
            //            }
            //        }
            //        sortedSelectedActuators = selectedActuators.OrderBy(a => a.CalculatedPrice).ToList();
            //        finalCode = sortedSelectedActuators[0].Code;

            //    }

            //    if (finalCode == string.Empty)
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        return finalCode;
            //    }



            //});
            //evalOps.AddOperator("actuatorqty", (p, args, data) =>
            //{
            //    var dataDictionary = data.ConvertToDictionary();

            //    var actRow = p.Apply(args[4], data) as string;
            //    List<KeyValuePair<string, double>> tempprice = new();
            //    string finalCode = string.Empty;
            //    double finalQty = 0;
            //    List<Actuators> selectedActuators = new();
            //    List<Actuators> sortedSelectedActuators = new();
            //    List<KeyValuePair<string, double>> calculatedPrices = new();
            //    var actRowSplit = actRow.Split(';');
            //    foreach (var ar in actRowSplit)
            //    {
            //        var arSplit = ar.Split(',');

            //        var widthstr = p.Apply(args[0], data) as string;
            //        _ = dataDictionary.TryGetValue(widthstr, out object widthobj);
            //        double width = Convert.ToDouble(widthobj);

            //        var sectHstr = p.Apply(args[1], data) as string;
            //        _ = dataDictionary.TryGetValue(sectHstr, out object sectHobj);
            //        double sectH = Convert.ToDouble(sectHobj);

            //        var sectWstr = p.Apply(args[2], data) as string;
            //        _ = dataDictionary.TryGetValue(sectHstr, out object sectWobj);
            //        double sectW = Convert.ToDouble(sectWobj);

            //        var numofsecthorizontalstr = p.Apply(args[3], data) as string;
            //        _ = dataDictionary.TryGetValue(numofsecthorizontalstr, out object numofsecthorizontalobj);
            //        double numofsecthorizontal = Convert.ToDouble(numofsecthorizontalobj);

            //        var unitsqft = Convert.ToDouble(arSplit[0]);
            //        var dampersects = Convert.ToDouble(arSplit[1]);
            //        var unitsize1 = Convert.ToDouble(arSplit[2]);
            //        var unitsize2 = Convert.ToDouble(arSplit[3]);
            //        var price = Convert.ToDouble(arSplit[4]);
            //        var area = Convert.ToDouble(Math.Round((double)(width * sectH), 2, MidpointRounding.AwayFromZero) / unitsqft);
            //        var sect = Convert.ToDouble(Math.Floor(area));
            //        var code = arSplit[5].ToString();
            //        var autoSelectRule = arSplit[6].ToString();

            //        var tempselectedActuators = GetSelectedActuators(dampersects, sectW, sectH, unitsize1, unitsize2, unitsqft, area, sect, numofsecthorizontal, code, price, autoSelectRule);
            //        selectedActuators.AddRange(tempselectedActuators);

            //    }
            //    if (selectedActuators.Count > 0)
            //    {
            //        if (selectedActuators.Count == 1)
            //        {
            //            finalCode = selectedActuators[0].Code.ToString();
            //        }
            //        else
            //        {
            //            foreach (var actCode in selectedActuators)
            //            {
            //                var actnumber = actCode.DamperSections;
            //                var actprice = actCode.Price * actnumber;
            //                actCode.CalculatedPrice = actprice;
            //                actCode.Qty = actCode.DamperSections * actCode.HorizontalSections;
            //                var kvp = new KeyValuePair<string, double>(actCode.Code, actCode.CalculatedPrice);
            //                tempprice.Add(kvp);
            //            }
            //        }
            //        sortedSelectedActuators = selectedActuators.OrderBy(a => a.CalculatedPrice).ToList();
            //        finalCode = sortedSelectedActuators[0].Code;
            //        finalQty = sortedSelectedActuators[0].DamperSections;

            //    }

            //    if (finalQty == 0)
            //    {
            //        return null;
            //    }
            //    else
            //    {
            //        return finalQty;
            //    }

            //});

            evalOps.AddOperator("SelectActuator", (p, args, data) =>
            {
                bool isControlDamper = false; //for easier SMP qty calc
                bool isChangeUnitSize = false; //determine if 2" needs to be added to unit sizes for actuator selection
                var modelSeries = p.Apply(args[0], data) as string;
                var isMultiSect = Convert.ToBoolean(p.Apply(args[1], data));
                //Get Atype string to set data dictionary
                var atypestr = p.Apply(args[4], data) as string;
                var dataDictionary = data.ConvertToDictionary();
                var metric = dataDictionary.TryGetValue("som", out var som) && som?.ToString().ToLower() == "metric";

                if (!dataDictionary.TryGetValue("ActSelBy", out var actSelBy)) return ClearCurrentActuator(dataDictionary, atypestr).ConvertToDynamic();
                if (actSelBy?.ToString() == "MAN" || actSelBy?.ToString() == "N/A") return ClearCurrentActuator(dataDictionary, atypestr).ConvertToDynamic();
                if (dataDictionary.TryGetValue("PowerReq", out var powReq))
                {
                    if (powReq?.ToString() == "MAN" || powReq?.ToString() == "MANQ")
                    {
                        return ClearCurrentActuator(dataDictionary, atypestr).ConvertToDynamic();
                    }
                }

                //Control Dampers
                string[] rectControlDampers = { "1000", "1002", "1100", "2000", "AMD" };
                string[] rndControlDampers = { "1090" };

                //Fire Dampers
                string[] fireSmokeDampers = { "1201-MD", "1210M", "1210SS", "1210VB", "1221-OW", "1221DOWM", "1220M", "1220SS", "1220VB", "1210", "1220", "1270", "1260", "1280", "1290", "1290S-SS", "1221-OWM" };
                //need to determine if these models have unit size changes, see query below
                //models determined by: SELECT * [Mo_Model] FROM [NS_Houston_Prod].[dbo].[tblModels] where Mo_ActuSizeChangeUnitSize1 is not NULL AND Mo_ActuSizeChangeUnitSize2 is not NULL
                //AND Mo_EditStatus not in ('D','R','M') AND Mo_AutoSelectRule is not NULL
                string[] fsdWithChangeunitSize = { "1213", "1213M", "1213SS", "1213VB", "1223", "1223-3", "1223M", "1223M-3", "1223SS", "1223SS-3", "1223VB", "1263", "1273", "1283" };
                if (dataDictionary.TryGetValue("model", out var model))
                {
                    if (fsdWithChangeunitSize.Contains(model))
                    {
                        isChangeUnitSize = true;
                    }
                }

                //Louvers
                string[] louvers = { "1600", "1700AD", "1600AD" };
                if (rectControlDampers.Contains(modelSeries) || rndControlDampers.Contains(modelSeries) || louvers.Contains(modelSeries))
                {
                    isControlDamper = true;
                }

                dataDictionary = ClearCurrentActuator(dataDictionary, atypestr);

                var unitSectionSize1Value = 0D;
                var unitSectionSize2Value = 0D;
                var unitNominalSize1Name = p.Apply(args[2], data) as string; //Nominal Size1
                var unitNominalSize2Name = p.Apply(args[3], data) as string; //Nominal Size2
                var unitNominalSize1Value = 0D;
                var unitNominalSize2Value = 0D;
                var unitSize1Value = 0D; //can be nominal or sect size 1
                var unitSize2Value = 0D; //can be nominal or sect size 2
                var numSectVValue = 1;
                var numSectHValue = 1;
                var numSectName = "numSects"; //total number of sections
                var numSectValue = 1;

                // get total number of sections (should exist for both single and multi-section) *actually does not for 1090, had to manually set numsects, maybe bc it ONLY has diameter?
                if (dataDictionary.TryGetValue(numSectName, out var numSectValueObj))
                    numSectValue = Convert.ToInt32(numSectValueObj);

                if (isMultiSect) //must use section sizes if multi-section
                {

                    if (isControlDamper) // The size 1 value is always nominal size, and the size 2 value is section size ONLY for control dampers
                    {
                        if (!dataDictionary.TryGetValue(unitNominalSize1Name, out var unitSize1ValueObj)) return dataDictionary.ConvertToDynamic();
                        unitSize1Value = unitSize1ValueObj.ToUSCultureDouble();
                    }
                    else //use section width if firedampers and multisection
                    {
                        if (!dataDictionary.TryGetValue("sectW", out var unitSize1ValueObj)) return dataDictionary.ConvertToDynamic();
                        unitSize1Value = unitSize1ValueObj.ToUSCultureDouble();
                    }

                    if (unitNominalSize1Name?.ToLower() == "diameter")
                    {
                        if (!dataDictionary.TryGetValue("sectD", out var unitSize2ValueObj)) return dataDictionary.ConvertToDynamic();
                        unitSize2Value = unitSize2ValueObj.ToUSCultureDouble();
                    }
                    else
                    {
                        if (!dataDictionary.TryGetValue("sectH", out var unitSize2ValueObj)) return dataDictionary.ConvertToDynamic();
                        unitSize2Value = unitSize2ValueObj.ToUSCultureDouble();
                    }

                    if (metric)
                    {
                        unitSize1Value = unitSize1Value.ToUSCultureDouble() / 25.4;
                        unitSize2Value = unitSize2Value.ToUSCultureDouble() / 25.4;
                    }
                    // Set number of vertical sections to NumSectV if it exists
                    if (dataDictionary.TryGetValue(SizingEnumeration.NumberSectionsVertical.DisplayName, out var numVSectObj))
                        numSectVValue = Convert.ToInt32(numVSectObj);
                    // Set number of horicontal sections to NumSectH if it exists
                    if (dataDictionary.TryGetValue(SizingEnumeration.NumberSectionsHorizontal.DisplayName, out var numHSectObj))
                        numSectHValue = Convert.ToInt32(numHSectObj);
                }
                else //use nomial sizes if singlesection
                {
                    if (!dataDictionary.TryGetValue(unitNominalSize1Name, out var unitSize1ValueObj)) return dataDictionary.ConvertToDynamic();
                    unitSize1Value = unitSize1ValueObj.ToUSCultureDouble();

                    if (unitNominalSize1Name?.ToLower() == "diameter")
                    {
                        // Only have the one size dimention, so make the second dimention equal to the first so it can be squared
                        unitSize2Value = unitSize1Value;
                    }
                    else
                    {
                        if (!dataDictionary.TryGetValue(unitNominalSize2Name, out var unitSize2ValueObj)) return dataDictionary.ConvertToDynamic();
                        unitSize2Value = unitSize2ValueObj.ToUSCultureDouble();
                    }

                    if (metric)
                    {
                        unitSize1Value = unitSize1Value.ToUSCultureDouble() / 25.4;
                        unitSize2Value = unitSize2Value.ToUSCultureDouble() / 25.4;
                    }
                    //set num of sect vertical to numOfSects (total) since num of sect vertical doesn't exist for single section units
                    numSectVValue = numSectValue;
                    numSectHValue = numSectValue;
                }
                if (unitSize1Value <= 0 || unitSize2Value <= 0) return dataDictionary.ConvertToDynamic();//1290s is setting Diameter to 0 and picking an actuator based on 0 dims - Jen 3/25/24
                var sqF = (unitSize1Value * unitSize2Value) / 144;
                if (rectControlDampers.Contains(modelSeries) || rndControlDampers.Contains(modelSeries) || louvers.Contains(modelSeries)) //For ALL Control Dampers and Louvers
                {
                    isControlDamper = true; //this will help with smp qty calc, but it's not clear how this calc works for control dampers bc Xin calculates the num of damper sects by the unit's max sqft while fire dampers already knows the num of damper sects
                    if (!dataDictionary.TryGetValue("PowerReq", out var powerReq))
                    {
                        if (modelSeries?.ToUpper() != "AMD")
                            return dataDictionary.ConvertToDynamic();
                        else
                            if (!dataDictionary.TryGetValue("ActuType", out powerReq)) return dataDictionary.ConvertToDynamic();
                    }
                    //if (!dataDictionary.TryGetValue("Fail_Pos", out var failPos)) return dataDictionary.ConvertToDynamic();
                    if (!dataDictionary.TryGetValue("CtrlType", out var ctrlType)) return dataDictionary.ConvertToDynamic();
                    if (!dataDictionary.TryGetValue("SprRet", out var sprRet)) return dataDictionary.ConvertToDynamic();
                    dataDictionary.TryGetValue("Fail_Pos", out var failPos); //sometimes this is null 
                    dataDictionary.TryGetValue("AuxSwtPkg", out var auxSwtPkg);
                    dataDictionary.TryGetValue("BladeSeal", out var bladeSeal);
                    dataDictionary.TryGetValue("JambSeal", out var jambSeal);
                    dataDictionary.TryGetValue("SMP", out var smp);
                    var viableActuators = new List<ActuatorRules>();
                    var actuatorsList = new List<ActuatorRules>();

                    if (rectControlDampers.Contains(modelSeries))
                    {
                        actuatorsList = new List<ActuatorRules>(GetRectangleControlDamperActuators(rectControlDampers));
                        //foreach (var actuatorRule in actuatorsList)
                        //{
                        //    if (actuatorRule.IsValidActuator(powerReq.ToString(),
                        //        failPos?.ToString(), ctrlType.ToString(), sprRet.ToString(), auxSwtPkg?.ToString(),
                        //        bladeSeal?.ToString(), jambSeal?.ToString(),
                        //        actSelBy.ToString()))
                        //    {
                        //        viableActuators.Add(actuatorRule);
                        //    }
                        //}
                    }
                    else if (rndControlDampers.Contains(modelSeries))
                    {
                        actuatorsList = new List<ActuatorRules>(GetRoundControlDamperActuators(rndControlDampers));
                        // The 'Max SQ Ft' value for these models is just the diameter limit in inches, so keep it as the diameter in inches
                        sqF = unitSize1Value;
                    }
                    else if (louvers.Contains(modelSeries))
                    {
                        actuatorsList = new List<ActuatorRules>(GetLouverActuators(louvers));
                    }
                    else
                    {
                        return dataDictionary.ConvertToDynamic();
                    }
                    foreach (var actuatorRule in actuatorsList)
                    {
                        if (actuatorRule.IsValidActuator(powerReq?.ToString(),
                                failPos?.ToString(), ctrlType?.ToString(), sprRet?.ToString(), auxSwtPkg?.ToString(),
                                bladeSeal?.ToString(), jambSeal?.ToString(),
                                actSelBy?.ToString()))
                        {
                            viableActuators.Add(actuatorRule);
                        }
                    }

                    if (viableActuators.Any())
                    {
                        var smallestCost = 0f;
                        string selectedActuator = null;
                        string actuatorDesc = null;
                        var actuatorQty = 0;

                        foreach (var actuator in viableActuators)
                        {

                            var (cost, qty) = actuator.CalcualateActuatorPrice(sqF, numSectVValue);
                            if (smallestCost == 0 || cost < smallestCost)
                            {
                                selectedActuator = actuator.Code;
                                smallestCost = cost;
                                actuatorQty = qty;
                                actuatorDesc = $"{actuator.Model} - {actuator.Manufacturer} {actuator.Desc}";
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(selectedActuator))
                        {
                            if (smp != null)
                            {
                                //var sectFactor = 1;
                                //if (numSectHValue > 1) sectFactor = 2;
                                int smpQty = actuatorQty; //this is probably not right, in RAPP code, num of damper sects is calculated; represented by lNumOfSectH for control dampers
                                //int smpQty = sectFactor * numSectVValue;

                                dataDictionary["SMP_Quantity"] = smpQty;
                            }
                            dataDictionary[atypestr] = selectedActuator;
                            dataDictionary[atypestr + "_Quantity"] = actuatorQty;
                            dataDictionary[atypestr + "_Description"] = actuatorDesc;
                        }
                    }
                }
                else if (fireSmokeDampers.Contains(modelSeries))
                {
                    string maxVelPrs = null;
                    string dampMount = null;
                    isControlDamper = false;
                    if (!dataDictionary.TryGetValue("PowerReq", out var powerReq)) return dataDictionary.ConvertToDynamic();
                    if (!dataDictionary.TryGetValue("Fail_Pos", out var failPos)) return dataDictionary.ConvertToDynamic();
                    if (!dataDictionary.TryGetValue("Elev_Temp", out var elevTemp)) return dataDictionary.ConvertToDynamic();
                    if (modelSeries == "1201-MD")
                    {
                        dataDictionary.TryGetValue("MaxVelPrs", out var maxVelPrsObj); //null for 1201-MD series
                        maxVelPrs = maxVelPrsObj?.ToString() ?? "NULL";
                    }
                    else
                    {
                        if (!dataDictionary.TryGetValue("MaxVelPrs", out var maxVelPrsObj)) return dataDictionary.ConvertToDynamic();
                        maxVelPrs = maxVelPrsObj?.ToString() ?? "NULL";
                    }
                    if (!dataDictionary.TryGetValue("Mounting", out var dampMountObj))
                    {
                        if (modelSeries == "1290" || modelSeries == "1290S-SS")
                        {
                            dampMount = dampMountObj?.ToString() ?? "NULL";
                        }
                        else
                        {
                            return dataDictionary.ConvertToDynamic();
                        }
                    }
                    else
                    {
                        dampMount = dampMountObj?.ToString();
                    }

                    if (!dataDictionary.TryGetValue("Actu_Moun", out var mntLoc)) return dataDictionary.ConvertToDynamic();
                    dataDictionary.TryGetValue("SMP", out var smp);

                    var actuatorsList = new List<FireSmokeActuatorRules>(GetFireSmokeDamperActuators(modelSeries));
                    var viableActuators = new List<FireSmokeActuatorRules>();

                    if (isChangeUnitSize && numSectValue == 1) //need to add numsects chaeck here because actuators always add extra 2", regarless if multisection ofr not. These sizes changes are alredy handled in the sectioning manger if the unit is multi section but not if it's single section
                    {
                        //do not need to convert, already converted above
                        //double addChangeSize = 0.0;
                        //if (metric) { addChangeSize = 50.8; }
                        //else { addChangeSize = 2; }
                        unitSize1Value += 2;
                        unitSize2Value += 2;
                    }

                    foreach (var actuatorRule in actuatorsList)
                    {
                        if (actuatorRule.IsValidActuator(powerReq?.ToString(),
                          failPos?.ToString(), maxVelPrs?.ToString(), mntLoc?.ToString(), dampMount?.ToString(), elevTemp?.ToString(),
                          actSelBy?.ToString()))
                        {
                            //Check mounting location for multisections. 
                            if (IsMountingLocationCorrect(numSectHValue, actuatorRule.MountingLocation, actuatorRule.NumberDamperSections))
                            {
                                viableActuators.Add(actuatorRule);
                            }
                            //viableActuators.Add(actuatorRule);
                        }
                    }
                    if (viableActuators.Any())
                    {
                        var smallestCost = 0f;
                        string selectedActuator = null;
                        var actuatorQty = 0;
                        var damperSects = 0;
                        string actuatorDesc = null;
                        foreach (var actuator in viableActuators)
                        {
                            // if ((unitSize1Value / actuator.NumberDamperSections) <= actuator.MaxDamperWidth && unitSize2Value <= actuator.MaxDamperHeight)
                            if ((actuator.MaxDamperWidth / actuator.NumberDamperSections) >= unitSize1Value && actuator.MaxDamperHeight >= unitSize2Value)
                            {
                                var (cost, qty) = actuator.CalcualateActuatorPrice(numSectHValue, numSectVValue, mntLoc.ToString().ToUpper());
                                if ((smallestCost == 0 || cost < smallestCost) && qty != 0)
                                {
                                    selectedActuator = actuator.Code;
                                    smallestCost = cost;
                                    actuatorQty = qty;
                                    damperSects = actuator.NumberDamperSections;
                                    actuatorDesc = $"{actuator.Model} - {actuator.Manufacturer} {actuator.Desc}";
                                }
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(selectedActuator))
                        {
                            //calculate SMP qty depending on if it's selected, if it's a control damper or not an if the numer of damper sects is <= 1
                            //if (smp != null)
                            //{
                            //    int smpQty = 0;
                            //    if (isControlDamper && damperSects <= 1)
                            //    {
                            //        smpQty = numSectVValue;
                            //    }
                            //    else if (isControlDamper && damperSects > 1)
                            //    {
                            //        smpQty = numSectVValue * 2;
                            //    }
                            //    else { smpQty = actuatorQty; }
                            //    dataDictionary["SMP_Quantity"] = smpQty;
                            //}
                            if (smp != null)
                            {
                                int smpQty = actuatorQty;
                                dataDictionary["SMP_Quantity"] = smpQty;
                            }

                            dataDictionary[atypestr] = selectedActuator;
                            dataDictionary[atypestr + "_Quantity"] = actuatorQty;
                            dataDictionary[atypestr + "_Description"] = actuatorDesc;
                        }
                    }
                }
                return dataDictionary.ConvertToDynamic();
            });

        }

        private static bool IsMountingLocationCorrect(int numSectHValue, List<string> mountingLocation, int numOfDamperSects)
        {
            decimal numSectHValueD = Convert.ToDecimal(numSectHValue);
            decimal numOfDamperSectsD = Convert.ToDecimal(numOfDamperSects);

            //Check if this is multisection, if more than 2 sections, the middle section is assumed to have a mounting location of INT. Therefore, any actuators that require EXT only are not viable options and should not be added to the available list of actuators
            if ((numSectHValueD / numOfDamperSectsD <= 2) || (numSectHValueD / numOfDamperSectsD > 2 && mountingLocation.Contains("INT")))
            {
                return true;
            }
            return false;
        }

        private static Dictionary<string, object> ClearCurrentActuator(Dictionary<string, object> dataDictionary, string atypestr)
        {
            // Clear out the current actuator selection if it exists
            if (dataDictionary.TryGetValue(atypestr, out _)) dataDictionary.Remove(atypestr);
            if (dataDictionary.TryGetValue(atypestr + "_Quantity", out _)) dataDictionary.Remove(atypestr + "_Quantity");
            if (dataDictionary.TryGetValue(atypestr + "_Description", out _)) dataDictionary.Remove(atypestr + "_Description");
            if (dataDictionary.TryGetValue("SMP_Quantity", out _)) dataDictionary.Remove("SMP_Quantity");
            return dataDictionary;
        }
        private static IEnumerable<FireSmokeActuatorRules> GetFireSmokeDamperActuators(string fireSmokeDamperSeries)
        {

            return fireSmokeDamperSeries switch
            {
                "1270" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT12","FSTF120","Belimo","120VAC","24",18,12,"250",120,"120","CL","EXT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT24","FSTF24","Belimo","24VAC","24",18,12,"250",125,"24","CL","EXT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT23","FSTF230","Belimo","230VAC","24",18,12,"250",125,"230","CL","EXT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","24",18,12,"250",125,"120","CL","EXT, INT","V, H ,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","24",18,12,"250",130,"24","CL","EXT, INT","V, H ,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","24",18,12,"250",130,"230","CL","EXT, INT","V, H ,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",36,24,"250",130,"120","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",30,24,"250",130,"120","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",24,24,"350",130,"120","CL","EXT","V, H ,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",36,24,"250",130,"24","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",30,24,"250",130,"24","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",24,24,"350",130,"24","CL","EXT","V, H ,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",36,24,"250",130,"230","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",30,24,"250",130,"230","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",24,24,"350",130,"230","CL","EXT","V, H ,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",36,24,"250",135,"120","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF121","Belimo","120VAC","24",30,24,"250",135,"120","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF121","Belimo","120VAC","24",24,24,"250",135,"120","CL","INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF121","Belimo","120VAC","24",24,24,"350",135,"120","CL","EXT, INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",36,24,"250",135,"24","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",30,24,"250",135,"24","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"250",135,"24","CL","INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"350",135,"24","CL","EXT, INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",36,24,"250",170,"230","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",30,24,"250",170,"230","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"250",170,"230","CL","INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"350",170,"230","CL","EXT, INT","V, H ,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,48,"250",185,"120","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",30,40,"250",185,"120","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"250",185,"120","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",30,36,"250",185,"120","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"350",185,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",30,36,"350",185,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,48,"250",185,"24","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",30,40,"250",185,"24","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"250",185,"24","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",30,36,"250",185,"24","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"350",185,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",30,36,"350",185,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,48,"250",185,"230","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",30,40,"250",185,"230","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"250",185,"230","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",30,36,"250",185,"230","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"350",185,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",30,36,"350",185,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,48,"250",190,"120","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",30,40,"250",190,"120","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"350",190,"120","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",30,36,"350",190,"120","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,48,"250",190,"24","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",30,40,"250",190,"24","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,36,"350",190,"24","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",30,36,"350",190,"24","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,48,"250",210,"230","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",30,40,"250",210,"230","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,36,"350",210,"230","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",30,36,"350",210,"230","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250",225,"120","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",30,40,"250",225,"120","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",30,40,"350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,36,"250",225,"120","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",60,36,"250",225,"120","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,24,"250",225,"120","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",60,24,"250",225,"120","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,24,"350",225,"121","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",60,24,"350",225,"122","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250",225,"24","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",30,40,"250",225,"24","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",30,40,"350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,36,"250",225,"24","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",60,36,"250",225,"24","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,24,"250",225,"24","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",60,24,"250",225,"24","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,24,"350",225,"24","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",60,24,"350",225,"24","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250",225,"230","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",30,40,"250",225,"230","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",30,40,"350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,36,"250",225,"230","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",60,36,"250",225,"230","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,24,"250",225,"230","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",60,24,"250",225,"230","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,24,"350",225,"230","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",60,24,"350",225,"230","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Honeywell","120VAC","24",36,48,"350",240,"120","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Honeywell","120VAC","24",30,40,"350",240,"120","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Honeywell","24VAC","24",36,48,"350",240,"24","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Honeywell","24VAC","24",30,40,"350",240,"24","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Honeywell","230VAC","24",36,48,"350",275,"230","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Honeywell","230VAC","24",30,40,"350",275,"230","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"250",142,"25","CL","EXT, INT","V,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",30,40,"250",142,"25","CL","EXT, INT","H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"350",142,"25","CL","EXT","V,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",30,40,"350",142,"25","CL","EXT","H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",72,36,"250, 350",275,"25","CL","EXT","V,H/V",2,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",60,36,"250, 350",275,"25","CL","EXT","H,H/V",2,"AUTO, SIE")
                },
                "1260" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT12","FSTF120","Belimo","120VAC","24",18,12,"250",120,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT24","FSTF24","Belimo","24VAC","24",18,12,"250",125,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT23","FSTF230","Belimo","230VAC","24",18,12,"250",125,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","24",18,12,"250",125,"120","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","24",18,12,"250",130,"24","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","24",18,12,"250",130,"230","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",36,24,"250",130,"120","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",24,24,"350",130,"120","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",36,24,"250",130,"24","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",24,24,"350",130,"24","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",36,24,"250",130,"230","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",24,24,"350",130,"230","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",36,24,"250",135,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"250",135,"120","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"350",135,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF120","Belimo","24VAC","24",36,24,"250",135,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF120","Belimo","24VAC","24",24,24,"250",135,"24","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF120","Belimo","24VAC","24",24,24,"350",135,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF120","Belimo","230VAC","24",36,24,"250",170,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF120","Belimo","230VAC","24",24,24,"250",170,"230","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF120","Belimo","230VAC","24",24,24,"350",170,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,48,"250",185,"120","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,36,"250",185,"120","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,36,"350",185,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC, FATPA MT","24",36,48,"250",185,"24","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC, FATPA MT","24",36,36,"250",185,"24","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC, FATPA MT","24",36,36,"350",185,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC, FATPA MT","24",36,48,"250",185,"230","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC, FATPA MT","24",36,36,"250",185,"230","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC, FATPA MT","24",36,36,"350",185,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,48,"250",190,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"350",190,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,36,"350",190,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,36,"350",190,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,36,"350",210,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,36,"350",210,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",36,48,"250",225,"120","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,36,"250",225,"120","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,24,"250",225,"120","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",36,48,"350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,24,"350",225,"120","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",36,48,"250, 350",225,"120","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",36,48,"250",225,"24","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,36,"250",225,"24","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,24,"250",225,"24","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",36,48,"350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,24,"350",225,"24","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",36,48,"250, 350",225,"24","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",36,48,"250",225,"230","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,36,"250",225,"230","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,24,"250",225,"230","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",36,48,"350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,24,"350",225,"230","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",36,48,"250, 350",225,"230","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",36,48,"350",240,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",36,48,"350",240,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",36,48,"350",275,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"250",142,"25","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"350",142,"25","CL","EXT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",72,36,"250, 350",275,"25","CL","EXT","H/V",2,"AUTO, SIE")
                },
                "1220" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT12","FSTF120","Belimo","120VAC","24",18,10,"250",120,"120","CL","EXT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT24","FSTF24","Belimo","24VAC","24",18,10,"250",125,"24","CL","EXT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT23","FSTF230","Belimo","230VAC","24",18,10,"250",125,"230","CL","EXT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","24, 34, 36, 46",18,10,"250",125,"120","CL","EXT, INT","V, H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","24, 34, 36, 46",18,10,"250",130,"24","CL","EXT, INT","V, H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","24, 34, 36, 46",18,10,"250",130,"230","CL","EXT, INT","V, H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",36,24,"250",130,"120","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",32,24,"250",130,"120","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",24,24,"250",130,"120","CL","INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC, FATPA LT","24",24,24,"350",130,"120","CL","EXT, INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",36,24,"250",130,"24","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",32,24,"250",130,"24","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",24,24,"250",130,"24","CL","INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC, FATPA LT","24",24,24,"350",130,"24","CL","EXT, INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",36,24,"250",130,"230","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",32,24,"250",130,"230","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",24,24,"250",130,"230","CL","INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC, FATPA LT","24",24,24,"350",130,"230","CL","EXT, INT","V, H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",36,24,"250",135,"120","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",32,24,"250",135,"120","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"250",135,"120","CL","INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"350",135,"120","CL","EXT, INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",36,24,"250",135,"24","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",32,24,"250",135,"24","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"250",135,"24","CL","INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"350",135,"24","CL","EXT, INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",36,24,"250",135,"230","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",32,24,"250",170,"230","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"250",170,"230","CL","INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"350",170,"230","CL","EXT, INT","V, H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,48,"250",185,"120","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",32,48,"250",185,"120","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,36,"250",185,"120","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",32,36,"250",185,"120","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",36,36,"350",185,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC, FATPA MT","24",32,36,"350",185,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS4109F","Honeywell","120VAC, FATPA MT","24",36,48,"250",185,"24","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","120VAC, FATPA MT","24",32,48,"250",185,"24","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","120VAC, FATPA MT","24",36,36,"250",185,"24","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","120VAC, FATPA MT","24",32,36,"250",185,"24","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","120VAC, FATPA MT","24",36,36,"350",185,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","120VAC, FATPA MT","24",32,36,"350",185,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",36,48,"250",185,"230","CL","EXT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",32,48,"250",185,"230","CL","EXT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",36,36,"250",185,"230","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",32,36,"250",185,"230","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",36,36,"350",185,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","120VAC, FATPA MT","24",32,36,"350",185,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,48,"250",190,"120","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",32,48,"250",190,"120","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"350",190,"120","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",32,36,"350",190,"120","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"250",190,"120","OP","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"250",190,"120","OP","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",36,48,"250",190,"24","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",32,48,"250",190,"24","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",36,48,"350",190,"24","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",32,36,"350",190,"24","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",36,36,"250",190,"24","OP","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSLF24","Belimo","24VAC","24",36,36,"250",190,"24","OP","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",36,48,"250",210,"230","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",32,48,"250",210,"230","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",36,48,"350",210,"230","CL","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",32,36,"350",210,"230","CL","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",36,36,"250",210,"230","OP","EXT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSLF230","Belimo","230VAC","24",36,36,"250",210,"230","OP","EXT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",36,48,"250",225,"120","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",32,48,"250",225,"120","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",36,48,"350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",32,48,"350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,36,"250",225,"120","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",64,36,"250",225,"120","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,24,"250",225,"120","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",64,24,"250",225,"120","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",72,24,"350",225,"120","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","24",64,24,"350",225,"120","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","34",36,48,"250, 350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","34",32,48,"250, 350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","36",36,48,"250, 350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","36",32,48,"250, 350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","46",36,48,"250, 350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","46",32,48,"250, 350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","48",36,48,"250",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","48",32,48,"250",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","48",36,24,"350",225,"120","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC, FATPA HT","48",32,24,"350",225,"120","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",36,48,"250",225,"24","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",32,48,"250",225,"24","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",36,48,"350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",32,48,"350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,36,"250",225,"24","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",64,36,"250",225,"24","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,24,"250",225,"24","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",64,24,"250",225,"24","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",72,24,"350",225,"24","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","24",64,24,"350",225,"24","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","34",36,48,"250, 350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","34",32,48,"250, 350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","36",36,48,"250, 350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","36",32,48,"250, 350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","46",36,48,"250, 350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","46",32,48,"250, 350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","48",36,48,"250",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","48",32,48,"250",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","48",36,24,"350",225,"24","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC, FATPA HT","48",32,24,"350",225,"24","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",36,48,"250",225,"230","CL","INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",32,48,"250",225,"230","CL","INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",36,48,"350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",32,48,"350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,36,"250",225,"230","CL","EXT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",64,36,"250",225,"230","CL","EXT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,24,"250",225,"230","CL","INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",64,24,"250",225,"230","CL","INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",72,24,"350",225,"230","CL","EXT, INT","V,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","24",64,24,"350",225,"230","CL","EXT, INT","H,H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","34",36,48,"250, 350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","34",32,48,"250, 350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","36",36,48,"250, 350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","36",32,48,"250, 350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","46",36,48,"250, 350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","46",32,48,"250, 350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","48",36,48,"250",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","48",32,48,"250",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","48",36,24,"350",225,"230","CL","EXT, INT","V,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC, FATPA HT","48",32,24,"350",225,"230","CL","EXT, INT","H,H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",36,48,"350",240,"120","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",32,48,"350",240,"120","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",72,30,"250",240,"120","CL","EXT","V,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",64,30,"250",240,"120","CL","EXT","H,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",36,48,"350",240,"24","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",32,48,"350",240,"24","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",72,30,"250",240,"24","CL","EXT","V,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",64,30,"250",240,"24","CL","EXT","H,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",36,48,"350",275,"230","CL","EXT, INT","V,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",32,48,"350",275,"230","CL","EXT, INT","H,H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",72,30,"250",275,"230","CL","EXT","V,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",64,30,"250",275,"230","CL","EXT","H,H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"250",142,"25","CL","EXT, INT","V,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",32,48,"250",143,"25","CL","EXT, INT","H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"350",144,"25","CL","EXT","V,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",32,48,"350",145,"25","CL","EXT","H,H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",72,36,"250, 350",275,"25","CL","EXT","V,H/V",2,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",64,36,"250, 350",275,"25","CL","EXT","H,H/V",2,"AUTO, SIE")
                },
                "1221-OW" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemans","120VAC","24",18,10,"250",125,"120","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemans","24VAC","24",18,10,"250",130,"24","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemans","230VAC","24",18,10,"250",130,"230","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"250, 350",135,"120","CL","INT","V, H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"250, 350",135,"24","CL","INT","V, H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"250, 350",170,"230","CL","INT","V, H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"250, 350",185,"120","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",32,36,"250, 350",185,"120","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"250, 350",185,"24","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",32,36,"250, 350",185,"24","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"250, 350",190,"230","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",32,36,"250, 350",190,"230","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,48,"250",190,"120","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",32,48,"250",190,"120","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,48,"250",190,"24","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",32,48,"250",190,"24","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,48,"250",210,"230","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",32,48,"250",210,"230","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250, 350",225,"120","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",32,48,"250, 350",225,"120","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250, 350",225,"24","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",32,48,"250, 350",225,"24","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250, 350",225,"230","CL","INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",32,48,"250, 350",225,"230","CL","INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",36,48,"350",240,"120","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",32,48,"350",240,"120","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",36,48,"350",240,"24","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",32,48,"350",240,"24","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",36,48,"350",275,"230","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",32,48,"350",275,"230","CL","INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemans","25 psi Pneumatic","24",36,48,"250",142,"25","CL","INT","V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemans","25 psi Pneumatic","24",32,48,"250",142,"25","CL","INT","H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemans","120VAC","34",18,10,"250",125,"120","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemans","120VAC","36",18,10,"250",125,"120","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemans","120VAC","46",18,10,"250",125,"120","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemans","24VAC","34",18,10,"250",130,"24","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemans","24VAC","36",18,10,"250",130,"24","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemans","24VAC","46",18,10,"250",130,"24","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemans","230VAC","34",18,10,"250",130,"230","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemans","230VAC","36",18,10,"250",130,"230","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemans","230VAC","46",18,10,"250",130,"230","CL","EXT, INT","V, H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","34",36,48,"250, 350",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","34",32,48,"250, 350",225,"120","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","36",36,48,"250, 350",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","36",32,48,"250, 350",225,"120","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","46",36,48,"250, 350",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","46",32,48,"250, 350",225,"120","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","34",36,48,"250, 350",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","34",32,48,"250, 350",225,"24","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","36",36,48,"250, 350",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","36",32,48,"250, 350",225,"24","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","46",36,48,"250, 350",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","46",32,48,"250, 350",225,"24","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","34",36,48,"250, 350",225,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","34",32,48,"250, 350",225,"230","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","36",36,48,"250, 350",225,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","36",32,48,"250, 350",225,"230","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","46",36,48,"250, 350",225,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","46",32,48,"250, 350",225,"230","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",36,48,"250",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",32,48,"250",225,"120","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",36,24,"350",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",32,24,"350",225,"120","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",36,48,"250",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",32,48,"250",225,"24","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",36,24,"350",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",32,24,"350",225,"24","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",36,48,"250",225,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",32,48,"250",225,"230","CL","EXT, INT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",36,24,"350",225,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",32,24,"350",225,"230","CL","EXT, INT","H",1,"AUTO, HON")
                },
                "1220SS" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",18,18,"250",185,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",18,18,"250",185,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",18,18,"250",190,"230","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",30,48,"250",225,"120","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",30,48,"250",225,"24","CL","EXT, INT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",30,48,"250",225,"230","CL","EXT, INT","V",1,"AUTO, HON")

                },
                "1220M" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",36,48,"250",310,"24","CL","EXT, INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",32,48,"250",310,"24","CL","EXT, INT","H",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296P","331-2961PR","Siemens","25 psi Pneumatic","24",36,36,"250",285,"25","CL","EXT","V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296P","331-2961PR","Siemens","25 psi Pneumatic","24",32,36,"250",285,"25","CL","EXT","H",1,"AUTO, SIE")
                },
                "1220VB" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",18,18,"250",185,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",18,18,"250",185,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",18,18,"250",190,"230","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",48,36,"250",225,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",48,36,"250",225,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",48,36,"250",225,"230","CL","EXT","V",1,"AUTO, HON")
                },
                "1221DOWM" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",36,48,"250",310,"24","CL","EXT, INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",32,48,"250",310,"24","CL","EXT, INT","H",1,"AUTO, BEL")
                },
                "1221-OWM" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",36,48,"250",310,"24","CL","INT","V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",32,48,"250",310,"24","CL","INT","H",1,"AUTO, BEL")
                },
                "1210" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT12","FSTF120","Belimo","120VAC","24",18,10,"250",120,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT24","FSTF24","Belimo","24VAC","24",18,10,"250",125,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FT23","FSTF230","Belimo","230VAC","24",18,10,"250",125,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","24",18,10,"250",125,"120","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","24",18,10,"250",130,"24","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","24",18,10,"250",130,"230","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",36,24,"250",130,"120","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",24,24,"250",130,"120","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",24,24,"350",130,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",36,24,"250",130,"24","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",24,24,"250",130,"24","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",24,24,"350",130,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",36,24,"250",130,"230","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",24,24,"250",130,"230","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",24,24,"350",130,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",36,24,"250",135,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"250",135,"120","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",24,24,"350",135,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",36,24,"250",135,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"250",135,"24","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",24,24,"350",135,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",36,24,"250",170,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"250",170,"230","CL","INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",24,24,"350",170,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,48,"250",185,"120","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"250",185,"120","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"350",185,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,48,"250",185,"24","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"250",185,"24","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"350",185,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,48,"250",190,"230","CL","EXT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"250",190,"230","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"350",190,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,48,"250",190,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",36,36,"350",190,"120","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,48,"250",190,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",36,36,"350",190,"24","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,48,"250",210,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",36,36,"350",210,"230","CL","EXT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250",225,"120","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,36,"250",225,"120","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,24,"250",225,"120","CL","INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",72,24,"350",225,"120","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250, 350",225,"120","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250",225,"24","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,36,"250",225,"24","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,24,"250",225,"24","CL","INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",72,24,"350",225,"24","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250, 350",225,"24","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250",225,"230","CL","INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,36,"250",225,"230","CL","EXT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,24,"250",225,"230","CL","INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",72,24,"350",225,"230","CL","EXT, INT","H/V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250, 350",225,"230","OP","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","34",36,48,"250, 350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","36",36,48,"250, 350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","46",36,48,"250, 350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","34",36,48,"250, 350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","36",36,48,"250, 350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","46",36,48,"250, 350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","34",36,48,"250, 350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","36",36,48,"250, 350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","46",36,48,"250, 350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",36,48,"250",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","48",36,24,"350",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",36,48,"250",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","48",36,24,"350",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",36,48,"250",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","48",36,24,"350",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",36,48,"350",360,"120","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA12","FSAF120A","Belimo","120VAC","24",72,30,"250",360,"120","CL","EXT","H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",36,48,"350",360,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA24","FSAF24A","Belimo","24VAC","24",72,30,"250",360,"24","CL","EXT","H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",36,48,"350",415,"230","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FA23","FSAF230A","Belimo","230VAC","24",72,30,"250",415,"230","CL","EXT","H/V",2,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25VAC","24",36,48,"250",213,"25","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"350",213,"25","CL","EXT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"306","331-3060","Siemens","25 psi Pneumatic","24",72,36,"250, 350",413,"25","CL","EXT","H/V",2,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","34",18,10,"250",185,"120","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","36",18,10,"250",185,"120","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST12","GJD22X","Siemens","120VAC","46",18,10,"250",185,"120","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","34",18,10,"250",195,"24","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","36",18,10,"250",195,"24","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST24","GJD12X","Siemens","24VAC","46",18,10,"250",195,"24","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","34",18,10,"250",195,"230","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","36",18,10,"250",195,"230","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"ST23","GJD32X","Siemens","230VAC","46",18,10,"250",195,"230","CL","EXT, INT","H/V",1,"AUTO, SIE")
                },
                "1210M" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FAM","FSAFB24-SR","Belimo","24VAC/ DC","24",36,48,"250",310,"24","CL","EXT, INT","H/V",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296P","331-2961PR","Siemens","25 psi Pneumatic","24",36,36,"250",285,"25","CL","EXT","H/V",1,"AUTO, SIE")
                },
                "1210SS" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",18,18,"250",185,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",18,18,"250",185,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",18,18,"250",190,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON")
                },
                "1210VB" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",18,18,"250",185,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",18,18,"250",185,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",18,18,"250",190,"230","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",48,36,"250",225,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",48,36,"250",225,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",48,36,"250",225,"230","CL","EXT","V",1,"AUTO, HON")
                },
                "1280" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",36,36,"250",185,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",36,36,"250",185,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",36,36,"250",190,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","24",36,48,"250",225,"120","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","24",36,48,"250",225,"24","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","24",36,48,"250",225,"230","CL","EXT, INT","H/V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"250",142,"25","CL","EXT, INT","H/V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",36,48,"350",142,"25","CL","EXT","H/V",1,"AUTO, SIE")
                },
                "1290" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",14,14,"250",130,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",14,14,"350",130,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",14,14,"250",130,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",14,14,"350",130,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",14,14,"250",130,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",14,14,"350",130,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","24",14,14,"250",135,"120","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","24",14,14,"250",135,"24","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","24",14,14,"250",170,"230","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","34",14,14,"250",135,"120","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","34",14,14,"250",135,"24","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","34",14,14,"250",170,"230","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL12","FSLF120","Belimo","120VAC","44",14,14,"250",135,"120","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL24","FSLF24","Belimo","24VAC","44",14,14,"250",135,"24","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"FL23","FSLF230","Belimo","230VAC","44",14,14,"250",170,"230","CL, OP","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",14,14,"250",190,"120","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F12","FSNF120","Belimo","120VAC","24",14,14,"350",190,"120","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",14,14,"250",190,"24","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F24","FSNF24","Belimo","24VAC","24",14,14,"350",190,"24","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",14,14,"250",210,"230","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"F23","FSNF230","Belimo","230VAC","24",14,14,"350",210,"230","CL","EXT","H/V,NULL",1,"AUTO, BEL"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",24,24,"250",185,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",24,24,"350",185,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",24,24,"250",185,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",24,24,"350",185,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",24,24,"250",190,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",24,24,"350",190,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","24",12,12,"250",88,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","24",12,12,"350",88,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",24,24,"250",142,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",24,24,"350",142,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE")
                },
                "1290S-SS" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",14,14,"250",130,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL1","MS4104F","Honeywell","120VAC","24",14,14,"350",130,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",14,14,"250",130,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL2","MS8104F","Honeywell","24VAC","24",14,14,"350",130,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",14,14,"250",130,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HL3","MS4604F","Honeywell","230VAC","24",14,14,"350",130,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",24,24,"250",185,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4109F","Honeywell","120VAC","24",24,24,"350",185,"120","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",24,24,"250",185,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8109F","Honeywell","24VAC","24",24,24,"350",185,"24","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",24,24,"250",190,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4609F","Honeywell","230VAC","24",24,24,"350",190,"230","CL","EXT","H/V,NULL",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","24",12,12,"250",88,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","24",12,12,"350",88,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",24,24,"250",142,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","24",24,24,"350",142,"25","CL","EXT","H/V,NULL",1,"AUTO, SIE")
                },
                "1201-MD" => new List<FireSmokeActuatorRules>
                {
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4X09F","Honeywell","120VAC","NULL",36,36,"165, 212",185,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM1","MS4X09F","Honeywell","120VAC","NULL",36,36,"165, 212",185,"120","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8X09F","Honeywell","24VAC","NULL",36,36,"165, 212",185,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM2","MS8X09F","Honeywell","24VAC","NULL",36,36,"165, 212",185,"24","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4Y09F","Honeywell","230VAC","NULL",36,36,"165, 212",190,"230","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HM3","MS4Y09F","Honeywell","230VAC","NULL",36,36,"165, 212",190,"230","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",36,36,"165, 212",225,"120","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",72,36,"165, 212",225,"120","CL","EXT","V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",36,36,"165, 212",225,"120","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",72,36,"165, 212",225,"120","CL","EXT","H",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",36,36,"165, 212",225,"120","OP","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH1","MS4120F","Honeywell","120VAC","NULL",36,36,"165, 212",225,"120","OP","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",36,36,"165, 212",225,"24","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",72,36,"165, 212",225,"24","CL","EXT","V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",36,36,"165, 212",225,"24","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",72,36,"165, 212",225,"24","CL","EXT","H",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",36,36,"165, 212",225,"24","OP","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH2","MS8120F","Honeywell","24VAC","NULL",36,36,"165, 212",225,"24","OP","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",36,36,"165, 212",225,"230","CL","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",72,36,"165, 212",225,"230","CL","EXT","V",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",36,36,"165, 212",225,"230","CL","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",72,36,"165, 212",225,"230","CL","EXT","H",2,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",36,36,"165, 212",225,"230","OP","EXT","V",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"HH3","MS4620F","Honeywell","230VAC","NULL",36,36,"165, 212",225,"230","OP","EXT","H",1,"AUTO, HON"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","NULL",36,36,"165, 212",88,"25","CL","EXT","V",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"482","331-4826","Siemens","25 psi Pneumatic","NULL",36,36,"165, 212",88,"25","CL","EXT","H",1,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","NULL",72,48,"165, 212",142,"25","CL","EXT","V",2,"AUTO, SIE"),
                    new FireSmokeActuatorRules(fireSmokeDamperSeries,"296","331-2961","Siemens","25 psi Pneumatic","NULL",72,36,"165, 212",142,"25","CL","EXT","H",2,"AUTO, SIE")
                },
                _ => new List<FireSmokeActuatorRules>(),
            };
        }
        private static IEnumerable<ActuatorRules> GetRectangleControlDamperActuators(string[] rectControlDampers)
        {
            //string[] rectControlDampers = { "1000", "1100", "2000" };
            return new List<ActuatorRules>
                {
                    new ActuatorRules(rectControlDampers,"TF12","FSTF120","Belimo","120Vac",6,120,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",12,130,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",24,185,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",60,225,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL12","FSLF120","Belimo","120Vac",12,135,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12","FSNF120","Belimo","120Vac",20,190,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA12","FSAF120A","Belimo","120Vac",60,240,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF24","FSTF24","Belimo","24Vac/dc",6,125,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",12,130,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",20,185,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",60,225,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL24","FSLF24","Belimo","24Vac/dc",12,135,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24","FSNF24","Belimo","24Vac/dc",20,190,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA24","FSAF24A","Belimo","24Vac/dc",60,240,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",12,130,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",20,185,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",60,225,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF12","FSTF120","Belimo","120Vac",3,120,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",30,225,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12","FSNF120","Belimo","120Vac",12,190,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA12","FSAF120A","Belimo","120Vac",30,240,"120","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF24","FSTF24","Belimo","24Vac/dc",3,125,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",30,225,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24","FSNF24","Belimo","24Vac/dc",12,190,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA24","FSAF24A","Belimo","24Vac/dc",30,240,"24","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",30,225,"230","CL, OP, NULL","2POS","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF12","FSTF120","Belimo","120Vac",3,120,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",30,225,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12","FSNF120","Belimo","120Vac",12,190,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA12","FSAF120A","Belimo","120Vac",30,240,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF24","FSTF24","Belimo","24Vac/dc",3,125,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",30,225,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24","FSNF24","Belimo","24Vac/dc",12,190,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA24","FSAF24A","Belimo","24Vac/dc",30,240,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",30,225,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF12","FSTF120","Belimo","120Vac",3,120,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",30,225,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12","FSNF120","Belimo","120Vac",12,190,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA12","FSAF120A","Belimo","120Vac",30,240,"120","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF24","FSTF24","Belimo","24Vac/dc",3,125,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",30,225,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24","FSNF24","Belimo","24Vac/dc",12,190,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA24","FSAF24A","Belimo","24Vac/dc",30,240,"24","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",30,225,"230","CL, OP, NULL","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF1S","FSTF120-S","Belimo","120Vac w/Aux. Switches",6,170,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA w/Aux. Switches",12,180,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA w/Aux. Switches",20,235,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA w/Aux. Switches",60,275,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL1S","FSLF120-S","Belimo","120Vac w/Aux. Switches",12,185,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12S","FSNF120-S","Belimo","120Vac w/Aux. Switches",20,240,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA1S","FSAF120A-S","Belimo","120Vac w/Aux. Switches",50,290,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF2S","FSTF24-S","Belimo","24Vac/dc with Auxiliary Switch",6,175,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA w/Aux. Switches",12,180,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA w/Aux. Switches",20,235,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA w/Aux. Switches",60,275,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL2S","FSLF24-S","Belimo","24Vac/dc w/Aux. Switches",12,185,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24S","FSNF24-S","Belimo","24Vac/dc w/Aux. Switches",20,240,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA2S","FSAF24A-S","Belimo","24Vac/dc w/Aux. Switches",60,290,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA w/Aux. Switches",12,180,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA w/Aux. Switches",20,235,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA w/Aux. Switches",60,275,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF1S","FSTF120-S","Belimo","120Vac w/Aux. Switches",3,170,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA w/Aux. Switches",6,180,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA w/Aux. Switches",12,235,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA w/Aux. Switches",30,275,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL1S","FSLF120-S","Belimo","120Vac w/Aux. Switches",6,185,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12S","FSNF120-S","Belimo","120Vac w/Aux. Switches",12,240,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA1S","FSAF120A-S","Belimo","120Vac w/Aux. Switches",25,290,"120","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF2S","FSTF24-S","Belimo","24Vac/dc with Auxiliary Switch",3,175,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA w/Aux. Switches",6,180,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA w/Aux. Switches",12,235,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA w/Aux. Switches",30,275,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL2S","FSLF24-S","Belimo","24Vac/dc w/Aux. Switches",6,185,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24S","FSNF24-S","Belimo","24Vac/dc w/Aux. Switches",12,240,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA2S","FSAF24A-S","Belimo","24Vac/dc w/Aux. Switches",30,290,"24","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA w/Aux. Switches",6,180,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA w/Aux. Switches",12,235,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA w/Aux. Switches",30,275,"230","CL, OP, NULL","2POS","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF1S","FSTF120-S","Belimo","120Vac w/Aux. Switches",3,170,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA w/Aux. Switches",6,180,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA w/Aux. Switches",12,235,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA w/Aux. Switches",30,275,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL1S","FSLF120-S","Belimo","120Vac w/Aux. Switches",6,185,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12S","FSNF120-S","Belimo","120Vac w/Aux. Switches",12,240,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA1S","FSAF120A-S","Belimo","120Vac w/Aux. Switches",25,290,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF2S","FSTF24-S","Belimo","24Vac/dc with Auxiliary Switch",3,175,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA w/Aux. Switches",6,180,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA w/Aux. Switches",12,235,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA w/Aux. Switches",30,275,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL2S","FSLF24-S","Belimo","24Vac/dc w/Aux. Switches",6,185,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24S","FSNF24-S","Belimo","24Vac/dc w/Aux. Switches",12,240,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA2S","FSAF24A-S","Belimo","24Vac/dc w/Aux. Switches",30,290,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA w/Aux. Switches",6,180,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA w/Aux. Switches",12,235,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA w/Aux. Switches",30,275,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA w/Aux. Switches",30,275,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"TF1S","FSTF120-S","Belimo","120Vac w/Aux. Switches",3,170,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA w/Aux. Switches",6,180,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA w/Aux. Switches",12,235,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA w/Aux. Switches",30,275,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL1S","FSLF120-S","Belimo","120Vac w/Aux. Switches",6,185,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F12S","FSNF120-S","Belimo","120Vac w/Aux. Switches",12,240,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA1S","FSAF120A-S","Belimo","120Vac w/Aux. Switches",25,290,"120","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"TF2S","FSTF24-S","Belimo","24Vac/dc with Auxiliary Switch",3,175,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA w/Aux. Switches",6,180,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA w/Aux. Switches",12,235,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA w/Aux. Switches",30,275,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"FL2S","FSLF24-S","Belimo","24Vac/dc w/Aux. Switches",6,185,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"F24S","FSNF24-S","Belimo","24Vac/dc w/Aux. Switches",12,240,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"FA2S","FSAF24A-S","Belimo","24Vac/dc w/Aux. Switches",30,290,"24","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA w/Aux. Switches",6,180,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA w/Aux. Switches",12,235,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA w/Aux. Switches",30,275,"230","CL, OP, NULL","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"BTM","TFB24-SR","Belimo","24Vac/dc, 2-10Vdc",6,220,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLM","LF24-SR US","Belimo","24Vac/dc, 2-10Vdc",12,260,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNM","NFB24-SR","Belimo","24Vac/dc, 2-10Vdc",32,305,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAM","AFB24-SR","Belimo","24Vac/dc, 2-10Vdc",60,345,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTM","TFB24-SR","Belimo","24Vac/dc, 2-10Vdc",3,220,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLM","LF24-SR US","Belimo","24Vac/dc, 2-10Vdc",6,260,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNM","NFB24-SR","Belimo","24Vac/dc, 2-10Vdc",16,305,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAM","AFB24-SR","Belimo","24Vac/dc, 2-10Vdc",30,345,"24","CL, OP, NULL","MOD","SPR","NULL","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTM","TFB24-SR","Belimo","24Vac/dc, 2-10Vdc",3,220,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLM","LF24-SR US","Belimo","24Vac/dc, 2-10Vdc",6,260,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNM","NFB24-SR","Belimo","24Vac/dc, 2-10Vdc",16,305,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAM","AFB24-SR","Belimo","24Vac/dc, 2-10Vdc",30,345,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTM","TFB24-SR","Belimo","24Vac/dc, 2-10Vdc",3,220,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLM","LF24-SR US","Belimo","24Vac/dc, 2-10Vdc",6,260,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNM","NFB24-SR","Belimo","24Vac/dc, 2-10Vdc",16,305,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAM","AFB24-SR","Belimo","24Vac/dc, 2-10Vdc",30,345,"24","CL, OP, NULL","MOD","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"S70","MS7505A2030","Honeywell","24Vac/dc, 0/2-10Vdc",12,235,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71","MS7510A2008","Honeywell","24Vac/dc, 0/2-10Vdc",20,280,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72","MS7520A2007","Honeywell","24Vac/dc, 0/2-10Vdc",60,310,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70","MS7505A2030","Honeywell","24Vac/dc, 0/2-10Vdc",7,235,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71","MS7510A2008","Honeywell","24Vac/dc, 0/2-10Vdc",12,280,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72","MS7520A2007","Honeywell","24Vac/dc, 0/2-10Vdc",30,310,"24","CL, OP, NULL","MODF","SPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70","MS7505A2030","Honeywell","24Vac/dc, 0/2-10Vdc",7,235,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71","MS7510A2008","Honeywell","24Vac/dc, 0/2-10Vdc",12,280,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72","MS7520A2007","Honeywell","24Vac/dc, 0/2-10Vdc",30,310,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70","MS7505A2030","Honeywell","24Vac/dc, 0/2-10Vdc",7,235,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71","MS7510A2008","Honeywell","24Vac/dc, 0/2-10Vdc",12,280,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72","MS7520A2007","Honeywell","24Vac/dc, 0/2-10Vdc",30,310,"24","CL, OP, NULL","MODF","SPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"BTMS","TFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switch (1)",6,255,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLMS","LF24-SR-S US","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",12,295,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNMS","NFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",32,345,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAMS","AFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",60,385,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTMS","TFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switch (1)",3,255,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLMS","LF24-SR-S US","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",6,295,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNMS","NFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",16,345,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAMS","AFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",30,385,"24","CL, OP, NULL","MOD","SPR","AUXS","NULL","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTMS","TFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switch (1)",3,255,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLMS","LF24-SR-S US","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",6,295,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNMS","NFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",16,345,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAMS","AFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",30,385,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BTMS","TFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switch (1)",3,255,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BLMS","LF24-SR-S US","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",6,295,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BNMS","NFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",16,345,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"BAMS","AFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",30,385,"24","CL, OP, NULL","MOD","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, BEL"),
                    new ActuatorRules(rectControlDampers,"S70S","MS7505A2130","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switch (1)",12,270,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71S","MS7510A2206","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",20,345,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72S","MS7520A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",60,430,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70S","MS7505A2130","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switch (1)",7,270,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71S","MS7510A2206","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,345,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72S","MS7520A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",30,430,"24","CL, OP, NULL","MODF","SPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70S","MS7505A2130","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switch (1)",7,270,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71S","MS7510A2206","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,345,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72S","MS7520A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",30,430,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S70S","MS7505A2130","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switch (1)",7,270,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S71S","MS7510A2206","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,345,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"S72S","MS7520A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",30,430,"24","CL, OP, NULL","MODF","SPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60","MN6105A1011","Honeywell","24Vac/dc, On/Off",12,135,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61","MN6110A1003","Honeywell","24Vac/dc, On/Off",20,194,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62","MN6120A1002","Honeywell","24Vac/dc, On/Off",50,310,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60","MN6105A1011","Honeywell","24Vac/dc, On/Off",7,135,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61","MN6110A1003","Honeywell","24Vac/dc, On/Off",12,194,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62","MN6120A1002","Honeywell","24Vac/dc, On/Off",25,310,"24","CL, OP, NULL","FL","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60","MN6105A1011","Honeywell","24Vac/dc, On/Off",7,135,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61","MN6110A1003","Honeywell","24Vac/dc, On/Off",12,194,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62","MN6120A1002","Honeywell","24Vac/dc, On/Off",25,310,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60","MN6105A1011","Honeywell","24Vac/dc, On/Off",7,135,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61","MN6110A1003","Honeywell","24Vac/dc, On/Off",12,194,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62","MN6120A1002","Honeywell","24Vac/dc, On/Off",25,310,"24","CL, OP, NULL","FL","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60S","MN6105A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",12,177,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61S","MN6110A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",20,252,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62S","MN6120A1200","Honeywell","24Vac/dc, On/Off w/Aux. Switches",50,380,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60S","MN6105A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",7,177,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61S","MN6110A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",12,252,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62S","MN6120A1200","Honeywell","24Vac/dc, On/Off w/Aux. Switches",25,380,"24","CL, OP, NULL","FL","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60S","MN6105A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",7,177,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61S","MN6110A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",12,252,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62S","MN6120A1200","Honeywell","24Vac/dc, On/Off w/Aux. Switches",25,380,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N60S","MN6105A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",7,177,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N61S","MN6110A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",12,252,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N62S","MN6120A1200","Honeywell","24Vac/dc, On/Off w/Aux. Switches",25,380,"24","CL, OP, NULL","FL","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70","MN7505A2001","Honeywell","24Vac/dc, 2-10Vdc",12,200,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71","MN7510A2001","Honeywell","24Vac/dc, 2-10Vdc",20,226,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72","MN7220A2007","Honeywell","24Vac/dc, 0/2-10Vdc",50,315,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70","MN7505A2001","Honeywell","24Vac/dc, 2-10Vdc",7,200,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71","MN7510A2001","Honeywell","24Vac/dc, 2-10Vdc",12,226,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72","MN7220A2007","Honeywell","24Vac/dc, 0/2-10Vdc",25,315,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70","MN7505A2001","Honeywell","24Vac/dc, 2-10Vdc",7,200,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71","MN7510A2001","Honeywell","24Vac/dc, 2-10Vdc",12,226,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72","MN7220A2007","Honeywell","24Vac/dc, 0/2-10Vdc",25,315,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70","MN7505A2001","Honeywell","24Vac/dc, 2-10Vdc",7,200,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71","MN7510A2001","Honeywell","24Vac/dc, 2-10Vdc",12,226,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72","MN7220A2007","Honeywell","24Vac/dc, 0/2-10Vdc",25,315,"24","CL, OP, NULL","MOD, MODF","NSPR","NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70S","MN7505A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,275,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71S","MN7510A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",20,325,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72S","MN7220A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",50,435,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70S","MN7505A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",7,275,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71S","MN7510A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,325,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72S","MN7220A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",25,435,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","NULL","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70S","MN7505A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",7,275,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71S","MN7510A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,325,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72S","MN7220A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",25,435,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","NULL","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N70S","MN7505A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",7,275,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N71S","MN7510A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",12,325,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"N72S","MN7220A2205","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",25,435,"24","CL, OP, NULL","MOD, MODF","NSPR","AUXS","BPV, BSS, BSP, BSSP","JSS","AUTO, HON"),
                    new ActuatorRules(rectControlDampers,"482","331-4826","Siemens","#3 (8 - 13 spring)",20,88,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296","331-2961","Siemens","#4 (8 - 13 spring)",50,142,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306","331-3060","Siemens","#6 (8 - 13 spring)",84,275,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482","331-4826","Siemens","#3 (8 - 13 spring)",10,88,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296","331-2961","Siemens","#4 (8 - 13 spring)",25,142,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306","331-3060","Siemens","#6 (8 - 13 spring)",42,275,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482","331-4826","Siemens","#3 (8 - 13 spring)",10,88,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296","331-2961","Siemens","#4 (8 - 13 spring)",25,142,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306","331-3060","Siemens","#6 (8 - 13 spring)",42,275,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482","331-4826","Siemens","#3 (8 - 13 spring)",10,88,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296","331-2961","Siemens","#4 (8 - 13 spring)",25,142,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306","331-3060","Siemens","#6 (8 - 13 spring)",42,275,"PNU","CL, OP, NULL","2POS","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482P","331-4826PR","Siemens","#3 (8 - 13 spring) with pilot positioner",16,231,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296P","331-2961PR","Siemens","#4 (8 - 13 spring) with pilot positioner",40,285,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306P","331-3060PR","Siemens","#6 (8 - 13 spring) with pilot positioner",70,443,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482P","331-4826PR","Siemens","#3 (8 - 13 spring) with pilot positioner",8,231,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296P","331-2961PR","Siemens","#4 (8 - 13 spring) with pilot positioner",20,285,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306P","331-3060PR","Siemens","#6 (8 - 13 spring) with pilot positioner",35,443,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","NULL","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482P","331-4826PR","Siemens","#3 (8 - 13 spring) with pilot positioner",8,231,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296P","331-2961PR","Siemens","#4 (8 - 13 spring) with pilot positioner",20,285,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306P","331-3060PR","Siemens","#6 (8 - 13 spring) with pilot positioner",35,443,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","NULL","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"482P","331-4826PR","Siemens","#3 (8 - 13 spring) with pilot positioner",8,231,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"296P","331-2961PR","Siemens","#4 (8 - 13 spring) with pilot positioner",20,285,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE"),
                    new ActuatorRules(rectControlDampers,"306P","331-3060PR","Siemens","#6 (8 - 13 spring) with pilot positioner",35,443,"PNU","CL, OP, NULL","MOD","SPR","300, NULL","BPV, BSS, BSP, BSSP","JSS","AUTO, SIE")
                };
        }
        private static IEnumerable<ActuatorRules> GetRoundControlDamperActuators(string[] rndControlDampers)
        {
            //string[] rndControlDampers = { "1090" };
            return new List<ActuatorRules>
            {
                new ActuatorRules(rndControlDampers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",14,130,"120","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",24,185,"120","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"FL12","FSLF120","Belimo","120Vac",14,135,"120","CL, OP","2POS","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"F12","FSNF120","Belimo","120Vac",24,190,"120","CL, OP","2POS","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",14,130,"24","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",24,185,"24","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"FL24","FSLF24","Belimo","24Vac/dc",14,135,"24","CL, OP","2POS","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"F24","FSNF24","Belimo","24Vac/dc",24,190,"24","CL, OP","2POS","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",14,130,"230","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",24,185,"230","CL, OP","2POS","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA w/Aux. Switches",14,180,"120","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA w/Aux. Switches",24,235,"120","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"FL1S","FSLF120-S","Belimo","120Vac w/Aux. Switches",14,185,"120","CL, OP","2POS","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"F12S","FSNF120-S","Belimo","120Vac w/Aux. Switches",24,240,"120","CL, OP","2POS","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA w/Aux. Switches",14,180,"24","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA w/Aux. Switches",24,235,"24","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"FL2S","FSLF24-S","Belimo","24Vac/dc w/Aux. Switches",14,185,"24","CL, OP","2POS","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"F24S","FSNF24-S","Belimo","24Vac/dc w/Aux. Switches",24,240,"24","CL, OP","2POS","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA w/Aux. Switches",14,180,"230","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA w/Aux. Switches",24,235,"230","CL, OP","2POS","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"S70","MS7505A2030","Honeywell","24Vac/dc, 0/2-10Vdc",14,235,"24","CL, OP","MODF","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"S71","MS7510A2008","Honeywell","24Vac/dc, 0/2-10Vdc",24,280,"24","CL, OP","MODF","SPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"S70S","MS7505A2130","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switch (1)",14,270,"24","CL, OP","MODF","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"S71S","MS7510A2206","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",24,345,"24","CL, OP","MODF","SPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"BLM","LF24-SR US","Belimo","24Vac/dc, 2-10Vdc",14,260,"24","CL, OP","MOD","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"BNM","NFB24-SR","Belimo","24Vac/dc, 2-10Vdc",24,305,"24","CL, OP","MOD","SPR","NULL","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"BLMS","LF24-SR-S US","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",14,295,"24","CL, OP","MOD","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"BNMS","NFB24-SR-S","Belimo","24Vac/dc, 2-10Vdc w/Aux. Switches",24,345,"24","CL, OP","MOD","SPR","AUXS","","","AUTO,BEL"),
                new ActuatorRules(rndControlDampers,"N60","MN6105A1011","Honeywell","24Vac/dc, On/Off",14,135,"24","CL, OP","FL","NSPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N61","MN6110A1003","Honeywell","24Vac/dc, On/Off",24,194,"24","CL, OP","FL","NSPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N60S","MN6105A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",14,177,"24","CL, OP","FL","NSPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N61S","MN6110A1201","Honeywell","24Vac/dc, On/Off w/Aux. Switches",24,252,"24","CL, OP","FL","NSPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N70","MN7505A2001","Honeywell","24Vac/dc, 2-10Vdc",14,200,"24","CL, OP","MOD,MODF","NSPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N71","MN7510A2001","Honeywell","24Vac/dc, 2-10Vdc",24,226,"24","CL, OP","MOD,MODF","NSPR","NULL","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N70S","MN7505A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",14,275,"24","CL, OP","MOD,MODF","NSPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"N71S","MN7510A2209","Honeywell","24Vac/dc, 0/2-10Vdc w/Aux. Switches",24,325,"24","CL, OP","MOD,MODF","NSPR","AUXS","","","AUTO,HON"),
                new ActuatorRules(rndControlDampers,"482","331-4826","Siemens","#3 (8 - 13 spring)",14,88,"PNU","CL, OP","2POS","SPR","NULL","","","AUTO,SIE"),
                new ActuatorRules(rndControlDampers,"296","331-2961","Siemens","#4 (8 - 13 spring)",24,142,"PNU","CL, OP","2POS","SPR","NULL","","","AUTO,SIE"),
                new ActuatorRules(rndControlDampers,"482P","331-4826PR","Siemens","#3 (8 - 13 spring) with pilot positioner",14,231,"PNU","CL, OP","MOD","SPR","NULL","","","AUTO,SIE"),
                new ActuatorRules(rndControlDampers,"296P","331-2961PR","Siemens","#3 (8 - 13 spring) with pilot positioner",24,285,"PNU","CL, OP","MOD","SPR","NULL","","","AUTO,SIE")
            };
        }
        private static IEnumerable<ActuatorRules> GetLouverActuators(string[] louvers)
        {
            //string[] louvers = { "1600", "1700AD", "1600AD" };
            return new List<ActuatorRules>
            {
                new ActuatorRules(louvers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",12,130,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",24,185,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",60,225,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL12","FSLF120","Belimo","120Vac",12,135,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F12","FSNF120","Belimo","120Vac",20,190,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA12","FSAF120A","Belimo","120Vac",60,240,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1","GVD221","Siemens","120Vac",75,220,"120","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",12,130,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",24,185,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",60,225,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL24","FSLF24","Belimo","24Vac/dc",12,135,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F24","FSNF24","Belimo","24Vac/dc",20,190,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA24","FSAF24A","Belimo","24Vac/dc",60,240,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2","GVD121","Siemens","24Vac",75,220,"24","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",12,130,"230","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",20,185,"230","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",50,225,"230","CL, OP","2POS","SPR","NULL","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",40,225,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F12","FSNF120","Belimo","120Vac",10,190,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA12","FSAF120A","Belimo","120Vac",40,240,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1","GVD221","Siemens","120Vac",50,220,"120","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",40,225,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F24","FSNF24","Belimo","24Vac/dc",10,190,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA24","FSAF24A","Belimo","24Vac/dc",40,240,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2","GVD121","Siemens","24Vac",50,220,"24","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",25,225,"230","CL, OP","2POS","SPR","NULL","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",40,225,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F12","FSNF120","Belimo","120Vac",10,190,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA12","FSAF120A","Belimo","120Vac",40,240,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1","GVD221","Siemens","120Vac",50,220,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",40,225,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F24","FSNF24","Belimo","24Vac/dc",10,190,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA24","FSAF24A","Belimo","24Vac/dc",40,240,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2","GVD121","Siemens","24Vac",50,220,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",25,225,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HL1","MS4104F10","Honeywell","120Vac, FATPA",6,130,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM1","MS4109F10","Honeywell","120Vac, FATPA",12,185,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH1","MS4120F10","Honeywell","120Vac, FATPA",40,225,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL12","FSLF120","Belimo","120Vac",6,135,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F12","FSNF120","Belimo","120Vac",10,190,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA12","FSAF120A","Belimo","120Vac",40,240,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1","GVD221","Siemens","120Vac",50,220,"120","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2","MS8104F10","Honeywell","24Vac, FATPA",6,130,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM2","MS8109F10","Honeywell","24Vac, FATPA",12,185,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH2","MS8120F10","Honeywell","24Vac, FATPA",40,225,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL24","FSLF24","Belimo","24Vac/dc",6,135,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F24","FSNF24","Belimo","24Vac/dc",10,190,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA24","FSAF24A","Belimo","24Vac/dc",40,240,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2","GVD121","Siemens","24Vac",50,220,"24","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3","MS4604F10","Honeywell","230Vac, FATPA",6,130,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM3","MS4609F10","Honeywell","230Vac, FATPA",12,185,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH3","MS4620F10","Honeywell","230Vac, FATPA",25,225,"230","CL, OP","2POS","SPR","NULL","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA with Auxiliary Switch",12,180,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA with Auxiliary Switch",24,235,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA with Auxiliary Switch",60,275,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL1S","FSLF120-S","Belimo","120Vac with Auxiliary Switch",12,185,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F12S","FSNF120A-S","Belimo","120Vac with Auxiliary Switch",20,240,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA1S","FSAF120A-S","Belimo","120Vac with Auxiliary Switch",60,290,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1S","GVD226","Siemens","120Vac with Auxiliary Switch",75,270,"120","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA with Auxiliary Switch",12,180,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA with Auxiliary Switch",24,235,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA with Auxiliary Switch",60,275,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL2S","FSLF24-S","Belimo","24Vac/dc with Auxiliary Switch",12,185,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F24S","FSNF24-S","Belimo","24Vac/dc with Auxiliary Switch",20,240,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA2S","FSAF24A-S","Belimo","24Vac/dc with Auxiliary Switch",60,290,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2S","GVD126","Siemens","24Vac with Auxiliary Switch",75,270,"24","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA with Auxiliary Switch",12,180,"230","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA with Auxiliary Switch",20,235,"230","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA with Auxiliary Switch",50,275,"230","CL, OP","2POS","SPR","AUXS","00","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA with Auxiliary Switch",6,180,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA with Auxiliary Switch",12,235,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA with Auxiliary Switch",40,275,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL1S","FSLF120-S","Belimo","120Vac with Auxiliary Switch",6,185,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F12S","FSNF120A-S","Belimo","120Vac with Auxiliary Switch",10,240,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA1S","FSAF120A-S","Belimo","120Vac with Auxiliary Switch",40,290,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1S","GVD226","Siemens","120Vac with Auxiliary Switch",50,270,"120","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA with Auxiliary Switch",6,180,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA with Auxiliary Switch",12,235,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA with Auxiliary Switch",40,275,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL2S","FSLF24-S","Belimo","24Vac/dc with Auxiliary Switch",6,185,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F24S","FSNF24-S","Belimo","24Vac/dc with Auxiliary Switch",10,240,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA2S","FSAF24A-S","Belimo","24Vac/dc with Auxiliary Switch",40,290,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2S","GVD126","Siemens","24Vac with Auxiliary Switch",50,270,"24","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA with Auxiliary Switch",6,180,"230","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA with Auxiliary Switch",12,235,"230","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA with Auxiliary Switch",25,275,"230","CL, OP","2POS","SPR","AUXS","00","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA with Auxiliary Switch",6,180,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA with Auxiliary Switch",12,235,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA with Auxiliary Switch",40,275,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL1S","FSLF120-S","Belimo","120Vac with Auxiliary Switch",6,185,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F12S","FSNF120A-S","Belimo","120Vac with Auxiliary Switch",10,240,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA1S","FSAF120A-S","Belimo","120Vac with Auxiliary Switch",40,290,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1S","GVD226","Siemens","120Vac with Auxiliary Switch",50,270,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA with Auxiliary Switch",6,180,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA with Auxiliary Switch",12,235,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA with Auxiliary Switch",40,275,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"FL2S","FSLF24-S","Belimo","24Vac/dc with Auxiliary Switch",6,185,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"F24S","FSNF24-S","Belimo","24Vac/dc with Auxiliary Switch",10,240,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"FA2S","FSAF24A-S","Belimo","24Vac/dc with Auxiliary Switch",40,290,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2S","GVD126","Siemens","24Vac with Auxiliary Switch",50,270,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA with Auxiliary Switch",6,180,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA with Auxiliary Switch",12,235,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA with Auxiliary Switch",25,275,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSN","AUTO, HON"),
                new ActuatorRules(louvers,"HL1S","MS4104F12","Honeywell","120Vac, FATPA with Auxiliary Switch",6,180,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM1S","MS4109F12","Honeywell","120Vac, FATPA with Auxiliary Switch",12,235,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH1S","MS4120F12","Honeywell","120Vac, FATPA with Auxiliary Switch",40,275,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL1S","FSLF120-S","Belimo","120Vac with Auxiliary Switch",6,185,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F12S","FSNF120A-S","Belimo","120Vac with Auxiliary Switch",10,240,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA1S","FSAF120A-S","Belimo","120Vac with Auxiliary Switch",40,290,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH1S","GVD226","Siemens","120Vac with Auxiliary Switch",50,270,"120","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL2S","MS8104F12","Honeywell","24Vac, FATPA with Auxiliary Switch",6,180,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM2S","MS8109F12","Honeywell","24Vac, FATPA with Auxiliary Switch",12,235,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH2S","MS8120F12","Honeywell","24Vac, FATPA with Auxiliary Switch",40,275,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"FL2S","FSLF24-S","Belimo","24Vac/dc with Auxiliary Switch",6,185,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"F24S","FSNF24-S","Belimo","24Vac/dc with Auxiliary Switch",10,240,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"FA2S","FSAF24A-S","Belimo","24Vac/dc with Auxiliary Switch",40,290,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, BEL"),
                new ActuatorRules(louvers,"SH2S","GVD126","Siemens","24Vac with Auxiliary Switch",50,270,"24","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, SIE"),
                new ActuatorRules(louvers,"HL3S","MS4604F12","Honeywell","230Vac, FATPA with Auxiliary Switch",6,180,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HM3S","MS4609F12","Honeywell","230Vac, FATPA with Auxiliary Switch",12,235,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
                new ActuatorRules(louvers,"HH3S","MS4620F12","Honeywell","230Vac, FATPA with Auxiliary Switch",25,275,"230","CL, OP","2POS","SPR","AUXS","BPV, BSS, BSP, BSSP","JSM, JSS","AUTO, HON"),
            };
        }

        private static double GetActuatorQty(double dampersects, double sectW, double sectH, double unitsize1, double unitsize2, double unitsqft, double area, double sect, double numofsecthorizontal)
        {
            double lNumOfSectH = 0;
            if (dampersects != 99)
            {
                if ((sectW * dampersects <= unitsize1) && (sectH <= unitsize2) && (sectW * sectH <= unitsqft))
                {
                    lNumOfSectH = dampersects;
                }
            }
            //rule4
            else if (sectH <= unitsize2)
            {
                if (area == sect)
                {
                    lNumOfSectH = sect;

                }
                else
                {
                    lNumOfSectH = sect + 1;
                }
                if (lNumOfSectH > numofsecthorizontal)
                {
                    lNumOfSectH = 0;
                }
                if (lNumOfSectH > 99)
                {
                    lNumOfSectH = 90;
                }
            }
            return lNumOfSectH;
        }
        private static List<Actuators> GetSelectedActuators(double dampersects, double sectW, double sectH, double unitsize1, double unitsize2, double unitsqft, double area, double sect, double numofsecthorizontal, string code, double price, string actuatorselbyRule, string actuatorselby)
        {
            List<Actuators> selectedActuators = new();
            if (unitsqft == 9999999)
            {
                if ((sectW * dampersects) <= unitsize1 && sectH <= unitsize2)
                {
                    Actuators selectedActutor = new Actuators
                    {
                        Code = code,
                        Price = price,
                        DamperSections = dampersects,
                        CalculatedPrice = price,
                        HorizontalSections = numofsecthorizontal

                    };

                    if (actuatorselby != null)
                    {
                        if (actuatorselby.ToUpper() == "AUTO" || actuatorselby.ToUpper() == "MAN")
                        {
                            selectedActuators.Add(selectedActutor);
                        }
                        else if (actuatorselbyRule.ToUpper() == actuatorselby.ToUpper())
                        {
                            selectedActuators.Add(selectedActutor);
                        }

                    }
                }
            }
            else
            {
                double lNumOfSectH = GetActuatorQty(dampersects, sectW, sectH, unitsize1, unitsize2, unitsqft, area, sect, numofsecthorizontal);

                if (lNumOfSectH > 0)
                {
                    Actuators selectedActutor = new Actuators
                    {
                        Code = code,
                        Price = price,
                        DamperSections = lNumOfSectH,
                        HorizontalSections = numofsecthorizontal
                    };
                    if (actuatorselby != null)
                    {
                        if (actuatorselby.ToUpper() == "AUTO" || actuatorselby.ToUpper() == "MAN")
                        {
                            selectedActuators.Add(selectedActutor);
                        }
                        else if (actuatorselbyRule.ToUpper() == actuatorselby.ToUpper())
                        {
                            selectedActuators.Add(selectedActutor);
                        }
                    }
                }
            }
            return selectedActuators;
        }

        internal class Actuators
        {
            public string Code { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
            public double CalculatedPrice { get; set; }
            public double DamperSections { get; set; }
            public double HorizontalSections { get; set; }

        }

        internal class ActuatorRules
        {
            internal List<string> ModelSeries { get; init; }
            internal string Code { get; init; }
            internal string Model { get; init; }
            internal string Manufacturer { get; init; }
            internal string Desc { get; init; }
            internal int MaxSqFt { get; init; }
            internal float ListPrice { get; init; }
            internal string PowerReq { get; init; }
            internal List<string> FailPos { get; init; }
            internal List<string> CtrlType { get; init; }
            internal List<string> SprRet { get; init; }
            internal List<string> AuxSwtPkg { get; init; }
            internal List<string> BladeSeal { get; init; }
            internal List<string> JambSeal { get; init; }
            internal List<string> ActSelBy { get; init; }

            internal ActuatorRules(IEnumerable<string> modelSeries, string code, string model,
                string manufacturer, string desc, int maxSqFt, float listPrice,
                string powerReq, string failPos, string ctrlType, string sprRet,
                string auxSwtPkg, string bladeSeal,
                string jambSeal, string actSelBy)
            {
                ModelSeries = new List<string>(modelSeries);
                Code = code;
                Model = model;
                Manufacturer = manufacturer;
                Desc = desc;
                MaxSqFt = maxSqFt;
                ListPrice = listPrice;
                PowerReq = powerReq;
                FailPos = CreateFromValue(failPos);
                CtrlType = CreateFromValue(ctrlType);
                SprRet = CreateFromValue(sprRet);
                AuxSwtPkg = CreateFromValue(auxSwtPkg);
                BladeSeal = CreateFromValue(bladeSeal);
                JambSeal = CreateFromValue(jambSeal);
                ActSelBy = CreateFromValue(actSelBy);
            }


            //private static List<string> CreateFromValue(string value)
            //{
            //	if (string.IsNullOrWhiteSpace(value)) return new List<string>();
            //	if (value.Contains(','))
            //	{
            //		value = value.Replace(", ", ",").Replace(" ,", ",");
            //		return new List<string>(value.Split(','));
            //	}
            //	else
            //	{
            //		return new List<string>() { value };
            //	}
            //}

            //private static bool ValueInList(List<string> list, string value)
            //{
            //	if (string.IsNullOrWhiteSpace(value))
            //	{
            //		if (list.Contains("NULL") || list.Count == 0) return true;
            //	}
            //	else
            //	{
            //		if (list.Contains(value)) return true;
            //	}
            //	return false;
            //}

            internal bool IsValidActuator(string powerReq, string failPos, string ctrlType,
                string sprRet, string auxSwtPkg, string bladeSeal, string jambSeal, string actSelBy)
            {
                if (PowerReq != powerReq)
                    return false;
                if (!ValueInList(FailPos, failPos))
                    return false;
                if (!ValueInList(CtrlType, ctrlType))
                    return false;
                if (!ValueInList(SprRet, sprRet))
                    return false;
                if (!ValueInList(AuxSwtPkg, auxSwtPkg))
                    return false;
                if (!ValueInList(BladeSeal, bladeSeal))
                    return false;
                if (!ValueInList(JambSeal, jambSeal))
                    return false;
                if (!ValueInList(ActSelBy, actSelBy))
                    return false;
                return true;
            }

            internal (float cost, int qty) CalcualateActuatorPrice(double unitSqFt, int numSectV)
            {
                // Calculate the total price for this actuator given the sq feet of the unit
                // For louvers and control dampers, multiply qty by numSectV as per RAPP code line 672 in Calculte()
                var numActuators = Convert.ToInt32(Math.Ceiling(unitSqFt / MaxSqFt)) * numSectV;
                return (ListPrice * numActuators, numActuators);
            }
        }

        internal class FireSmokeActuatorRules
        {
            internal string ModelSeries { get; init; }
            internal string Code { get; init; }
            internal string Model { get; init; }
            internal string Manufacturer { get; init; }
            internal string Desc { get; init; }
            //internal int MaxSqFt { get; init; }
            internal int MaxDamperWidth { get; init; }
            internal int MaxDamperHeight { get; init; }
            internal List<string> ElevatedTemp { get; init; }
            internal float ListPrice { get; init; }
            internal string PowerReq { get; init; }
            internal List<string> FailPos { get; init; }
            //internal List<string> CtrlType { get; init; }
            internal List<string> MountingLocation { get; init; }
            internal List<string> DamperMounting { get; init; }
            internal int NumberDamperSections { get; init; }
            internal List<string> MaxVelPrs { get; init; }
            //internal List<string> AuxSwtPkg { get; init; }
            //internal List<string> BladeSeal { get; init; }
            //internal List<string> JambSeal { get; init; }
            internal List<string> ActSelBy { get; init; }

            internal FireSmokeActuatorRules(string modelSeries, string code, string model,
                string manufacturer, string desc, string maxVelPrs, int maxDamW, int maxDamH, string elevTemp, float listPrice,
                string powerReq, string failPos, string mntLoc,
                string dampMount, int numOfDampSects,
                string actSelBy)
            {
                ModelSeries = modelSeries;
                Code = code;
                Model = model;
                Manufacturer = manufacturer;
                Desc = desc;
                MaxDamperWidth = maxDamW;
                MaxDamperHeight = maxDamH;
                ElevatedTemp = CreateFromValue(elevTemp);
                ListPrice = listPrice;
                PowerReq = powerReq;
                FailPos = CreateFromValue(failPos);
                MaxVelPrs = CreateFromValue(maxVelPrs);
                MountingLocation = CreateFromValue(mntLoc);
                DamperMounting = CreateFromValue(dampMount);
                NumberDamperSections = numOfDampSects;
                //JambSeal = CreateFromValue(jambSeal);
                ActSelBy = CreateFromValue(actSelBy);
            }


            //private static List<string> CreateFromValue(string value)
            //{
            //    if (string.IsNullOrWhiteSpace(value)) return new List<string>();
            //    if (value.Contains(','))
            //    {
            //        value = value.Replace(", ", ",").Replace(" ,", ",");
            //        return new List<string>(value.Split(','));
            //    }
            //    else
            //    {
            //        return new List<string>() { value };
            //    }
            //}

            //private static bool ValueInList(List<string> list, string value)
            //{
            //    if (string.IsNullOrWhiteSpace(value))
            //    {
            //        if (list.Contains("NULL") || list.Count == 0) return true;
            //    }
            //    else
            //    {
            //        if (list.Contains(value)) return true;
            //    }
            //    return false;
            //}

            internal bool IsValidActuator(string powerReq, string failPos, string maxVelPrs, string mountLoc, string dampMount, string elevTemp, string actSelBy)
            {
                if (PowerReq != powerReq)
                    return false;
                if (!ValueInList(FailPos, failPos))
                    return false;
                if (!ValueInList(MaxVelPrs, maxVelPrs))
                    return false;
                if (!ValueInList(ElevatedTemp, elevTemp))
                    return false;
                if (!ValueInList(MountingLocation, mountLoc))
                    return false;
                if (!ValueInList(DamperMounting, dampMount))
                    return false;
                //if(NumberDamperSections != numOfDamperSects)
                //	return false;
                //if (!ValueInList(JambSeal, jambSeal))
                //	return false;
                if (!ValueInList(ActSelBy, actSelBy))
                    return false;
                return true;
            }

            internal (float cost, int qty) CalcualateActuatorPrice(int numOfSectsH, int numOfSectsV, string moutningLoc)
            {
                int qty = 0;

                //As per RAPP code
                decimal numOfSectsHDec = Convert.ToDecimal(numOfSectsH);
                decimal numOfSectsVDec = Convert.ToDecimal(numOfSectsV);
                decimal NumberDamperSectionsDec = Convert.ToDecimal(NumberDamperSections);

                if (numOfSectsHDec <= NumberDamperSectionsDec)
                {
                    if (numOfSectsHDec <= 2 || numOfSectsHDec > 2 && moutningLoc == "INT")
                    {
                        qty = (int)numOfSectsVDec;
                    }
                }
                else if ((numOfSectsHDec / NumberDamperSectionsDec) == Math.Floor(Convert.ToDecimal(numOfSectsHDec) / Convert.ToDecimal(NumberDamperSectionsDec)))
                {
                    qty = (int)((int)(numOfSectsHDec / NumberDamperSectionsDec) * numOfSectsVDec);
                    //RAPP swicthes ETX to INT to always make this condition true, makes no sense to have it
                    //if (numOfSectsHDec / NumberDamperSectionsDec <= 2 || (numOfSectsHDec / NumberDamperSectionsDec > 2 && moutningLoc == "INT"))
                    //{
                    //    qty = (int)((int)(numOfSectsHDec / NumberDamperSectionsDec) * numOfSectsVDec);
                    //}
                }
                else if ((numOfSectsHDec / NumberDamperSectionsDec) > Math.Floor(Convert.ToDecimal(numOfSectsHDec) / Convert.ToDecimal(NumberDamperSectionsDec)))
                {
                    qty = (int)((Math.Floor(numOfSectsHDec / NumberDamperSectionsDec) + 1) * numOfSectsVDec);
                    //RAPP swicthes ETX to INT to always make this condition true, makes no sense to have it
                    //if (Math.Floor(Convert.ToDecimal(numOfSectsHDec) / Convert.ToDecimal(NumberDamperSectionsDec)) + 1 <= 2 || (Math.Floor(Convert.ToDecimal(numOfSectsHDec) / Convert.ToDecimal(NumberDamperSectionsDec)) + 1 > 2 && moutningLoc == "INT"))
                    //{
                    //    qty = (int)((Math.Floor(numOfSectsHDec / NumberDamperSectionsDec) + 1) * numOfSectsVDec);
                    //}
                }


                //// Calculate the total price for this actuator given the sq feet of the unit
                //if (NumberDamperSections == 1)
                //{
                //    qty = numOfSects;
                //}
                //else
                //{
                //    if (NumberDamperSections > numOfSects) //do not give customer
                //    {
                //        qty = 0;
                //    }
                //    else //if numsects is greater then damper sect 
                //    {
                //        //convert types to decimal first since MS handles this calculation weird
                //        decimal numOfSectsD = Convert.ToDecimal(numOfSects);
                //        decimal numOfDamperSectsD = Convert.ToDecimal(NumberDamperSections);
                //        qty = Convert.ToInt32(Math.Ceiling(numOfSectsD / numOfDamperSectsD));

                //    }
                //}
                //if(numOfSects / NumberDamperSections == 1)
                //{
                //    qty = numOfSects;
                //}
                //else if (numOfSects / NumberDamperSections > 1)
                //{
                //    qty = numOfSects + 1;
                //}
                return (ListPrice * qty, qty);
                //            if ((sectWidth/NumberDamperSections) <= MaxDamperWidth && sectHeight <= MaxDamperHeight)
                //{
                //	return ((ListPrice * numOfSects) / NumberDamperSections, NumberDamperSections);
                //}
                //var numActuators = Convert.ToInt32(Math.Ceiling(sectWidth / NumberDamperSections));
                ////return (ListPrice * numActuators, numActuators);
                //return (1,1);
            }
        }
    
        private static List<string> CreateFromValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new List<string>();
            if (value.Contains(','))
            {
                value = value.Replace(", ", ",").Replace(" ,", ",");
                return new List<string>(value.Split(','));
            }
            else
            {
                return new List<string>() { value };
            }
        }

        private static bool ValueInList(List<string> list, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (list.Contains("NULL") || list.Count == 0) return true;
            }
            else
            {
                if (list.Contains(value)) return true;
            }
            return false;
        }



        /// <summary>
        /// Convert an array of object to a named type array.
        /// </summary>
        /// <typeparam name="T">Named type to convert the array into.</typeparam>
        /// <param name="objectArray">Object array to convert.</param>
        public static T[] ConvertToSingleArr<T>(this object[] objectArray)
        {
            var returnVals = new List<T>();
            foreach (var obj in objectArray)
            {
                if (obj is Array)
                {
                    var vals = ((object[])obj).ConvertToSingleArr<T>();
                    returnVals.AddRange(vals);
                }
                else
                {
                    returnVals.Add(obj.ConvertTo<T>());
                }
            }

            return returnVals.ToArray();
        }

        /// <summary>
        /// Convert an array of object to a named type array.
        /// </summary>
        /// <typeparam name="T">Named type to convert the array into.</typeparam>
        /// <param name="objectArray">Object array to convert.</param>
        public static T[][] ConvertToMultiArr<T>(this object[][] objectArray)
        {
            var returnVals = new List<T[]>();
            foreach (var obj in objectArray)
            {
                returnVals.Add(obj.ConvertToSingleArr<T>());
            }

            return returnVals.ToArray();
        }

        /// <summary>
        /// Convert an object to a named type.
        /// </summary>
        /// <param name="o">Object to convert.</param>
        /// <typeparam name="T">Named type to convert into.</typeparam>
        public static T ConvertTo<T>(this object o)
        {
            return (T)Convert.ChangeType(o, typeof(T));
        }

        /// <summary>
        /// Convert a dictionary to a dynamic object (ExpandoObject).
        /// </summary>
        /// <param name="dictionary">Dictionary to be converted.</param>
        public static dynamic ConvertToDynamic(this Dictionary<string, object> dictionary, ILogger logger = null)
        {
            var returnVal = dictionary.Aggregate(new ExpandoObject() as IDictionary<string, object>,
                (a, p) => { a.Add(p.Key, p.Value); return a; });
            return returnVal;
        }

        /// <summary>
        /// Convert a dynamic object (ExpandoObject) to a dictionary.
        /// </summary>
        /// <param name="dynamic">Dynamic object to be converted. Must be a dynamic object (ExpandoObject).</param>
        /// <remarks>This extension method attaches to any object type class as dynamic types cannot have extension methods.</remarks>
        public static Dictionary<string, object> ConvertToDictionary(this object dynamic, ILogger logger = null, IEqualityComparer<object> comparer = null)
        {
            var dictionary = new Dictionary<string, object>(comparer);
            foreach (var value in (IDictionary<string, object>)dynamic)
            {
                dictionary.Add(value.Key, value.Value);
            }
            return dictionary;
        }

        /// <summary>
        /// Run the JsonLogic rule.
        /// </summary>
        /// <param name="jsonRule">The JsonLogic rule string to be run.</param>
        /// <param name="data">The dictionary of currently selected values for the unit.</param>
        /// <remarks>Returns either a bool, dynamic object or multi-dimensional array.</remarks>
        public static object RunJsonRule(this string jsonRule, Dictionary<string, object> data, ILogger logger = null)
        {
            return jsonRule?.RunJsonRule((object)data.ConvertToDynamic(logger), logger);
        }

        /// <summary>
        /// Run the JsonLogic rule.
        /// </summary>
        /// <param name="jsonRule">The JsonLogic rule string to be run.</param>
        /// <param name="data">The dynamic object of currently selected values for the unit.</param>
        /// <remarks>Returns either a bool, dynamic object or multi-dimensional array.</remarks>
        public static object RunJsonRule(this string jsonRule, object data, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(jsonRule))
            {
                return null;
            }
            logger?.LogInformation($"Running json rule {jsonRule}");

            var evalOps = new EvaluateOperators();
            evalOps.AddCustomNailorOperators();
            var rule = JObject.Parse(jsonRule);
            var eval = new JsonLogicEvaluator(evalOps);
            var returnVal = eval.Apply(rule, data);
            return returnVal;
        }

        public static IList<string> GetVariableNamesFromJsonRule(this string jsonRule)
        {
            if (string.IsNullOrWhiteSpace(jsonRule) || (!jsonRule.Contains("var\':") && !jsonRule.Contains("var\":"))) return null;

            var rule = JToken.Parse(jsonRule);
            switch (rule)
            {
                case null:
                    return null;
                case JValue jValue:
                    return new List<string>() { jValue.Value.ToString() };
                case JArray jArray:
                    var enumList = jArray.Select(r => GetVariableNamesFromJsonRule(r.ToString()));
                    var returnList = new List<string>();
                    foreach (var enumValue in enumList)
                    {
                        returnList.AddRange(enumValue);
                    }
                    return returnList;
            }

            // Assume JObject here
            var ruleObj = (JObject)rule;
            var p = ruleObj.Properties().First();
            var opName = p.Name;
            var opArgs = (p.Value is JArray array) ? array.ToArray() : new JToken[] { p.Value };
            if (opName.Equals("var"))
            {
                return new List<string>() { opArgs[0].ToString() };
            }

            var argsList = opArgs.Select(jToken => GetVariableNamesFromJsonRule(jToken.ToString()));
            var returnArgsList = new List<string>();
            foreach (var enumValue in argsList)
            {
                if (!(enumValue?.Count > 0)) continue;
                returnArgsList.AddRange(enumValue);
            }
            return returnArgsList;
        }

        public static IEnumerable<object> Flatten(this object value)
        {
            var returnList = new List<object>();

            if (value is IEnumerable enumerable and not string and not JValue)
            {
                foreach (var o in enumerable)
                {
                    if (o is IEnumerable and not string and not JValue)
                    {
                        returnList.AddRange(Flatten(o));
                    }
                    else if (o is not null)
                    {
                        returnList.Add(o.ToString());
                    }
                }
            }
            else if (value is not null)
            {
                returnList.Add(value.ToString());
            }

            return returnList;
        }
    }
}
