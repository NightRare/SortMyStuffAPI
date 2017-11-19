using System;
using System.Linq;
using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using SortMyStuffAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Filters;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;
using SortMyStuffAPI.Models;
using AspNet.Security.OAuth.Validation;

namespace SortMyStuffAPI
{
    public class Startup
    {
        private readonly int? _httpsPort;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _httpsPort = configuration.GetValue<int>("iisSettings:iisExpress:sslPort");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject options from Configuration
            services.Configure<PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));
            services.Configure<ApiConfigs>(Configuration.GetSection("ApiConfigurations"));

            // Use in-memroy db for dev, change the scope of the DbContext and the option to Singleton
            // TODO: Swap out with a real database in production
            services.AddDbContext<SortMyStuffContext>(opt =>
                {
                    opt.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    opt.UseOpenIddict();
                },
                ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            //services.AddDbContext<SortMyStuffContext>(opt =>
            //{
            //    opt.UseSqlServer(Environment.GetEnvironmentVariable(
            //        ApiStrings.EnvConnectionStrings));
            //});

            // Add ASP.NET Core Identity
            services.AddIdentity<UserEntity, UserRoleEntity>()
                .AddEntityFrameworkStores<SortMyStuffContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = OAuthValidationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = OAuthValidationDefaults.AuthenticationScheme;
                opt.DefaultAuthenticateScheme = OAuthValidationDefaults.AuthenticationScheme;
            }).AddOAuthValidation();

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(ApiStrings.PolicyDeveloper,
                    p => p.RequireAuthenticatedUser().RequireRole(ApiStrings.RoleDeveloper));
            });

            // Map some of the default claim names to the proper OpenID Connect claim names
            services.Configure<IdentityOptions>(opt =>
            {
                opt.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                opt.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                opt.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Add OpenIddict services
            services.AddOpenIddict<string>(opt =>
            {
                opt.AddEntityFrameworkCoreStores<SortMyStuffContext>();
                opt.AddMvcBinders();

                opt.EnableTokenEndpoint("/token");
                opt.AllowPasswordFlow();
            });

            services.AddAutoMapper();

            services.AddMvc(opt =>
            {
                //*********************************************
                // TODO: DISABLE THIS IN PRODUCTION
                // toggle authentication in development
                //*********************************************
                //opt.Filters.Add(new AllowAnonymousFilter());

                // add Filters
                opt.Filters.Add(typeof(JsonExceptionFilter));
                opt.Filters.Add(typeof(LinkRewrittingFilter));

                // setting up the ssl port for development mode
                // this will be ignored if not in development mode and _httpsPort will be null
                opt.SslPort = _httpsPort;
                // Add RequireHttps Attribute to all the controllers
                opt.Filters.Add(typeof(RequireHttpsAttribute));

                // change json formatter
                var jsonFormatter = opt.OutputFormatters.OfType<JsonOutputFormatter>().Single();
                opt.OutputFormatters.Remove(jsonFormatter);
                opt.OutputFormatters.Add(new IonOutputFormatter(jsonFormatter));


            });

            // lower case routes
            services.AddRouting(opt => opt.LowercaseUrls = true);

            services.AddApiVersioning(opt =>
            {
                opt.ApiVersionReader = new MediaTypeApiVersionReader();
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.DefaultApiVersion = new ApiVersion(0, 1);
                opt.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opt);
            });

            // Dependency injeciton
            services.AddScoped<IAssetDataService, DefaultAssetDataService>();
            services.AddScoped<ICategoryDataService, DefaultCategoryDataService>();
            services.AddScoped<IUserDataService, DefaultUserDataService>();
            services.AddScoped<IBaseDetailDataService, DefaultBaseDetailDataService>();
            services.AddScoped<IDetailDataService, DefaultDetailDataService>();

            services.AddScoped<IPhotoFileService, DefaultFileService>();
            services.AddScoped<IThumbnailFileService, DefaultFileService>();

            services.AddSingleton<ILocalResourceService, DefaultLocalResourceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //// Add test roles and users in development
                //using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                //{
                //    var dbContext = app.ApplicationServices.GetRequiredService<SortMyStuffContext>();

                //    var roleManager = scope.ServiceProvider
                //        .GetRequiredService<RoleManager<UserRoleEntity>>();
                //    var userManager = scope.ServiceProvider
                //        .GetRequiredService<UserManager<UserEntity>>();
                //    TestDataRepository.LoadRolesAndUsers(roleManager, userManager).Wait();

                //    // Add test data in development
                //    TestDataRepository.LoadData(dbContext, userManager);
                //}
            }

            // include HSTS header
            app.UseHsts(opt =>
            {
                opt.MaxAge(days: 360);
                opt.IncludeSubdomains();
                opt.Preload();
            });

            app.UseMvc();
        }
    }
}
