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
    public static class Paths
    {
        static Dictionary<string, string> Path = new();

        /// <summary>
        /// Add A New Path
        /// </summary>
        /// <param name="key">Key For Finding And Replacing</param>
        /// <param name="value">The Actual Path</param>
        public static void Add(string key, string value)
        {
            Path.Add(key, value);
        }

        /// <summary>
        /// Set The Path
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">The Final Path</param>
        public static void Set(string key, string value)
        {
            Path[key] = value;
        }

        /// <summary>
        /// If Has Key, call Set() else call Add()
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void TrySet(string key, string value)
        {
            if (!Path.ContainsKey(key)) Add(key, value);
            else Set(key, value);
        }

        /// <summary>
        /// Get The Processed Path
        /// </summary>
        /// <param name="key"></param>
        /// <returns>THe FInal Path After Replacement</returns>
        public static string Get(string key)
        {
            string Ret = Path[key];
            string pat = Match(Ret);
            return Replace(pat, Ret);
        }

        private static string Match(string val)
        {
            string pattern = @"\$\{.*?\}";
            Regex.Match(val, pattern);
            MatchCollection matches = Regex.Matches(val, pattern);
            if (matches.Count > 1) throw new Exception();
            if (matches.Count == 0) return "";
            return matches[0].Value;
        }

        private static string KeyProcess(string key)
            => key.Substring(2, key.Length - 3).Trim();

        private static string Replace(string dictkey, string val)
        {
            if (dictkey == "") return val;
            var key = KeyProcess(dictkey);
            if (!Path.TryGetValue(key, out string? value)) return val;
            return val.Replace(dictkey, value);
        }
    }
}
