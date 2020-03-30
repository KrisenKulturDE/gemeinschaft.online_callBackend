using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Callcenter.Config;
using Callcenter.Controllers;
using Callcenter.DBConnection;
using Callcenter.Models;
using Callcenter.Models.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDbGenericRepository;

namespace Callcenter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddOptions();
            services.Configure<MongoDbConf>(Configuration.GetSection(nameof(MongoDbConf)));
            services.AddSignalR();
            services.AddSingleton<Database>();
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                 //Hier muss irgendwie rein, des es über die gemeinsame db verbindung mit Database geht, komme nur noch nicht an das singleton.
                 .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>("mongodb://192.168.10.142:27017", "coronadb")
                 .AddDefaultTokenProviders();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            ////app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<SignalRHub>("/Hub");
            });
        }
    }
}
