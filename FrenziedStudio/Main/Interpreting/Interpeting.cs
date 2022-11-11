using Dasync.Collections;
using FrenziedStudio.Creation;
using FrenziedStudio.External;
using FrenziedStudio.Interpretation;
using MiscUtil;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using SharpSpin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ExternalSel.TextWriter;

namespace FrenziedStudio.Main
{
    public static class Interpreting
    {
        public static async void RunProject(InterpretedProject pr, bool PrintLines = false)
        {
            //Parallel.For(0, IterationCount, new ParallelOptions { MaxDegreeOfParallelism = pr.ThreadCount }, count =>
            //{
            //    await InterpretActions(pr, PrintLines);
            //});

            var sem = new SemaphoreSlim(pr.ThreadCount);

            var tasks = Enumerable.Range(0, pr.IterationCount)
                .Select(i => ExecTask(sem, i, pr))
                .ToArray();

            await Task.WhenAll(tasks);
        }
        static async Task<int> ExecTask(SemaphoreSlim sem, int index, InterpretedProject pr)
        {
            await sem.WaitAsync();
            try
            {
                Console.WriteLine($"Task {index} starting.");
                await Task.Delay(1000);
                await InterpretActions(pr, true);
                Console.WriteLine($"Task {index} done.");
                return index;
            }
            finally
            {
                sem.Release();
            }
        }

