using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Services
{
    public class DefaultLocalResourceService : ILocalResourceService
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly IHostingEnvironment _env;

        private readonly IDictionary<LocalResources, string> _resPaths;

        public DefaultLocalResourceService(
            IOptions<ApiConfigs> apiConfigs, 
            IHostingEnvironment env)
        {
            _env = env;
            _apiConfigs = apiConfigs.Value;
            _resPaths = InitialiseResourcePaths(_env.ContentRootPath, _apiConfigs);
        }

        public Stream GetResource(LocalResources resTag)
        {
            if (!_resPaths.TryGetValue(resTag, out var path))
                return null;

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public async Task<Stream> GetResourceAsync(LocalResources resTag, CancellationToken ct)
        {
            return await Task.Run(() => GetResource(resTag), ct);
        }

        private IDictionary<LocalResources, string> InitialiseResourcePaths(string rootPath, ApiConfigs configs)
        {
            var dict = new Dictionary<LocalResources, string>();

            var allProperties = configs.GetType()
                .GetTypeInfo()
                .DeclaredProperties
                .Where(p => p.GetCustomAttributes<LocalResourceAttribute>().Any());

            foreach(var prop in allProperties)
            {
                var tag = Enum.Parse<LocalResources>(prop.Name);
                var filePath = (string) prop.GetValue(configs);
                var completePath = Path.Combine(
                    rootPath,
                    Path.Combine(filePath.Split('/', StringSplitOptions.RemoveEmptyEntries)));
                dict.Add(tag, completePath);
            }

            return dict;
        }
    }

    public enum LocalResources
    {
        DefaultPhoto = 0,
        DefaultThumbnail = 1
    }
}
