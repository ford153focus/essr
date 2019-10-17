using System.Net;
using essr.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace essr
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.Run(async context =>
            {
                var url = context.Request.Path.ToString().TrimStart('/');

                if (!Utils.Validators.IsValidUrl(url))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Bad Request");
                    return;
                }

                var prerenderController = new PrerenderController();
                var result = await prerenderController.GetPageFromCacheAsync(url);
                context.Response.StatusCode = result.AnswerHttpCode;
                await context.Response.WriteAsync(result.SourceCode);
            });
        }
    }
}