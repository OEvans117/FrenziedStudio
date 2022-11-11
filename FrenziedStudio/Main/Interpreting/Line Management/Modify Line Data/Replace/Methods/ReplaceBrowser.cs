using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ReplaceBrowser
    {
        public InterpretedProject _project { get; set; }
        public Automation _automationType { get; set; }

        public ReplaceBrowser(InterpretedProject _project, Automation _automationType)
        {
            this._project = _project;
            this._automationType = _automationType;
        }

        /// <summary>
        /// Used by Replace method classes to identify method patterns
        /// </summary>
        private Random MethodRandom = new Random();
        /// <summary>
        /// Replaces regex method pattern with name of method
        /// </summary>
        /// <returns>
        /// Returns whether CurrentActionValue contained method
        /// </returns>
        private string ReplaceMethodPattern(string MethodName)
        {
            string MethodPattern = @"{{(.*?)->(.*?)}}";

            string MethodNamePattern = "{{" + MethodName + "->";

            bool ContainsMethod = _project.CurrentActionValue.Contains(MethodNamePattern);

            if (ContainsMethod)
            {
                MethodPattern = MethodPattern.Replace("{{(.*?)->", MethodNamePattern);

                return MethodPattern;
            }
            else
                return string.Empty;
        }

        public async Task ReplacePageURL()
        {
            if (_project.CurrentActionValue.Contains("{{GetPageURL}}"))
                _project.CurrentActionValue = _project.CurrentActionValue.Replace("{{GetPageURL}}", await _automationType.GetPageUrl());
        }

        public async Task ReplacePageHTML()
        {
            if (_project.CurrentActionValue.Contains("{{GetPageHTML}}"))
            {
                var pageHtml = await _automationType.GetPageHtml();
                _project.CurrentActionValue = _project.CurrentActionValue.Replace("{{GetPageHTML}}", pageHtml);
            }
        }

        public async Task ReplaceText()
        {
            string MethodPattern = ReplaceMethodPattern("GetText");

            if (!string.IsNullOrEmpty(MethodPattern))
            {
                string[] Identifier = Regex.Split(Regex.Match(_project.CurrentActionValue, MethodPattern).Groups[1].Value, "->");
                string TextValue = await _automationType.GetText(Identifier[0], Identifier[1]);
                _project.CurrentActionValue = Regex.Replace(_project.CurrentActionValue, MethodPattern, TextValue);
            }
        }

        public async Task ReplaceElementCount()
        {
            string MethodPattern = ReplaceMethodPattern("GetElementCount");

            if (!string.IsNullOrEmpty(MethodPattern))
            {
                string[] Identifier = Regex.Split(Regex.Match(_project.CurrentActionValue, MethodPattern).Groups[1].Value, "->");
                string ElementCount = await _automationType.GetElementCount(Identifier[0], Identifier[1]);
                _project.CurrentActionValue = Regex.Replace(_project.CurrentActionValue, MethodPattern, ElementCount);
            }
        }

        public async Task ReplaceAttribute()
        {
            if (_project.CurrentActionValue.Contains("{{GetAttribute:"))
            {
                string GetAttributePattern = @"{{(GetAttribute:(.*?)->(.*?)->(.*?))}}";
                string AttributeName = Regex.Match(_project.CurrentActionValue, GetAttributePattern).Groups[1].Value;
                string ElementType = Regex.Match(_project.CurrentActionValue, GetAttributePattern).Groups[2].Value;
                string ElementValue = Regex.Match(_project.CurrentActionValue, GetAttributePattern).Groups[3].Value;
                string Attribute = await _automationType.GetAttribute(AttributeName, ElementType, ElementValue);
                _project.CurrentActionValue = Regex.Replace(_project.CurrentActionValue, GetAttributePattern, Attribute);
            }
        }



    }
}
