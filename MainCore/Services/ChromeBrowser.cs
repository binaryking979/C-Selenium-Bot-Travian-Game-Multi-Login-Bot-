﻿using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Errors;
using MainCore.Common.Models;
using MainCore.Infrasturecture.AutoRegisterDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Support.UI;
using Polly;

namespace MainCore.Services
{
    [DoNotAutoRegister]
    public sealed class ChromeBrowser : IChromeBrowser
    {
        private ChromeDriver _driver;
        private readonly ChromeDriverService _chromeService;
        private WebDriverWait _wait;

        private readonly string[] _extensionsPath;
        private readonly HtmlDocument _htmlDoc = new();

        public string EndpointAddress
        {
            get
            {
                if (_driver is null) return "";
                return _driver.GetDevToolsSession().EndpointAddress;
            }
        }

        public ChromeBrowser(string[] extensionsPath)
        {
            _extensionsPath = extensionsPath;

            _chromeService = ChromeDriverService.CreateDefaultService();
            _chromeService.HideCommandPromptWindow = true;
        }

        public async Task<Result> Setup(ChromeSetting setting)
        {
            var options = new ChromeOptions();
            
            options.AddExtensions(_extensionsPath);
            /*
      
            if (!string.IsNullOrEmpty(setting.ProxyHost))
            {
                if (!string.IsNullOrEmpty(setting.ProxyUsername))
                {
                    options.AddHttpProxy(setting.ProxyHost, setting.ProxyPort, setting.ProxyUsername, setting.ProxyPassword);
                }
                else
                {
                    options.AddArgument($"--proxy-server={setting.ProxyHost}:{setting.ProxyPort}");
                }
            }

            options.AddArgument($"--user-agent={setting.UserAgent}");

            // So websites (Travian) can't detect the bot
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-features=UserAgentClientHint");
            options.AddArgument("--disable-logging");
            options.AddArgument("--ignore-certificate-errors");

            options.AddArguments("--mute-audio", "--disable-gpu");

            options.AddArguments("--no-default-browser-check", "--no-first-run");
            options.AddArguments("--no-sandbox", "--test-type");
            
            if (setting.IsHeadless)
            {
                options.AddArgument("--headless=new");
            }
            else
            {
                options.AddArgument("--start-maximized");
            }

            */

            // Set GoLogin profile name
            //  options.AddArgument("--profile-directory=profile 2");

            // Set GoLogin proxy server address
            // options.AddArgument("--proxy-server= 127.0.0.1:80");
            string gologinApiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjA1ZjYyNDliNWVjMTA0YWQ3MTM3NDQiLCJ0eXBlIjoiZGV2Iiwiand0aWQiOiI2NjA1ZjY1MDY5Y2IwZGZlMWM3YjJlYWUifQ.RGL8NFxQHwoXbw8aKo0SZDmPswScFNvyf-wj5uaK9g4";
            string profileId = "660732a0ef138937eb14e9d7";
            options.AddArgument("--proxy-server=http://proxy.gologin.com:3200");
            options.AddArgument("--user-data-dir=C:\\Path\\To\\GoLogin\\Profiles\\" + profileId);
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-popup-blocking");
            // Set GoLogin browser API token as an environment variable
            options.AddArgument("--env=gologin_token=" + gologinApiToken);

            // Create GoLogin ChromeDriver instance
            IWebDriver driver = new ChromeDriver(options);

            // Open Gmail website
            driver.Navigate().GoToUrl("https://mail.google.com/");


            //=========================
            var pathUserData = Path.Combine(AppContext.BaseDirectory, "Data", "Cache", setting.ProfilePath);
            if (!Directory.Exists(pathUserData)) Directory.CreateDirectory(pathUserData);

            pathUserData = Path.Combine(pathUserData, string.IsNullOrEmpty(setting.ProxyHost) ? "default" : setting.ProxyHost);

            options.AddArguments($"user-data-dir={pathUserData}");

            _driver = await Task.Run(() => new ChromeDriver(_chromeService, options, TimeSpan.FromMinutes(3)));

            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(3);
            _driver.GetDevToolsSession();
            _wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(3)); // watch ads

            return Result.Ok();
        }

        public ChromeDriver Driver => _driver;

        public HtmlDocument Html
        {
            get
            {
                UpdateHtml();
                return _htmlDoc;
            }
        }

        public async Task Shutdown()
        {
            if (_driver is null) return;
            await Close();
            _chromeService.Dispose();
        }

