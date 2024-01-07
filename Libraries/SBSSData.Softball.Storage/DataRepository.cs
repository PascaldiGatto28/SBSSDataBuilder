using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Storage
{
    /// <summary>
    /// Utilities to manage the embedded resources whose links are in the <c>Data</c> file.
    /// </summary>
    /// <remarks>
    /// The embedded resources are JSON files for the various data stores and the current log file. The files
    /// actually are in OneDrive, but using links allows the files to be easily accessed by the class methods.
    /// The only files that are updated are the current data store and the log and associated <c>.json</c> file.
    /// </remarks>
    public static class DataRepository
    {
        /// <summary>
        /// Returns the <see cref="Assembly"/> in which the embedded resources are located.
        /// </summary>
        /// <returns></returns>
        public static Assembly GetAssembly() => typeof(DataRepository).Assembly;

        /// <summary>
        /// The list of all embedded resource names.
        /// </summary>
        /// Resources in this assembly have the resource name <c>SBSSData.Softball.Storage.Data.[fileName}</c>
        /// where "[fileName]" is the name of the file in the <c>Data</c> folder of this project.
        /// <returns>
        /// The list of embedded resources; the empty list is returned if no embedded resources
        /// are found
        /// </returns>
        public static List<string> GetResourceNames() => GetAssembly().GetManifestResourceNames().ToList();

        /// <summary>
        /// Returns the embedded resource as an array of bytes.
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource; it must be name returned by
        /// <see cref="GetResourceName"/>
        /// </param>
        /// <returns>
        /// The contents of the resource as an array of <see cref="byte"/>. If the <paramref name="resourceName"/>
        /// is <c>null</c> or empty, or the name is not the resource name of an embedded resource in this assembly
        /// the empty array is returned. <c>null</c> is never returned.
        /// </returns>
        public static byte[] GetResource(string resourceName)
        {
            byte[] bytes = [];
            if (!string.IsNullOrEmpty(resourceName) && GetResourceNames().Contains(resourceName))
            {
                bytes = GetAssembly().GetEmbeddedResourceAsBytes(resourceName);
            }

            return bytes;
        }

        /// <summary>
        /// Returns the resource name associated with an embedded resource file in the <c>Data</c> folder of this
        /// project.
        /// </summary>
        /// <param name="fileName">
        /// The full file nName (not path) of the embedded resource. For example "DataStoreManager.log".
        /// </param>
        /// <returns>
        /// The resource name associated with the embedded resource. Using the example in <paramref name="fileName"/>,
        /// <c>SBSSData.Softball.Storage.Data.DataStoreManager.log</c> is returned. If the <c>fileName </c> is null
        /// or empty, or no embedded resource for that file can be found, the empty string is returned. <c>null</c>
        /// is never returned.
        /// </returns>
        public static string GetResourceName(string fileName)
        {
            string resourceName = string.Empty;
            if (!string.IsNullOrEmpty(fileName))
            {
                IEnumerable<string> names = GetResourceNames().Where(n => n.EndsWith(fileName ?? string.Empty, StringComparison.Ordinal));
                if (names.Any())
                {
                    resourceName = names.First();
                }
            }

            return resourceName;
        }

        /// <summary>
        /// Returns the contents of the embedded resource as an UTF8 string.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The byte data of the resource converted to a string. If the <paramref name="resourceName"/>
        /// is <c>null</c> or empty, or the resource does not exist, the empty string is returned.</returns>
        public static string GetStringResource(string resourceName) => GetResource(resourceName).ByteArrayToString();


        /// <summary>
        /// Returns the contents of the file embedded resource as an UTF8 string.
        /// </summary>
        /// <remarks>
        /// This is often the easy way to get the resource. 
        /// </remarks>
        /// <param name="fileName">
        /// The full file nName (not path) of the embedded resource. For example "DataStoreManager.log".
        /// <returns>The byte data of the resource converted to a string. If the <paramref name="fileName"/>
        /// is <c>null</c> or empty, or the resource does not exist, the empty string is returned.</returns>
        public static string GetFileResource(string fileName) => GetStringResource(GetResourceName(fileName));
    }
}
