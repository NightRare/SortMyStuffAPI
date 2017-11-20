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
using System.Threading.Tasks;

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
            // Swap out with a real database in production
            //services.AddDbContext<SortMyStuffContext>(opt =>
            //    {
            //        opt.UseInMemoryDatabase(Guid.NewGuid().ToString());
            //        opt.UseOpenIddict();
            //    },
            //    ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            services.AddDbContext<SortMyStuffContext>(opt =>
            {
                var connectionString = Environment.GetEnvironmentVariable(
                    ApiStrings.EnvConnectionStrings);
                opt.UseSqlServer(connectionString);
                opt.UseOpenIddict();

            }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);

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

            services.AddScoped<IUserService, DefaultUserDataService>();
            services.AddSingleton<ILocalResourceService, DefaultLocalResourceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            using (var scope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                InitialiseDeveloperRoleAndUser(app, scope)
                    .GetAwaiter().GetResult();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    //HandleTestData(app, scope);
                }
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

        private void HandleTestData(IApplicationBuilder app, IServiceScope scope)
        {
            var dbContext = app.ApplicationServices
                .GetRequiredService<SortMyStuffContext>();

            //TestDataFactory.DeleteAllData(dbContext)
            //    .GetAwaiter().GetResult();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<UserEntity>>();
            TestDataFactory.LoadUsers(userManager)
                .GetAwaiter().GetResult();
            TestDataFactory.LoadData(dbContext, userManager)
                .GetAwaiter().GetResult();
        }

        private async Task InitialiseDeveloperRoleAndUser(
            IApplicationBuilder app, IServiceScope scope)
        {
            var context = app.ApplicationServices
                .GetRequiredService<SortMyStuffContext>();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<UserEntity>>();
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<UserRoleEntity>>();

            var developerRole = await roleManager.FindByNameAsync(ApiStrings.RoleDeveloper);
            if (developerRole == null)
            {
                await roleManager.CreateAsync(new UserRoleEntity(ApiStrings.RoleDeveloper));
            }

            var developerUid = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperUid);
            var developerRootAssetId = "rootassetid";

            var devUser = await userManager.FindByIdAsync(developerUid);
            if (devUser != null) return;

            // developer user

            var developer = new UserEntity
            {
                Id = developerUid,
                Email = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperEmail),
                UserName = "Developer",
                CreateTimestamp = DateTimeOffset.UtcNow,
                RootAssetId = developerRootAssetId
            };

            await userManager.CreateAsync(
                developer,
                Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperPassword));

            await userManager.AddToRoleAsync(developer, ApiStrings.RoleDeveloper);
            await userManager.UpdateAsync(developer);

            var root = new AssetEntity
            {
                Id = developerRootAssetId,
                Name = "Assets",
                ContainerId = ApiStrings.RootAssetToken,
                CategoryId = null,
                UserId = developerUid
            };

            await context.AddAsync(root);
            await context.SaveChangesAsync();
        }
    }
}
