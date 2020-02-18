using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace essr
{
    class PrerenderSpecials
    {
        public string StatusCode {get; set;}
        public string Header {get; set;}
    }

    class Chromium
    {
        private static Chromium _instance;
        private Browser _browser;

        private Chromium()
        {
            Task.Run(SpawnBrowser).Wait();
        }

        ~Chromium()
        {
            Task.Run(
                async () => { await _browser.CloseAsync(); }
            ).Wait();
        }

        public static Chromium GetInstance()
        {
            return _instance ??= new Chromium();
        }

        private async void SpawnBrowser()
        {
            RevisionInfo browserFetcher = await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                IgnoreHTTPSErrors = true,
                Headless = true, //TODO: set 'true' before commit!
                ExecutablePath = browserFetcher.ExecutablePath,
                Args = new[]
                {
                    "--disable-gpu",
                    "--enable-logging",
                    "--headless",
                    "--no-sandbox"
                },
                DumpIO = true,
                LogProcess = true
            });
        }

        public async Task<Tuple<Response, string, PrerenderSpecials>> GoToUrlAsync(string url)
        {
            // Chromium can die unexpectedly - respawn on fail
            if (_browser == null || _browser.IsClosed)
            {
                SpawnBrowser();
            }

            var page = await _browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080
            });

            var response = await page.GoToAsync(url);

            // Waiting for full application load, not just DOM
            // WaitUntilNavigation.Networkidle{0,2} is very unstable
            // So wrap it in try-catch to avoid unwanted exceptions
            try
            {
                await page.WaitForNavigationAsync(new NavigationOptions
                {
                    Timeout = 5310,
                    WaitUntil = new[] {WaitUntilNavigation.Networkidle0}
                });
            }
            catch (Exception)
            {
                // Nothing is required to do, it's ok
            }

            #region Trying to fix base href
            try
            {
                var baseFixerPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
                    "js", 
                    "base_fixer.js"
                );
                var baseFixerStr = File.ReadAllText(baseFixerPath);

                await page.EvaluateExpressionAsync<dynamic>(baseFixerStr);
            }
            catch (Exception)
            {
                // Nothing is required to do, it's ok
            }
            #endregion

            #region Trying to get and parse special prerender meta-tag
            var prerenderSpecials = new PrerenderSpecials();
            try
            {
                prerenderSpecials.StatusCode = await page.EvaluateExpressionAsync<string>("document.querySelector('meta[name=\"prerender-status-code\"]') ? document.querySelector('meta[name=\"prerender-status-code\"]').content : null");
                prerenderSpecials.Header = await page.EvaluateExpressionAsync<string>("document.querySelector('meta[name=\"prerender-header\"]') ? document.querySelector('meta[name=\"prerender-header\"]').content : null");
            }
            catch (Exception)
            {
                // Nothing is required to do, it's ok
            }
            #endregion

            // Grab source code of page
            var pageSourceCode = await page.GetContentAsync();

            // Close tab to save resources
            await page.CloseAsync();

            return Tuple.Create(response, pageSourceCode, prerenderSpecials);
        }
    }
}
