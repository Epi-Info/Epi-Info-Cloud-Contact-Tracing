using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Epi.Cloud.Resources
{
    public static class ResourceProvider
    {
        static Dictionary<string, ResourceManager> _resourceManagers = new Dictionary<string, System.Resources.ResourceManager>();
        static Assembly _thisAssembly = typeof(ResourceProvider).Assembly;

        public static ResourceManager GetResourceManager(string resourceNamespace)
        {
            ResourceManager resourceManager;
            lock (_resourceManagers)
            {
                if (!_resourceManagers.TryGetValue(resourceNamespace, out resourceManager))
                {
                    resourceManager = new ResourceManager(resourceNamespace, _thisAssembly);
                    _resourceManagers.Add(resourceNamespace, resourceManager);
                }
            }
            return resourceManager;
        }

        public static string GetResourceString(string resourceNamespace, string resourceName)
        {
            return GetResourceManager(resourceNamespace).GetString(resourceName);
        }

        public static string GetResourceString(string resourceNamespace, string resourceName, CultureInfo culture)
        {
            return GetResourceManager(resourceNamespace).GetString(resourceName, culture);
        }
    }
}
