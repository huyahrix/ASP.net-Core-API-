using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using ApiCore.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using ApiCore.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApiCore.Interfaces;
using System.Text;
using ApiCore.Infrastructure;

namespace ApiCore
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

            services.AddCors();
            services.AddDbContext<NorthwindContext>(Options =>
                Options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<NorthwindContext>();

            services.AddRouting();  
          
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1 );

            services.AddAutoMapper();
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = int.Parse(context.Principal.Identity.Name);
                        var user = userService.GetById(userId);
                        if (user == null)
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }
                        return Task.CompletedTask;
                    }
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            // add swagger UI
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info
            //    {
            //        Version = "v1",
            //        Title = "API",
            //        Description = "ASP.NET Core Web API",
            //        TermsOfService = "None",
            //        Contact = new Contact() { Name = "Ahrix", Email = "huy.ahrix@outlook.com", Url = "www" }
            //    });
            //});
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "API",
                    Description = "ASP.NET Core Web API",
                    TermsOfService = "None",
                    Contact = new Contact() { Name = "ApiCore", Email = "huy.ahrix@outlook.com", Url = "www.www.com" }
                });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                { "Bearer", Enumerable.Empty<string>() },
            });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage(); 
            //    app.UseDatabaseErrorPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    app.UseHsts();
            //}

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();
            


            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                   name: "default",
                   template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
