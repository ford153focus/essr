using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace essr
{
    class Chromium
    {
        private static Chromium _instance;
        private Browser _browser;

        private Chromium()
        {
            Task.Run(() => SpawnBrowser()).Wait();
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
                            "--no-sandbox",
                        },
                DumpIO = true,
                LogProcess = true,
            });
        }

        public async Task<Tuple<Response, string>> GoToUrlAsync(string url)
        {
            // Chromium can die unexpectedly
            // Respawn it if failed
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
                    Timeout = 10530,
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }
                });
            }
            catch (Exception)
            {
                // Nothing is required to do, it's ok
            }
            
            // Trying to fix base href            
            try
            {
                var tabHostUrl = await page.EvaluateExpressionAsync<string>("window.location.protocol+'//'+window.location.host+'/'");
                var baseSelector = await page.EvaluateExpressionAsync<dynamic>("document.querySelector('base')");
                if (baseSelector == null) {
                    await page.EvaluateExpressionAsync<dynamic>($"base=document.createElement('base');base.href='{tabHostUrl}';document.getElementsByTagName('head')[0].appendChild(base);");
                } else {
                    await page.EvaluateExpressionAsync<dynamic>($"document.querySelector('base').href='{tabHostUrl}'");
                }
                
            }
            catch (Exception) {
                // Nothing is required to do, it's ok
            }

            // Grab source code of page
            var pageSourceCode = await page.GetContentAsync();

            // Close tab to save resources
            await page.CloseAsync();

            return Tuple.Create(response, pageSourceCode);
        }
    }
}