        public bool IsOpen()
        {
            try
            {
                _ = _driver.Title;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string CurrentUrl => _driver.Url;

        public async Task<Result> Refresh(CancellationToken cancellationToken)
        {
            Result refresh()
            {
                try
                {
                    _driver.Navigate().Refresh();
                    return Result.Ok();
                }
                catch (Exception exception)
                {
                    return Result.Fail(new Stop(exception.Message));
                }
            }

            var result = await Task.Run(refresh);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return result;
        }

        public async Task<Result> Navigate(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return await Refresh(cancellationToken);
            }

            Result goToUrl()
            {
                try
                {
                    _driver.Navigate().GoToUrl(url);
                    return Result.Ok();
                }
                catch (Exception exception)
                {
                    return Result.Fail(new Stop(exception.Message));
                }
            }
            var result = await Task.Run(goToUrl);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return result;
        }

        private void UpdateHtml(string source = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                try
                {
                    _htmlDoc.LoadHtml(_driver.PageSource);
                }
                catch { }
            }
            else
            {
                _htmlDoc.LoadHtml(source);
            }
        }

        public TimeSpan GetTimeEnoughResource(long[] required)
        {
            var production = GetProduction();
            var storage = GetStorage();

            var missing = new long[4];

            for (var i = 0; i < 4; i++)
            {
                missing[i] = required[i] - storage[i];
            }

            var time = new TimeSpan[4];

            for (var i = 0; i < 4; i++)
            {
                time[i] = TimeSpan.FromSeconds(3600 * missing[i] / production[i]);
            }

            return time.Max();
        }

        public long[] GetProduction()
        {
            var production = _driver.ExecuteScript("return resources.production") as Dictionary<string, object>;
            return new long[] { (long)production["l1"], (long)production["l2"], (long)production["l3"], (long)production["l4"] };
        }

        public long[] GetStorage()
        {
            var storage = _driver.ExecuteScript("return resources.storage") as Dictionary<string, object>;
            return new long[] { (long)storage["l1"], (long)storage["l2"], (long)storage["l3"], (long)storage["l4"] };
        }

        public async Task<Result> Click(By by)
        {
            var elements = _driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound();
            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable();

            await Task.Run(element.Click);

            return Result.Ok();
        }

        public async Task<Result> InputTextbox(By by, string content)
        {
            var elements = _driver.FindElements(by);
            if (elements.Count == 0) return Retry.ElementNotFound();

            var element = elements[0];
            if (!element.Displayed || !element.Enabled) return Retry.ElementNotClickable();

            void input()
            {
                element.SendKeys(Keys.Home);
                element.SendKeys(Keys.Shift + Keys.End);
                element.SendKeys(content);
            }
            await Task.Run(input);

            return Result.Ok();
        }

        public async Task<Result> Wait(Func<IWebDriver, bool> condition, CancellationToken cancellationToken)
        {
            Result wait()
            {
                try
                {
                    _wait.Until(driver =>
                    {
                        if (cancellationToken.IsCancellationRequested) return true;
                        return condition(driver);
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    return Result.Fail(new Stop("Page not loaded in 3 mins"));
                }
                if (cancellationToken.IsCancellationRequested) return Result.Fail(new Cancel());
                return Result.Ok();
            }
            return await Task.Run(wait);
        }

        public async Task<Result> WaitPageLoaded(CancellationToken cancellationToken)
        {
            static bool pageLoaded(IWebDriver driver) => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");

            var retryPolicy = Policy
                .HandleResult<Result>(x => x.HasError<Stop>())
                .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(5), onRetry: (error, _, retryCount, _) =>
                {
                    _driver.Navigate().Refresh();
                });

            var poliResult = await retryPolicy.ExecuteAndCaptureAsync(() => Wait(pageLoaded, cancellationToken));

            var result = await Wait(pageLoaded, cancellationToken);
            if (result.IsFailed)
            {
                return result
                    .WithError(new Error(message: $"page stuck at loading stage [Current: {CurrentUrl}]"))
                    .WithError(new TraceMessage(TraceMessage.Line()));
            }
            return result;
        }

        public async Task<Result> WaitPageChanged(string part, CancellationToken cancellationToken)
        {
            bool pageChanged(IWebDriver driver) => driver.Url.Contains(part);
            Result result;
            result = await Wait(pageChanged, cancellationToken);
            if (result.IsFailed)
            {
                return result
                    .WithError(new Error($"page stuck at changing stage [Current: {CurrentUrl}] [Expected: {part}]"))
                    .WithError(new TraceMessage(TraceMessage.Line()));
            }

            result = await WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return result;
        }

        public async Task Close()
        {
            if (_driver is null) return;
            await Task.Run(_driver.Quit);
            _driver = null;
        }
    }
}