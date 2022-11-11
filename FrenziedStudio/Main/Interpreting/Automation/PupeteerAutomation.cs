using FrenziedStudio.Interpretation;
using MiscUtil;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    class PupeteerAutomation : Automation
    {
        private Browser _pupeteerBrowser { get; set; }
        private Page _pupeteerPage { get; set; }
        private Frame _pupeteerFrame { get; set; }

        private ReplacementManager _replacementManager { get; set; }

        public PupeteerAutomation(InterpretedProject pr, ReplacementManager rm) : base(pr, rm)
        {
            _replacementManager = rm;
        }

        #region Pup Actions

        public async override Task Start()
        {
            var extra = new PuppeteerExtra();

            // Use stealth plugin
            extra.Use(new StealthPlugin());

            // Import proxies into browser
            string[] proxyArgs = { "" };
            _project.Proxies.TryDequeue(out string Proxy);
            var proxyType = ChromeActions.GetProxyType(Proxy);

            if (proxyType == ProxyType.NormalProxy)
                proxyArgs[0] = string.Format("--proxy-server={0}:{1}", Proxy.Split(':')[0], Proxy.Split(':')[1]);
            if(proxyType == ProxyType.UserPassProxy)
            {
                string proxyExtensionPath = ChromeActions.CreateProxyExtension(Proxy);
                proxyArgs[0] = "--load-extension=" + proxyExtensionPath;
                proxyArgs = proxyArgs.Append("--disable-extensions-except=" + proxyExtensionPath).ToArray();
            }

            // Launch the puppeteer browser with plugins
            _pupeteerBrowser = await extra.LaunchAsync(new LaunchOptions()
            {
                Headless = false,
                DefaultViewport = null,
                Args = proxyArgs
            });

            _pupeteerPage = (await _pupeteerBrowser.PagesAsync()).First();

            // Import user agents into browser
            if (_project.UserAgents.TryDequeue(out string UserAgent))
                await _pupeteerPage.SetUserAgentAsync(UserAgent);
        }

        public async override Task Quit()
        {
            if (_pupeteerBrowser != null)
            {
                await _pupeteerBrowser.CloseAsync();
            }
        }

        public async override Task Goto()
        {
            await _pupeteerPage.GoToAsync(_project.CurrentActionValue);
        }

        public async override Task WaitElement()
        {
            WaitForSelectorOptions wfso = new WaitForSelectorOptions()
            {
                Timeout = Convert.ToInt32(ThirdParameter) * 1000
            };

            switch (FirstParameter)
            {
                case "xpath":
                    await _pupeteerPage.WaitForXPathAsync(SecondParameter, wfso);
                    break;

                case "id":
                    await _pupeteerPage.WaitForXPathAsync(ConvertToXPath.Convert(SecondParameter, ECT.id), wfso);
                    break;

                case "name":
                    await _pupeteerPage.WaitForXPathAsync(ConvertToXPath.Convert(SecondParameter, ECT.name), wfso);
                    break;

                case "classname":
                    await _pupeteerPage.WaitForXPathAsync(ConvertToXPath.Convert(SecondParameter, ECT.classname), wfso);
                    break;

                case "cssselector":
                    await _pupeteerPage.WaitForSelectorAsync(SecondParameter, wfso);
                    break;

                case "linktext":
                    await _pupeteerPage.WaitForXPathAsync(ConvertToXPath.Convert(SecondParameter, ECT.linktext), wfso);
                    break;
            }
        }

        public async override Task SendKeys()
        {
            base.SendKeys();

            _ = (await GetElementFromIdentifier()).TypeAsync(ThirdParameter);
        }

        public async override Task TypeSlow()
        {
            base.TypeSlow();

            int minRnd = Convert.ToInt32(FourthParameter.Split(',')[0]);
            int maxRnd = Convert.ToInt32(FourthParameter.Split(',')[1]);

            foreach (char c in SecondParameter)
            {
                _ = (await GetElementFromIdentifier()).TypeAsync(c.ToString());
                Thread.Sleep(StaticRandom.Next(minRnd, maxRnd));
            }
        }

        public async override Task Click()
        {
            var element = await GetElementFromIdentifier();
            _ = element.ClickAsync();
        }

        public async override Task Scroll()
        {
            var element = await GetElementFromIdentifier();
            await _pupeteerPage.EvaluateFunctionAsync("arguments[0].scrollIntoViewIfNeeded({behavior: \"smooth\"})", element);
        }

        public async override Task Screenshot()
        {
            await _pupeteerPage.ScreenshotAsync(_project.CurrentActionValue);
        }

        public async override Task Javascript()
        {
            await _pupeteerPage.EvaluateExpressionAsync(_project.CurrentActionValue);
        }

        public async override Task Switchframe()
        {
            int SwitchFrameNumber;

            bool isNumeric = int.TryParse(FirstParameter, out SwitchFrameNumber);

            if (isNumeric)
            {
                _pupeteerFrame = _pupeteerPage.Frames[SwitchFrameNumber];
            }
            else
            {
                if (FirstParameter == "parent")
                {
                    _pupeteerFrame = _pupeteerPage.MainFrame;
                }
                else
                {
                    foreach(var frame in _pupeteerPage.Frames)
                    {
                        Enum.TryParse(FirstParameter, out ECT elementType);

                        var element = await frame.XPathAsync(ConvertToXPath.Convert(SecondParameter, elementType));
                        
                        if(element.Length > 0)
                        {
                            _pupeteerFrame = frame;

                            break;
                        }
                    }
                }
            }
        }

        public async override Task TypeEnter()
        {
            await _pupeteerPage.Keyboard.PressAsync("Enter");
        }

        public async override Task Select()
        {
            _ = (await GetElementFromIdentifier()).SelectAsync(FourthParameter);
        }

        public async override Task SetCookies()
        {
            string cookieCombo = string.Empty;

            var cookies = await _pupeteerPage.GetCookiesAsync(new string[] { _pupeteerPage.Url });

            foreach (var c in cookies)
            {
                if (c == cookies.First()) { cookieCombo += c.Name + ":" + c.Value; }
                else { cookieCombo += "|" + c.Name + ":" + c.Value; }
            }

            _project.Cookies = cookieCombo;
        }

        public async override Task ElementLoop(int index, bool Try)
        {
            int IterationCount = (await GetElementsFromIdentifier()).Count();

            if (IterationCount > 0)
            {
                _project.InternalLoopTimeout = IterationCount;
                _project.Loop = true;
                _project.LoopIndex = index;

                if (Try)
                    _project.TryLoop = true;
            }
            else
            {
                _project.Loop = false;

                if (Try)
                    _project.TryLoop = false;
            }
        }

        #endregion

        #region Pupeteer Data

        private async Task<ElementHandle> GetElement(string ElementType, string ElementValue)
        {
            Enum.TryParse(ElementType, out ECT elementType);

            if (_pupeteerFrame == null)
                return (await _pupeteerPage.XPathAsync(ConvertToXPath.Convert(ElementValue, elementType)))[0];
            else
                return (await _pupeteerFrame.XPathAsync(ConvertToXPath.Convert(ElementValue, elementType)))[0];
        }

        private async Task<ElementHandle> GetElementFromIdentifier()
        {
            int ElementToBeSelected = ReplaceReturnElemIdentifierIndex();

            Enum.TryParse(FirstParameter, out ECT elementType);

            if(_pupeteerFrame == null)
                return (await _pupeteerPage.MainFrame.XPathAsync(ConvertToXPath.Convert(SecondParameter, elementType)))[ElementToBeSelected];
            else
                return (await _pupeteerFrame.XPathAsync(ConvertToXPath.Convert(SecondParameter, elementType)))[ElementToBeSelected];
        }

        private async Task<ElementHandle[]> GetElementsFromIdentifier()
        {
            Enum.TryParse(FirstParameter, out ECT elementType);

            if (_pupeteerFrame == null)
                return (await _pupeteerPage.XPathAsync(ConvertToXPath.Convert(SecondParameter, elementType)));
            else
                return (await _pupeteerFrame.XPathAsync(ConvertToXPath.Convert(SecondParameter, elementType)));
        }

        public override async Task<string> GetPageUrl()
        {
            return _pupeteerPage.Url;
        }

        public override async Task<string> GetPageHtml()
        {
            return await (_pupeteerPage.GetContentAsync());
        }

        public override async Task<string> GetText(string ElementName, string ElementValue)
        {
            return await (GetElement(ElementName, ElementValue)).EvaluateFunctionAsync<string>("node => node.textContent");
        }

        public override async Task<string> GetElementCount(string ElementName, string ElementValue)
        {
            return (await GetElementsFromIdentifier()).Length.ToString();
        }

        public override async Task<string> GetAttribute(string AttributeName, string ElementName, string ElementValue)
        {
            return await (GetElement(ElementName, ElementValue)).EvaluateFunctionAsync<string>("node => node." + AttributeName);
        }

        #endregion
    }
}
