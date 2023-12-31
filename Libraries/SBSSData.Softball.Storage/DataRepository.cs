using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Storage
{
    public static class DataRepository
    {
        public static Assembly GetAssembly() => typeof(DataRepository).Assembly;

        public static List<string> GetResourceNames() => GetAssembly().GetManifestResourceNames().ToList();

        public static byte[] GetResource(string resourceName)
        {
            byte[] bytes = [];
            if (GetResourceNames().Contains(resourceName))
            {
                bytes = GetAssembly().GetEmbeddedResourceAsBytes(resourceName);
            }

            return bytes;
        }

        public static string GetResourceName(string fileName)
        {
            string resourceName = string.Empty;
            IEnumerable<string> names = GetResourceNames().Where(n => n.EndsWith(fileName, StringComparison.Ordinal));
            if (names.Any())
            {
                resourceName = names.First();
            }

            return resourceName;
        }

        public static string GetStringResource(string resourceName) => GetResource(resourceName).ByteArrayToString();

        public static string GetJsonResource(string resourceName)
        {
            string json = string.Empty;

            if (resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                json = GetStringResource(resourceName);
            }

            return json;
        }
    }
}
