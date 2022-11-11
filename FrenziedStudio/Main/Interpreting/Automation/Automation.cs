using MiscUtil;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ExternalSel.TextWriter;

namespace FrenziedStudio.Main
{
    public abstract class Automation
    {
        public InterpretedProject _project { get; set; }
        private ReplacementManager _replacementManager { get; set; }

        public string FirstParameter { get; set; }
        public string SecondParameter { get; set; }
        public string ThirdParameter { get; set; }
        public string FourthParameter { get; set; }
        public int ParameterCount { get; set; }

        public Automation(InterpretedProject pr, ReplacementManager rm)
        {
            _project = pr;
            _replacementManager = rm;

            SetAVParameters();
        }

        /// <summary>
        /// Set the ActionValue parameters used for all Interpretation
        /// methods, such as goto, sendkeys, waitelement etc
        /// </summary>
        public void SetAVParameters()
        {
            string[] Parameters = Regex.Split(_project.CurrentActionValue, "->");

            // If parameters.length == 0, just use _project.CurrentActionValue.

            if(Parameters.Length == 1)
            {
                FirstParameter = _project.CurrentActionValue;
            }

            if (Parameters.Length == 2)
            {
                FirstParameter = Parameters[0];
                SecondParameter = Parameters[1];
            }

            if (Parameters.Length == 3)
            {
                FirstParameter = Parameters[0];
                SecondParameter = Parameters[1];
                ThirdParameter = Parameters[2];
            }

            if (Parameters.Length == 4)
            {
                FirstParameter = Parameters[0];
                SecondParameter = Parameters[1];
                ThirdParameter = Parameters[2];
                FourthParameter = Parameters[3];
            }

            ParameterCount = Parameters.Length;
        }

        /// <summary>
        /// Show message modal
        /// </summary>
        public void MessageModal()
        {
            MessageBox.Show(FirstParameter);
        }

        /// <summary>
        /// Write line of text to .txt file
        /// </summary>
        public void WriteLine()
        {
            LoggingExtensions.WriteDebug(SecondParameter, FirstParameter);
        }

        /// <summary>
        /// Replace text in .txt file
        /// </summary>
        public void Write()
        {
            LoggingExtensions.ReWriteDebug(SecondParameter, FirstParameter);
        }

        /// <summary>
        /// Start the browser service
        /// </summary>
        public virtual async Task Start(){ }

        /// <summary>
        /// Quit the browser service
        /// </summary>
        public virtual async Task Quit() { }

        /// <summary>
        /// Visit website (abstract so must be overriden)
        /// </summary>
        public virtual async Task Goto() { }

        /// <summary>
        /// Wait for element to load
        /// </summary>
        public virtual async Task WaitElement() { }

        /// <summary>
        /// Wait a random time
        /// </summary>
        public virtual void RndWait()
        {
            Thread.Sleep(StaticRandom.Next(Convert.ToInt32(FirstParameter.Split(',')[0]), Convert.ToInt32(FirstParameter.Split(',')[1])));
        }

        /// <summary>
        /// Wait a time
        /// </summary>
        public virtual void Wait()
        {
            Thread.Sleep(Convert.ToInt32(_project.CurrentActionValue));
        }

        /// <summary>
        /// Send keys to a textarea/input
        /// </summary>
        public virtual async Task SendKeys() {
            _replacementManager._replaceDynamics.ReplaceUrlWithValue();
        }

        /// <summary>
        /// Type message slowly
        /// </summary>
        public virtual async Task TypeSlow() { _replacementManager._replaceDynamics.ReplaceUrlWithValue(); }

        /// <summary>
        /// Click element
        /// </summary>
        public virtual async Task Click() { }

        /// <summary>
        /// Scroll to element
        /// </summary>
        public virtual async Task Scroll() { }

        /// <summary>
        /// Screenshot webpage
        /// </summary>
        public virtual async Task Screenshot() { }

        /// <summary>
        /// Run javascript
        /// </summary>
        public virtual async Task Javascript() { }

        /// <summary>
        /// Switch current page frame
        /// Sometimes needed to click items
        /// </summary>
        public virtual async Task Switchframe() { }

        /// <summary>
        /// Press enter on keyboard
        /// </summary>
        public virtual async Task TypeEnter() { }

        /// <summary>
        /// Select element from <select></select>
        /// </summary>
        public virtual async Task Select() { }

        /// <summary>
        /// Set the cookies.
        /// Format is returned from {{Cookies}} in ReplaceSettingWithValue
        /// </summary>
        public virtual async Task SetCookies() { }

        /// <summary>
        /// Loop over elements
        /// </summary>
        public virtual async Task ElementLoop(int index, bool Try) { }

        /// <summary>
        /// Acquire value of whatever the identifier is.
        /// ONLY used for IF, WHILE statements etc
        /// If the identifier is HTML, acquire that value
        /// If the identifier is text, get the text of the proceding element
        /// Examples:
        /// attribute:class->xpath->test
        /// elemcount->xpath->test
        /// text->xpath->test
        /// </summary>
        /// <param name="ActionValue"></param>
        /// <returns></returns>
        public virtual string GetValueFromIdentifier(string ActionValue = "") { return string.Empty; }

        /// <summary>
        /// Get HTTP elements from an ActionValue string
        /// Identifier = xpath->test
        /// ("xpath->test" returns the element object)
        /// Convention is Identifier->IdentifierValue throughout FrenziedLang
        /// </summary>
        /// <param name="MultipleElements"></param>
        /// <param name="ElementType"></param>
        /// <param name="ElementValue"></param>
        /// <returns></returns>
        public virtual object GetElementsFromIdentifier(bool MultipleElements, string ElementType = "", string ElementValue = "") { return null; }

        /// <summary>
        /// Return URL of current page
        /// </summary>
        /// <returns></returns>
        public virtual async Task<string> GetPageUrl() { return string.Empty; }

        /// <summary>
        /// Return HTML of current page
        /// </summary>
        /// <returns></returns>
        public virtual async Task<string> GetPageHtml() { return string.Empty; }

        /// <summary>
        /// Return text value of a HTML element
        /// </summary>
        public virtual async Task<string> GetText(string ElementType, string ElementValue) { return string.Empty; }

        /// <summary>
        /// Return amount of elements with value
        /// </summary>
        public virtual async Task<string> GetElementCount(string ElementType, string ElementValue) { return string.Empty; }

        /// <summary>
        /// Get the attribute that a HTML element holds
        /// </summary>
        public virtual async Task<string> GetAttribute(string AttributeName, string ElementType, string ElementValue) { return string.Empty; }

        /// <summary>
        /// If an ElementIdentifier contains an index (format = sendkeys=classname->test[2])
        /// then extract the index of the element & modify the actionvalue to remove it
        /// </summary>
        /// <returns></returns>
        public int ReplaceReturnElemIdentifierIndex()
        {
            bool SelectElementFromList = Regex.IsMatch(_project.CurrentActionValue, @"(?:.*?)->.*?(\[(\d+)\])");

            if (!SelectElementFromList)
                return 0;

            var regexMatches = Regex.Match(_project.CurrentActionValue, @"(?:.*?)->.*?(\[(\d+)\])");
            SecondParameter = SecondParameter.Replace(regexMatches.Groups[1].Value, string.Empty);
            return Convert.ToInt32(regexMatches.Groups[2].Value);
        }
    }
}