        private static async Task InterpretActions(InterpretedProject pr, bool PrintLines = false)
        {
            string ActionPattern = @"(.*?)=(.*)";

            // Local settings means settings local to the script and not the actual project
            // They exist by adding them using setvariable etc.
            List<Tuple<string, object>> LocalSettings = new List<Tuple<string, object>>();
            List<bool> CurrentTruths = new List<bool>();

            // Current loop type
            LoopType lType = LoopType.For;

            // Create a replacement manager to replace settings
            ReplacementManager _replacementManager = new ReplacementManager(pr);

            // Current automation type (selenium/pupeteer...)
            Automation _automationType = AutomationCreator.CreateAutomation(pr.AType, pr, _replacementManager);

            // Set automation object for automation replacement (pageurl/pagehtml etc)
            _replacementManager.SetAutomation(_automationType);

            // Create a method manager to deal with loops etc.
            MethodManager _loopManager = new LoopMethods(pr, _replacementManager, _automationType);
            MethodManager _conditionManager = new ConditionMethods(pr, _replacementManager, _automationType);

            for (pr.CurrentLineIndex = 0; pr.CurrentLineIndex < pr.InterpretingLines.Count; pr.CurrentLineIndex++)
            {
                // Local for modification.
                string line = pr.InterpretingLines[pr.CurrentLineIndex];

                // Empty line bool
                bool EmptyLine = string.IsNullOrEmpty(line);

                // Empty lines get ignored.
                if (EmptyLine && !pr.Loop) { continue; }

                // Commented code gets ignored "//"
                if (!EmptyLine && line[0] == '/' && line[1] == '/') { continue; }

                // Code for nested IF statements.
                if (!EmptyLine && CurrentTruths.Count != 0)
                {
                    if(line[0] != '-')
                        CurrentTruths.Clear();
                    else
                    {
                        ReturnLineData.GetLineInfoForNestedIf(ref CurrentTruths, line, out bool CurrentTruth, 
                            out string DashesForTruth, out string DashesInLine);

                        // If statement = false 
                        // -> Continue untill all ("-/--") scope is finished.
                        // -> Continue if dashes in line and in last truth are same.
                        // -> Also continue if dashes in line bigger than dashes in truth.
                        // -> If first if statement is false, just keep going until finished.
                        if (!CurrentTruth && (DashesInLine == DashesForTruth 
                            || DashesForTruth.Length < DashesInLine.Length))
                            continue;

                        // If statement is true and line contains "-", delete "-" so regex understands.
                        if (CurrentTruth && DashesInLine == DashesForTruth)
                        {
                            string TextAtStart = Regex.Match(line, @"^([\.|-].*?)(?=\w+)").Value.Replace("-", "");
                            line = Regex.Replace(line, @"^([\.|-].*?)(?=\w+)", "");
                            line = TextAtStart + line;
                        }
                    }
                }

                // If a While/For... Loop is true, and line does not contain "." and does not contain "-"
                // Go back to first line of while to redo. Unless while loop timeout count is X.
                if (pr.Loop && (EmptyLine || line[0] != '.' && line[0] != '-'))
                {
                    pr.InternalLoopCount += 1;

                    if (pr.InternalLoopCount >= pr.InternalLoopTimeout)
                        _loopManager.ResetLoop(ref lType);
                    else
                    {
                        if (_loopManager.RestartLoop(ref line, ref CurrentTruths))
                            continue;
                    }
                }

                // Made it so that empty lines can be checked for loop info. Continue here though.
                if (EmptyLine) { continue; }

                // If WhileLoop is true and line contains ".", delete "." so regex understands.
                if (pr.Loop && line[0] == '.') { line = line.Substring(1); }

                // pr.CurrentActionName = goto | ActionValue = link
                // pr.CurrentActionName = sendkeys | ActionValue = id->emailUsername->hello
                Match ActionMatch = Regex.Match(line, ActionPattern);
                pr.CurrentActionName = ActionMatch.Groups[1].Value;
                pr.CurrentActionValue = ActionMatch.Groups[2].Value;

                // Sometimes Chrome = null, and doing replace below this func throws error.
                if (pr.CurrentActionName == "chrome" && pr.CurrentActionValue == "quit") { try { _automationType.Quit(); } catch { } return; }

                // So in a method before InterpretLines() check if {{example}} if "example" is in the Settings tuple. 
                if (pr.CurrentActionValue.Contains("{{") && pr.CurrentActionValue.Contains("}}") && pr.CurrentActionName != "for")
                    LocalSettings = await _replacementManager.ReplaceActionValueAddSettingAsync(LocalSettings, pr);

                // This is ran in case the inside of a setting contained a spintax.
                _replacementManager._replaceDynamics.ReplaceSpintaxWithValue();

                // If pr.CurrentActionValue contains new line, remove it.
                if (!pr.CurrentActionValue.Contains("*\r*") && pr.CurrentActionValue.Contains("\r")) { pr.CurrentActionValue = pr.CurrentActionValue.Replace("\r", string.Empty); }

                // If PrintLines, print lines (debug).
                if (PrintLines) { Console.WriteLine(pr.CurrentActionName + "=" + pr.CurrentActionValue);  }

                #region Interpretation

                // Set new parameters for current actionvalue
                _automationType.SetAVParameters();

                try
                {
                    #region Automation Actions
                    
                    if (pr.CurrentActionName == "chrome")
                        await _automationType.Start();
                    if (pr.CurrentActionName == "goto")
                        await _automationType.Goto();
                    if (pr.CurrentActionName == "waitelement")
                        await _automationType.WaitElement();
                    if (pr.CurrentActionName == "sendkeys")
                        await _automationType.SendKeys();
                    if (pr.CurrentActionName == "typeslow")
                        await _automationType.TypeSlow();
                    if (pr.CurrentActionName == "click")
                        await _automationType.Click();
                    if (pr.CurrentActionName == "scroll")
                        await _automationType.Scroll();
                    if (pr.CurrentActionName == "screenshot")
                        await _automationType.Screenshot();
                    if (pr.CurrentActionName == "javascript")
                        await _automationType.Javascript();
                    if (pr.CurrentActionName == "switchframe")
                        await _automationType.Switchframe();
                    if (pr.CurrentActionName == "typeenter")
                        await _automationType.TypeEnter();
                    if (pr.CurrentActionName == "select")
                        await _automationType.Select();

                    #endregion

                    #region Misc Actions

                    if (pr.CurrentActionName == "rndwait")
                        _automationType.RndWait();
                    if (pr.CurrentActionName == "wait")
                        _automationType.Wait();
                    if (pr.CurrentActionName == "msgmodal")
                        _automationType.MessageModal();
                    if (pr.CurrentActionName == "writeline")
                        _automationType.WriteLine();
                    if (pr.CurrentActionName == "write")
                        _automationType.Write();
                    if (pr.CurrentActionName == "setcookies")
                        await _automationType.SetCookies();

                    #endregion

                    #region Condition Based Actions

                    if (pr.CurrentActionName == "tryelemloop")
                    {
                        lType = LoopType.ElemLoop;
                        await _automationType.ElementLoop(pr.CurrentLineIndex, true);
                    }
                    if (pr.CurrentActionName == "elemloop")
                    {
                        lType = LoopType.ElemLoop;
                        await _automationType.ElementLoop(pr.CurrentLineIndex, false);
                    }

                    if (pr.CurrentActionName == "for")
                        _loopManager.ForLoop(ref lType, LocalSettings);
                    if (pr.CurrentActionName == "while")
                        _loopManager.WhileLoop(ref lType, false);
                    if (pr.CurrentActionName == "trywhile")
                        _loopManager.WhileLoop(ref lType, true);

                    if (pr.CurrentActionName == "if")
                        _conditionManager.IfCondition(ref CurrentTruths);

                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                    if (!pr.TryLoop) { throw; }
                }

                #endregion
            }
        }
    }
}
