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
            Task.Run(
                async () =>
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
                            "--remote-debugging-port=9222",
                            "--v=1",
                        },
                        DumpIO = true,
                        LogProcess = true,
                    });
                }
            ).Wait();
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

        public async Task<Tuple<Response, string>> GoToUrlAsync(string url)
        {
            var page = await _browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080
            });

            var response = await page.GoToAsync(url);
            try
            {
                await page.WaitForNavigationAsync(new NavigationOptions
                {
                    WaitUntil = new[] {WaitUntilNavigation.Networkidle2}
                });
            }
            catch (Exception){}

            try
            {
                var tabHostUrl = await page.EvaluateExpressionAsync<string>("window.location.protocol+'//'+window.location.host+'/'");
                await page.EvaluateExpressionAsync<dynamic>($"document.querySelector('base').href='{tabHostUrl}'");
            }
            catch (Exception){}

            var pageSourceCode = await page.GetContentAsync();

            await page.CloseAsync();

            return Tuple.Create(response, pageSourceCode);
        }
    }
}