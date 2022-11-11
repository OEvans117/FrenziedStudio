using FrenziedStudio.Creation;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FrenziedStudio.Main
{
    public class InterpretedProject
    {
        public InterpretedProject(string ProjectText)
        {
            ReplacementManager = new ReplacementManager(this);

            var matches = Regex.Matches(ProjectText, @"\[(?<group>.*?)\]\r\n(?<v>(?:.*[\r\n]*)+?)\[/\k<group>\]").Cast<Match>().Reverse();

            foreach (Match match in matches)
            {
                List<string> idList = match.Groups["v"].Value.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

                idList.RemoveAt(idList.Count - 1);

                if (match.Value.Contains("[Settings]"))
                {
                    SetSettings(idList, false);
                }
                else if (match.Value.Contains("[Interpret]"))
                {
                    InterpretingLines = idList;
                }
                else if (match.Value.Contains("[Project]"))
                {
                    Title = idList.Where(s => s.Split('=')[0] == "Title").First().Split('=')[1];
                    Description = idList.Where(s => s.Split('=')[0] == "Description").First().Split('=')[1];

                    var AnyIteration = idList.Where(s => s.Split('=')[0] == "IterationCount").Any();
                    if(AnyIteration)
                        IterationCount = Convert.ToInt32(idList.Where(s => s.Split('=')[0] == "IterationCount").First().Split('=')[1]);

                    var LinkedProjects = idList.Where(s => s.Split('=')[0] == "LinkedProjects");
                    bool ContainsLinkedProjects = LinkedProjects.Any();

                    if(ContainsLinkedProjects)
                        LinkedProjectList = new List<string>(LinkedProjects.First().Split('=')[1].Split(','));
                }
                else if (match.Value.Contains("[/Request"))
                {
                    string requestPattern = @"\[Request=(.*?)->(.*?)\]";
                    string formatType = SentenceCase(Regex.Match(match.Value, requestPattern).Groups[1].Value);
                    string requestName = Regex.Match(match.Value, requestPattern).Groups[2].Value;
                    Enum.TryParse(formatType, out HttpRequestInterpreting.RequestType formatTypeEnum);
                    Requests.Add(HttpRequestInterpreting.CreateHttpRequest(requestName, idList.ToArray(), formatTypeEnum));
                }
            }
        }

        /// <summary>
        /// Goes through list of SettingName=SettingValues
        /// & adds them to settings list/replaces settings.
        /// </summary>
        /// <param name="idList">List of settings that will be added/replaced.</param>
        /// <param name="Reset">Reset the current Settings, to add new ones?</param>
        public async void SetSettings(List<string> idList, bool Reset)
        {
            if (Reset) { Settings.Clear(); }

            foreach (string setting in idList)
            {
                if (setting.Contains("="))
                {
                    string SettingName = setting.Split('=')[0];
                    string SettingValue = setting.Split('=')[1];

                    if (SettingName == "ThreadCount")
                    {
                        try { ThreadCount = Convert.ToInt32(SettingValue); } catch { }
                    }
                    else if (SettingName == "IterationCount")
                    {
                        try { IterationCount = Convert.ToInt32(SettingValue); } catch { }
                    }
                    else if (SettingName == "Proxies")
                    {
                        if (File.Exists(SettingValue)) { Proxies = new ConcurrentQueue<string>(File.ReadAllLines(SettingValue)); }
                    }
                    else if (SettingName == "Agents")
                    {
                        if (File.Exists(SettingValue)) { UserAgents = new ConcurrentQueue<string>(File.ReadAllLines(SettingValue)); }
                    }
                    else if (SettingName == "ShowBrowser")
                    {
                        try { ShowBrowser = bool.Parse(SettingValue); } catch { }
                    }
                    else if (SettingName == "ShowImages")
                    {
                        try { ShowImages = bool.Parse(SettingValue); } catch { }
                    }
                    else if (SettingName == "SocksProxy")
                    {
                        try { SocksProxy = bool.Parse(SettingValue); } catch { }
                    }
                    else if(SettingName == "Browser")
                    {
                        try { AType = (AutomationType)Convert.ToInt32(SettingValue); } catch { }
                    }
                    else if (SettingValue.Contains(".txt") && Reset)
                    {
                        // Only call this if resetting previous values.
                        // We want to include the text file for modification before starting project.

                        if (File.Exists(SettingValue)) { Settings.Add(new Tuple<string, object>(SettingName, new ConcurrentQueue<string>(File.ReadAllLines(SettingValue)))); }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(SettingValue))
                        {
                            bool DoesntContainEscapeString = !(SettingValue[0] == '`' && SettingValue[1] == '`'
                                && SettingValue[SettingValue.Length - 1] == '`'
                                && SettingValue[SettingValue.Length - 2] == '`');

                            if (DoesntContainEscapeString)
                            {
                                // Temporarily use these variables (used for interpretedlines)
                                // To modify/change settings based on the same rules.
                                // CurrentActionValue = NEW Setting Value
                                CurrentActionName = SettingName;
                                CurrentActionValue = SettingValue;

                                var EmptyList = new List<Tuple<string, object>>();
                                EmptyList = await ReplacementManager.ReplaceActionValueAddSettingAsync(EmptyList, this);
                                SettingValue = CurrentActionValue;

                                // No settings or variables replaced.
                                if (CurrentActionValue == SettingValue)
                                    ReplacementManager._replaceDynamics.ReplaceUrlWithValue();
                            }
                            else
                            {
                                if (Reset)
                                    CurrentActionValue = SettingValue.Substring(2, SettingValue.Length - 4);
                            }
                        }

                        Settings.Add(new Tuple<string, object>(CurrentActionName, CurrentActionValue));
                    }
                }
            }
        }

        /// <summary>
        /// Sets 
        /// </summary>
        /// <param name="LinkedProjects"></param>
        public void SetLinkedProjects(List<InterpretedProject> LinkedProjects)
        {
            foreach (var proj in LinkedProjects)
            {
                // Won't add aggregate settings yet.
                // So we can add pagination to MainWindow.
                // Once all textboxes filled by the user on the app
                // and he clicks run project, then we create settings.
                ProjectLinks.Add(proj);

                // Add lines of linked project to end of current.
                InterpretingLines.AddRange(proj.InterpretingLines);
            }
        }

        private string SentenceCase(string input)
        {
            if (input.Length < 1)
                return input;

            string sentence = input.ToLower();
            return sentence[0].ToString().ToUpper() +
               sentence.Substring(1);
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public List<InterpretedProject> ProjectLinks = new List<InterpretedProject>();

        public AutomationType AType = AutomationType.Pupeteer;
        public ReplacementManager ReplacementManager { get; set; }
        public List<Tuple<string, object>> Settings = new List<Tuple<string, object>>();
        public List<HttpRequest> Requests = new List<HttpRequest>();
        public List<string> InterpretingLines { get; set; }
        public int CurrentLineIndex { get; set; }
        public string CurrentLine { get; set; }
        public string CurrentActionName { get; set; }
        public string CurrentActionValue { get; set; }

        public int ThreadCount { get; set; } = 1;
        public int IterationCount { get; set; } = int.MaxValue;
        public ConcurrentQueue<string> Proxies = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> UserAgents = new ConcurrentQueue<string>();
        public bool ShowBrowser = true;
        public bool ShowImages = true;
        public bool SocksProxy = false;
        public string Cookies { get; set; }
        public string LastURLValue { get; set; }
        public int InternalLoopCount { get; set; }
        public int InternalLoopTimeout { get; set; }
        public bool Loop { get; set; }
        public int LoopIndex { get; set; }
        public List<string> LinkedProjectList { get; set; } = new List<string>();

        /// <summary>
        /// If loop fails, do not throw/crash the program
        /// </summary>
        public bool TryLoop { get; set; }
    }

    public enum AutomationType
    {
        Selenium,
        Pupeteer
    }
}
