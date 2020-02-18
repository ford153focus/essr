using System;
using System.Linq;
using System.Threading.Tasks;
using essr.Models;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;

namespace essr.Controllers
{
    public class PrerenderController : Controller
    {
        public async Task<CachedPage> GetPageFromCacheAsync(string url)
        {
            var db = new CachedPageContext();
            db.Database.EnsureCreated();

            // search page in cache database
            var page = db.CachedPages
                .FirstOrDefault(cachedPage => cachedPage.Url == url && cachedPage.TimeStamp > Utils.Dt.UnixNow() - Utils.Dt.SecsInWeek);

            if (page != null) return page;

            // save page to cache and relaunch method recursively
            await FillCache(url);
            return await GetPageFromCacheAsync(url);
        }

        private async Task FillCache(string url)
        {
            Chromium chromium = Chromium.GetInstance();
            Response response;
            string sourceCode;
            PrerenderSpecials prerenderSpecials;

            // relaunch method on chromium fail
            try
            {
                (response, sourceCode, prerenderSpecials) = await chromium.GoToUrlAsync(url);
            }
            catch (Exception)
            {
                await FillCache(url);
                return;
            }

            // save page to db
            var db = new CachedPageContext();
            db.Database.EnsureCreated();
            db.CachedPages.Add(new CachedPage
            {
                Url = url,
                TimeStamp = Utils.Dt.UnixNow(),
                AnswerHttpCode = string.IsNullOrEmpty(prerenderSpecials.StatusCode) ? (int)response.Status : Convert.ToInt32(prerenderSpecials.StatusCode),
                SourceCode = sourceCode
            });
            db.SaveChanges();
        }
    }
}
