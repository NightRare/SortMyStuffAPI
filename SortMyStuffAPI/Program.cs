using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Identity;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            //var serviceScopeFactory = (IServiceScopeFactory)host.Services.GetService(typeof(IServiceScopeFactory));
            ////Add test roles and users in development
            //using (var scope = serviceScopeFactory.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var dbContext = services.GetRequiredService<SortMyStuffContext>();

            //    var roleManager = scope.ServiceProvider
            //        .GetRequiredService<RoleManager<UserRoleEntity>>();
            //    var userManager = scope.ServiceProvider
            //        .GetRequiredService<UserManager<UserEntity>>();
            //    TestDataRepository.LoadRolesAndUsers(roleManager, userManager).Wait();

            //    // Add test data in development
            //    TestDataRepository.LoadData(dbContext, userManager);
            //}

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    if (!builderContext.HostingEnvironment.IsDevelopment()) return;

                    configBuilder.AddJsonFile("Properties\\launchSettings.json", optional: false, reloadOnChange: true);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
