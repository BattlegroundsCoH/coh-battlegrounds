using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.Resources {

    public class ResourceHandler {

        private string[] m_resourceNames;

        public ResourceHandler() {
            this.m_resourceNames = GetResourcePaths(Assembly.GetExecutingAssembly()).Cast<string>().ToArray();
        }

        private static IEnumerable<object> GetResourcePaths(Assembly assembly) {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);
            try {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);
                foreach (System.Collections.DictionaryEntry resource in resourceSet) {
                    yield return resource.Key;
                }
            } finally {
                resourceManager.ReleaseAllResources();
            }
        }

        public bool HasResource(string resourceName) {
            return this.m_resourceNames.Contains(resourceName.ToLowerInvariant());
        }

        public bool HasResource(Uri resourceUri) {
            return this.m_resourceNames.Contains(resourceUri.AbsolutePath[1..].ToLowerInvariant());
        }

    }

}
