using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using SortMyStuffAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Filters;

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
            services.AddMvc(opt =>
            {
                // add JsonExceptionFilter
                opt.Filters.Add(typeof(JsonExceptionFilter));

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
