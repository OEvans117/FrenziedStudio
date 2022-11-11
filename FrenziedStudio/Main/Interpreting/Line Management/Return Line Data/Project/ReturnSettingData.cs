using ExternalSel;
using FrenziedStudio.Creation;
using FrenziedStudio.External;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SharpSpin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FrenziedStudio.Main
{
    public static class ReturnSettingData
    {
        /// <summary>
        /// Returns the length of settings.
        /// Used to help FOR loop (iteration).
        /// Setting.item1 = setting name
        /// Setting.item2 = setting value (can be list)
        /// </summary>
        /// <param name="ActionValue"></param>
        /// <param name="LocalSettings"></param>
        /// <param name="pr"></param>
        /// <returns></returns>
        public static int ReturnSettingLength(string ActionValue, List<Tuple<string, object>> LocalSettings, InterpretedProject pr)
        {
            foreach (var setting in pr.Settings.ToList())
            {
                string SettingNameCode = "{{" + setting.Item1 + "}}";

            MatchingStart:

                var matches = Regex.Matches(ActionValue, SettingNameCode);

                foreach (Match match in matches)
                {
                    if (SettingNameCode != match.Value) { continue; }

                    // Check if item2 of the setting (value) is a queue (list) 
                    // or a number so you can loop x times (for X times)
                    if (setting.Item2 is ConcurrentQueue<string>)
                        return ((ConcurrentQueue<string>)setting.Item2).Count;
                    else
                    {
                        var SettingIsNumeric = int.TryParse((string)setting.Item2, out int Length);
                        if (SettingIsNumeric) return Length;
                    }

                    if (matches.Count > 1)
                        goto MatchingStart;
                }
            }
            foreach (var setting in LocalSettings)
            {
                string SettingNameCode = "{{" + setting.Item1 + "}}";

            MatchingStart:

                var matches = Regex.Matches(ActionValue, SettingNameCode);

                foreach (Match match in matches)
                {
                    if (SettingNameCode != match.Value) { continue; }

                    if (setting.Item2 is ConcurrentQueue<string>)
                        return ((ConcurrentQueue<string>)setting.Item2).Count;
                    else
                    {
                        var SettingIsNumeric = int.TryParse((string)setting.Item2, out int Length);
                        if (SettingIsNumeric) return Length;
                    }

                    if (matches.Count > 1)
                        goto MatchingStart;
                }
            }

            return 0;
        }
    }
}
