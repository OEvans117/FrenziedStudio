using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ReplaceSettings
    {
        public InterpretedProject _project { get; set; }
        public ReplaceFiles _replaceFiles { get; set; }

        public ReplaceSettings(InterpretedProject _project)
        {
            this._project = _project;
            _replaceFiles = new ReplaceFiles(_project);
        }

        /// <summary>
        /// Return setting values.
        /// {{Cookies}} = Browser cookies || {{LastURLValue}} = Last URL value.
        /// {{ListName}} = Pops item from list || {{SettingName}} = Returns value.
        /// </summary>
        /// <param name="ActionValue"></param>
        /// <param name="pr"></param>
        /// <returns>Returns value of setting.</returns>
        public string ReplaceSettingWithValue(bool CreatingVariable, List<Tuple<string, object>> LocalSettings)
        {
            string ValueReplacement = string.Empty;

            if (_project.CurrentActionValue.Contains("{{LastURLValue}}"))
            {
                string LastUrlValue = _project.LastURLValue;

                StringModManager.ApplyStringFunctions(_project, ref LastUrlValue, _project.CurrentActionValue.IndexOf("{{LastURLValue}}"));

                _project.CurrentActionValue = _project.CurrentActionValue.Replace("{{LastURLValue}}", LastUrlValue);
            }

            if (_project.CurrentActionValue.Contains("{{GetCookies}}"))
            {
                string CookieString = _project.Cookies;

                StringModManager.ApplyStringFunctions(_project, ref CookieString, _project.CurrentActionValue.IndexOf("{{Cookies}}"));

                _project.CurrentActionValue = _project.CurrentActionValue.Replace("{{Cookies}}", CookieString);
            }

            #region Loop through Settings (local/global)

            foreach (var setting in _project.Settings.ToList())
            {
                string SettingNameCode = "";

                if (CreatingVariable)
                    SettingNameCode = "{{" + setting.Item1;
                else
                    SettingNameCode = "{{" + setting.Item1 + "}}";

                bool SettingIsAtStart = _project.CurrentActionValue.IndexOf(SettingNameCode) == 0;

                if (CreatingVariable && !SettingIsAtStart)
                    SettingNameCode = SettingNameCode += "}}";

                MatchingStart:

                var matches = Regex.Matches(_project.CurrentActionValue, SettingNameCode);

                foreach (Match match in matches)
                {
                    if (SettingNameCode != match.Value) { continue; }

                    int SettingPosition = match.Index + SettingNameCode.Length;

                    if (setting.Item2 is ConcurrentQueue<string>)
                    {
                        var list = (ConcurrentQueue<string>)setting.Item2;

                        list.TryDequeue(out string ListItem);

                        int Count = SettingNameCode.Length;

                        StringModManager.ApplyStringFunctions(_project.CurrentActionValue, ref ListItem, ref Count, SettingPosition);

                        _project.CurrentActionValue = _project.CurrentActionValue.Remove(match.Index, Count).Insert(match.Index, ListItem);

                        ValueReplacement = ListItem;
                    }
                    else
                    {
                        string SettingValue = (string)setting.Item2;

                        int Count = SettingNameCode.Length;

                        StringModManager.ApplyStringFunctions(_project.CurrentActionValue, ref SettingValue, ref Count, SettingPosition);

                        _project.CurrentActionValue = _project.CurrentActionValue.Remove(match.Index, Count).Insert(match.Index, SettingValue);

                        ValueReplacement = SettingValue;
                    }

                    if (matches.Count > 1)
                        goto MatchingStart;
                }
            }
            foreach (var setting in LocalSettings)
            {
                string SettingNameCode = "";

                if (CreatingVariable)
                    SettingNameCode = "{{" + setting.Item1;
                else
                    SettingNameCode = "{{" + setting.Item1 + "}}";

                bool SettingIsAtStart = _project.CurrentActionValue.IndexOf(SettingNameCode) == 0;

                if (CreatingVariable && !SettingIsAtStart)
                    SettingNameCode = SettingNameCode += "}}";

                MatchingStart:

                var matches = Regex.Matches(_project.CurrentActionValue, SettingNameCode);

                foreach (Match match in matches)
                {
                    if (SettingNameCode != match.Value) { continue; }

                    int SettingPosition = match.Index + SettingNameCode.Length;

                    if (setting.Item2 is ConcurrentQueue<string>)
                    {
                        var list = (ConcurrentQueue<string>)setting.Item2;

                        list.TryDequeue(out string ListItem);

                        int Count = SettingNameCode.Length;

                        StringModManager.ApplyStringFunctions(_project.CurrentActionValue, ref ListItem, ref Count, SettingPosition);

                        _project.CurrentActionValue = _project.CurrentActionValue.Remove(match.Index, Count).Insert(match.Index, ListItem);

                        ValueReplacement = ListItem;
                    }
                    else
                    {
                        string SettingValue = (string)setting.Item2;

                        int Count = SettingNameCode.Length;

                        StringModManager.ApplyStringFunctions(_project.CurrentActionValue, ref SettingValue, ref Count, SettingPosition);

                        _project.CurrentActionValue = _project.CurrentActionValue.Remove(match.Index, Count).Insert(match.Index, SettingValue);

                        ValueReplacement = SettingValue;
                    }

                    if (matches.Count > 1)
                        goto MatchingStart;
                }
            }

            #endregion

            return ValueReplacement;
        }
    }
}
