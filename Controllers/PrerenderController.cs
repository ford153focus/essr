using System;
using System.Linq;
using System.Threading.Tasks;
using essr.Models;
using Microsoft.AspNetCore.Mvc;

namespace essr.Controllers
{
    public class PrerenderController : Controller
    {
        public async Task<CachedPage> GetPageFromCacheAsync(string url)
        {
            var db = new CachedPageContext();
            db.Database.EnsureCreated();
            var page = db.CachedPages
                .FirstOrDefault(cachedPage => cachedPage.Url == url && cachedPage.TimeStamp > Utils.Dt.UnixNow() - Utils.Dt.SecsInWeek);

            if (page != null) return page;

            await FillCache(url);
            return await GetPageFromCacheAsync(url);
        }

        private async Task FillCache(string url)
        {
            var chromium = Chromium.GetInstance();
            var (response, sourceCode) = await chromium.GoToUrlAsync(url);

            var db = new CachedPageContext();
            db.Database.EnsureCreated();
            db.CachedPages.Add(new CachedPage
            {
                Url = url,
                TimeStamp = Utils.Dt.UnixNow(),
                AnswerHttpCode = (int) response.Status,
                SourceCode = sourceCode
            });
            db.SaveChanges();
        }
    }
}