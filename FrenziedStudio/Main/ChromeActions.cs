using FrenziedStudio.Main;
using MiscUtil;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;

namespace FrenziedStudio.Interpretation
{
    public static class ChromeActions
    {
        /// <summary>
        /// Used by SeleniumAutomation to fill in text slowly at random times
        /// </summary>
        public static void FillSlowly(IWebElement elem, string text, int minrnd, int maxrnd, ChromeDriver cd)
        {
            FocusElement(elem, cd);

            foreach (char c in text)
            {
                elem.SendKeys(c.ToString());
                Thread.Sleep(StaticRandom.Next(minrnd, maxrnd));
            }
        }

        /// <summary>
        /// Used by SeleniumAutomation to focus the mouse over an element
        /// </summary>
        private static void FocusElement(IWebElement element, ChromeDriver cd)
        {
            Actions action = new Actions(cd);
            action.MoveToElement(element).Perform();
        }

        /// <summary>
        /// Create temporary random folder to store extension once in the %temp% folder
        /// on windows, which is deleted once used. ONLY used for IP:PORT:USER:PASS proxies
        /// </summary>
        /// <returns>Returns path to RandomDirectory storing chrome extension</returns>
        public static string CreateProxyExtension(string Proxy)
        {
            // Create temporary random folder to store extension once.
            string RandomDirectory = Path.GetTempPath() + "extensiony" + StaticRandom.Next(9999, 99999).ToString();
            Directory.CreateDirectory(RandomDirectory);

            string background = "var config = { mode: \"fixed_servers\", rules: { singleProxy: { scheme: \"http\", host: \"IPGOESHERE\", port: parseInt(PORTGOESHERE) },bypassList: [\"localhost\"]}}; chrome.proxy.settings.set({value: config, scope: \"regular\"}, function() {});function callbackFn(details) { return { authCredentials: { username: \"USERGOESHERE\", password: \"PASSGOESHERE\" } };} chrome.webRequest.onAuthRequired.addListener(callbackFn, {urls: [\"<all_urls>\"]}, ['blocking']);";
            background = background.Replace("IPGOESHERE", Proxy.Split(':')[0]);
            background = background.Replace("PORTGOESHERE", Proxy.Split(':')[1]);
            background = background.Replace("USERGOESHERE", Proxy.Split(':')[2]);
            background = background.Replace("PASSGOESHERE", Proxy.Split(':')[3]);
            string manifest = "{\"background\":{\"scripts\":[\"background.js\"]},\"manifest_version\":2,\"minimum_chrome_version\":\"22.0.0\",\"name\":\"Gmail\",\"permissions\":[\"proxy\",\"tabs\",\"unlimitedStorage\",\"storage\",\"<all_urls>\",\"webRequest\",\"webRequestBlocking\"],\"version\":\"1.0.0\"}";

            File.WriteAllText(RandomDirectory + "/background.js", background);
            File.WriteAllText(RandomDirectory + "/manifest.json", manifest);

            return RandomDirectory;
        }

        /// <summary>
        /// Get the type of proxy depending on the amounts of columns
        /// ip:port = normalproxy
        /// ip:port:user:pass = userpassproxy
        public static ProxyType GetProxyType(string Proxy)
        {
            if (string.IsNullOrEmpty(Proxy)) { return ProxyType.NoProxy; }

            int ProxyColonLength = Proxy.Split(':').Length;

            switch(ProxyColonLength - 1)
            {
                case 0:
                    return ProxyType.NoProxy;
                case 1:
                    return ProxyType.NormalProxy;
                case 3:
                    return ProxyType.UserPassProxy;
                default: return ProxyType.NoProxy;
            }
        }

        /// <summary>
        /// Return chrome options for stealth in Selenium
        /// (proxy, infobars, language, maximization)
        /// </summary>
        public static ChromeOptions StaticChromeOptions(InterpretedProject pr)
        {
            Environment.SetEnvironmentVariable("webdriver.chrome.driver", @"C:\Users\oscar\AppData\Roaming\Incogniton\Incogniton\browser\99\win\chromedriver.exe");

            ChromeOptions options = new ChromeOptions();

            if (!pr.ShowImages) { options.AddUserProfilePreference("profile.default_content_setting_values.images", 2); }
            if (!pr.ShowBrowser) { options.AddArgument("--window-position=-32000,-32000"); }
            //options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 2);

            //if(pr.UserAgents.TryDequeue(out string UserAgent))
            //    options.AddArgument("--user-agent=" + UserAgent);

            options.BinaryLocation = @"C:\Users\oscar\AppData\Roaming\Incogniton\Incogniton\browser\99\win\chrome.exe";
            //options.AddArgument("--disable-infobars");
            //options.AddArgument("ignore-certificate-errors");
            //options.AddArgument("--disable-bundled-ppapi-flash");
            //options.AddArgument("--disable-webgl");
            //options.AddArgument("--disable-single-click-autofill");
            //options.AddArgument("--lang=en-US");
            //options.AddArgument("--start-maximized");
            //options.AddArgument("no-sandbox");
            //options.AddExtension("Extensions/cy.crx");
            //options.AddExtension("Extensions/cd.crx");
            //options.AddExtension("Extensions/hr.crx");
            //options.AddArgument("--disable-gpu");

            //string[] ExcludedArguments = { "ignore-certificate-errors", "safebrowsing-disable-download-protection",
            //    "safebrowsing-disable-auto-update", "disable-client-side-phishing-detection", "enable-automation" };
            //options.AddExcludedArguments(ExcludedArguments);

            if (pr.Proxies.TryDequeue(out string Proxy))
            {
                var proxyType = GetProxyType(Proxy);

                if (proxyType == ProxyType.NoProxy)
                    return options;

                if (pr.SocksProxy) { 
                    options.AddArguments("--proxy-server=socks5://" + Proxy); return options; }

                if (proxyType == ProxyType.UserPassProxy)
                    options.AddArgument("--load-extension=" + CreateProxyExtension(Proxy));

                else if (proxyType == ProxyType.NormalProxy)
                {
                    Proxy instaProxy = new Proxy { 
                        Kind = ProxyKind.Manual, 
                        IsAutoDetect = false, 
                        HttpProxy = Proxy, 
                        SslProxy = Proxy };

                    options.Proxy = instaProxy;
                }
            }

            return options;
        }
        public static ChromeDriverService StaticChromeDriverService()
        {
            ChromeDriverService chromeDriverDirectory = ChromeDriverService.CreateDefaultService();
            chromeDriverDirectory.HideCommandPromptWindow = true;
            return chromeDriverDirectory;
        }

        /// <summary>
        /// Quit the current interpretlines loop with a message
        /// </summary>
        public static void QuitMessage(string message, bool TryWhileOn)
        {
            if (!TryWhileOn)
                throw new Exception(message);
        }
    }

    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(ExpectedConditions.ElementToBeClickable(by));
            }
            return driver.FindElement(by);
        }
    }

    public enum ProxyType
    {
        NoProxy,
        NormalProxy,
        UserPassProxy
    }
}
