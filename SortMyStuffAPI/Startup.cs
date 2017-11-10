using System;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddDbContext<SortMyStuffContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()),
                ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            services.AddAutoMapper();

            services.AddMvc(opt =>
            {
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
            services.AddSingleton<IAssetDataService, DefaultDataService>();
            services.AddSingleton<ICategoryDataService, DefaultDataService>();
            services.AddSingleton<IPhotoFileService, DefaultFileService>();
            services.AddSingleton<IThumbnailFileService, DefaultFileService>();
            services.AddSingleton<ILocalResourceService, DefaultLocalResourceService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Add test data in development
                var dbContext = app.ApplicationServices.GetRequiredService<SortMyStuffContext>();
                TestDataRepository.LoadAllIntoContext(dbContext);
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
