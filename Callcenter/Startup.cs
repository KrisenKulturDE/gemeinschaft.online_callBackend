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
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using MongoDbGenericRepository;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;

namespace Callcenter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var dbconfigobject = Configuration.GetSection(nameof(MongoDbConf));
            var dbconfig = dbconfigobject.Get<MongoDbConf>();
            services.Configure<MongoDbConf>(dbconfigobject);
            services.AddSingleton<Database>();
            services.AddSingleton<JwtManager>();
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                 .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(dbconfig.Connection, dbconfig.DbName);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/Hub/")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(dbconfig.JWTSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });


            services.AddSignalR();
            services.AddMvc();

            services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<SignalRHub>("/Hub");
            });
        }
    }
}
