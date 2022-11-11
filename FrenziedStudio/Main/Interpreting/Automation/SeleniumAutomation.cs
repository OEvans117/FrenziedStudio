using FrenziedStudio.Interpretation;
using MiscUtil;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ExternalSel.TextWriter;

namespace FrenziedStudio.Main
{
    class SeleniumAutomation : Automation
    {
        public ChromeDriver _chromeDriver { get; set; }

        private ReplacementManager _replacementManager { get; set; }

        public SeleniumAutomation(InterpretedProject pr, ReplacementManager rm) : base(pr, rm)
        {

        }

        #region Sel Actions

        public async override Task Start()
        {
            if (_project.CurrentActionValue == "start")
            {
                _chromeDriver = new ChromeDriver(ChromeActions.StaticChromeDriverService(), ChromeActions.StaticChromeOptions(_project));
            }
        }

        public async override Task Quit()
        {
            if(_chromeDriver != null)
            {
                _chromeDriver.Quit();
                _chromeDriver.Dispose();
            }
        }

        public async override Task Goto()
        {
            _chromeDriver.Navigate().GoToUrl(_project.CurrentActionValue);
        }

        public async override Task WaitElement()
        {
            switch (FirstParameter)
            {
                case "xpath":
                    _chromeDriver.FindElement(By.XPath(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;

                case "id":
                    _chromeDriver.FindElement(By.Id(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;

                case "name":
                    _chromeDriver.FindElement(By.Name(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;

                case "classname":
                    _chromeDriver.FindElement(By.ClassName(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;

                case "cssselector":
                    _chromeDriver.FindElement(By.CssSelector(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;

                case "linktext":
                    _chromeDriver.FindElement(By.LinkText(SecondParameter), Convert.ToInt32(ThirdParameter));
                    break;
            }
        }

        public async override Task SendKeys()
        {
            base.SendKeys();

            ((IWebElement)GetElementsFromIdentifier(false)).SendKeys(Regex.Split(_project.CurrentActionValue, "->")[2]);
        }

        public async override Task TypeSlow()
        {
            base.TypeSlow();
            
            var element = ((IWebElement)GetElementsFromIdentifier(false));

            int minRnd = Convert.ToInt32(FourthParameter.Split(',')[0]);
            int maxRnd = Convert.ToInt32(FourthParameter.Split(',')[1]);

            ChromeActions.FillSlowly(element, ThirdParameter, minRnd, maxRnd, _chromeDriver);
        }

        public async override Task Click()
        {
            ((IWebElement)GetElementsFromIdentifier(false)).Click();
        }

        public async override Task Scroll()
        {
            var element = ((IWebElement)GetElementsFromIdentifier(false));
            _chromeDriver.ExecuteJavaScript<string>("arguments[0].scrollIntoViewIfNeeded({behavior: \"smooth\"})", element);
        }

        public async override Task Screenshot()
        {
            Screenshot ss = ((ITakesScreenshot)_chromeDriver).GetScreenshot();
            ss.SaveAsFile(_project.CurrentActionValue, ScreenshotImageFormat.Png);
        }

        public async override Task Javascript()
        {
            _chromeDriver.ExecuteJavaScript<string>(_project.CurrentActionValue);
        }

        public async override Task Switchframe()
        {
            int SwitchFrameNumber;

            bool isNumeric = int.TryParse(FirstParameter, out SwitchFrameNumber);

            if (isNumeric)
            {
                _chromeDriver.SwitchTo().Frame(SwitchFrameNumber);
            }
            else
            {
                if(FirstParameter == "parent")
                {
                    _chromeDriver.SwitchTo().ParentFrame();
                }
                else
                {
                    _chromeDriver.SwitchTo().Frame(((IWebElement)GetElementsFromIdentifier(false)));
                }
            }
        }

        public async override Task TypeEnter()
        {
            Actions builder = new Actions(_chromeDriver);
            builder.SendKeys(Keys.Enter);
        }

        public async override Task Select()
        {
            SelectElement elem = new SelectElement(((IWebElement)GetElementsFromIdentifier(false)));

            switch (ThirdParameter)
            {
                case "index":
                    elem.SelectByIndex(Convert.ToInt32(FourthParameter));
                    break;

                case "text":
                    elem.SelectByText(FourthParameter);
                    break;

                case "value":
                    elem.SelectByValue(FourthParameter);
                    break;
            }
        }

        public async override Task SetCookies()
        {
            string cookieCombo = string.Empty;
            var cookies = _chromeDriver.Manage().Cookies.AllCookies;

            foreach (Cookie c in cookies)
            {
                if (c == cookies.First()) { cookieCombo += c.Name + ":" + c.Value; }
                else { cookieCombo += "|" + c.Name + ":" + c.Value; }
            }

            cookieCombo = cookieCombo.Remove(cookieCombo.Length - 1);
            cookieCombo = Regex.Replace(cookieCombo, @"\t|\n|\r", "");

            _project.Cookies = cookieCombo;
        }

        public async override Task ElementLoop(int index, bool Try)
        {
            int IterationCount = ((IReadOnlyCollection<IWebElement>)GetElementsFromIdentifier(true)).Count;

            if (IterationCount > 0)
            {
                _project.InternalLoopTimeout = IterationCount;
                _project.Loop = true;
                _project.LoopIndex = index;

                if(Try)
                    _project.TryLoop = true;
            }
            else
            {
                _project.Loop = false;

                if(Try)
                    _project.TryLoop = false;
            }
        }

        #endregion

        #region Sel Data

        /// <param name="MultipleElements">If true, returns many elements (elemloop/tryelemloop)</param>
        /// <returns></returns>
        public override object GetElementsFromIdentifier(bool MultipleElements, string ElementType = "", string ElementValue = "")
        {
            // These two strings are used for GetIdentifierValue, where the Identifier
            // Is equal to something like:
            // attribute:class->xpath->test
            // elemcount->xpath->test
            // text->xpath->test
            if (!string.IsNullOrEmpty(ElementType)) { FirstParameter = ElementType; }
            if (!string.IsNullOrEmpty(ElementValue)) { SecondParameter = ElementValue; }

            // If elementvalue contains "[x]" then select the appropriate element
            // This means user wants to click on x variable in a list of elements
            // For example, this can be used for elemloops.
            int ElementToBeSelected = ReplaceReturnElemIdentifierIndex();
            bool SelectElementFromList = ElementToBeSelected != 0;

            if (FirstParameter == "xpath")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.XPath(SecondParameter)); }
                if (SelectElementFromList) { return _chromeDriver.FindElements(By.XPath(SecondParameter))[ElementToBeSelected]; }
                else { return _chromeDriver.FindElement(By.XPath(SecondParameter)); }
            }
            else if (FirstParameter == "id")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.Id(SecondParameter)); }
                else { return _chromeDriver.FindElement(By.Id(SecondParameter)); }
            }
            else if (FirstParameter == "name")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.Name(SecondParameter)); }
                if (SelectElementFromList) { return _chromeDriver.FindElements(By.Name(SecondParameter))[ElementToBeSelected]; }
                else { return _chromeDriver.FindElement(By.Name(SecondParameter)); }
            }
            else if (FirstParameter == "classname")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.ClassName(SecondParameter)); }
                if (SelectElementFromList) { return _chromeDriver.FindElements(By.ClassName(SecondParameter))[ElementToBeSelected]; }
                else { return _chromeDriver.FindElement(By.ClassName(SecondParameter)); }
            }
            else if (FirstParameter == "cssselector")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.CssSelector(SecondParameter)); }
                if (SelectElementFromList) { return _chromeDriver.FindElements(By.CssSelector(SecondParameter))[ElementToBeSelected]; }
                else { return _chromeDriver.FindElement(By.CssSelector(SecondParameter)); }
            }
            else if (FirstParameter == "linktext")
            {
                if (MultipleElements) { return _chromeDriver.FindElements(By.LinkText(SecondParameter)); }
                else { return _chromeDriver.FindElement(By.LinkText(SecondParameter)); }
            }

            return null;
        }

        public override async Task<string> GetPageUrl()
        {
            return _chromeDriver.Url;
        }

        public override async Task<string> GetPageHtml()
        {
            return _chromeDriver.PageSource;
        }

        public override async Task<string> GetText(string ElementName, string ElementValue)
        {
            return ((IWebElement)GetElementsFromIdentifier(false, ElementName, ElementValue)).Text;
        }

        public override async Task<string> GetElementCount(string ElementName, string ElementValue)
        {
            return ((IReadOnlyCollection<IWebElement>)GetElementsFromIdentifier(true, ElementName, ElementValue)).Count.ToString();
        }

        public override async Task<string> GetAttribute(string AttributeName, string ElementName, string ElementValue)
        {
            return ((IWebElement)GetElementsFromIdentifier(false, ElementName, ElementValue)).GetAttribute(AttributeName);
        }

        #endregion
    }
}
