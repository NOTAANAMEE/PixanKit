using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Extention
{
    /// <summary>
    /// Path Dictionary
    /// </summary>
    public static partial class Paths
    {
        static Dictionary<string, string> PathDict = new();

        /// <summary>
        /// Add A New Path
        /// </summary>
        /// <param name="key">Key For Finding And Replacing</param>
        /// <param name="value">The Actual Path</param>
        public static void Add(string key, string value)
        {
            PathDict.Add(key, value);
        }

        /// <summary>
        /// Set The Path
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">The Final Path</param>
        public static void Set(string key, string value)
        {
            PathDict[key] = value;
        }

        /// <summary>
        /// If Has Key, call Set() else call Add()
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void TrySet(string key, string value)
        {
            if (!PathDict.ContainsKey(key)) Add(key, value);
            else Set(key, value);
        }

        /// <summary>
        /// Try get the value. If exists, value will be he path. Else, value
        /// will be null and return false
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue(string key, out string? value)
        {
            try
            {
                value = Get(key);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Get The Processed Path
        /// </summary>
        /// <param name="key"></param>
        /// <returns>THe FInal Path After Replacement</returns>
        public static string Get(string key)
            => Replace(PathDict[key]);

        /// <summary>
        /// Get the Path. If not exist, add the item.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetOrAdd(string key, string value)
        {
            if (!PathDict.TryGetValue(key, out string? Ret)) {
                Add(key, value);
                Ret = value; 
            }
            return Replace(Ret);
        }


        private static string Replace(string value)
        {
            string result = MyRegex().Replace(value, match =>
            {
                string key = match.Groups[1].Value; 
                return PathDict.TryGetValue(key, out string? value) ? value : match.Value; 
            });
            return value;
        }

        [GeneratedRegex(@"\${(.*?)}")]
        private static partial Regex MyRegex();
    }
}
