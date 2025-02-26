using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Json
{
    /// <summary>
    /// This class implements some methods that helps read the json
    /// and merge the json
    /// </summary>
    public static class JSON
    {
        /// <summary>
        /// The method reads the JSON data from file
        /// </summary>
        /// <remarks>File should start with '{' and end with '}'</remarks>
        /// <param name="file">The path of the JSON file</param>
        /// <returns>The JSON data</returns>
        public static JObject ReadFromFile(string file)
        {
            StreamReader sr = new(file);
            JsonTextReader reader = new(sr);
            JObject ret = JObject.Load(reader);
            reader.Close();
            sr.Close();
            return ret;
        }

        /// <summary>
        /// The method saves the JObject to the file
        /// </summary>
        /// <param name="file">the exact path of the file</param>
        /// <param name="obj">the JObject JSON data</param>
        public static void SaveFile(string file, JObject obj) 
        {
            StreamWriter sw = new(file);
            JsonTextWriter writer = new(sw);
            obj.WriteTo(writer);
            sw.Close();
            writer.Close();
        }

        /// <summary>
        /// The method merges the 2 JObjects.
        /// The result will be stored in target
        /// </summary>
        /// <param name="target">the target JSON data</param>
        /// <param name="needtomerge">the JSON data that needs to merge to the target</param>
        public static void MergeJObject(JObject target, JObject needtomerge)
        {
            foreach (var item in needtomerge)
            {
                if (target[item.Key] == null)
                {
                    target.Add(item.Key, item.Value);
                    continue;
                }
                if (item.Value == null) continue;
                MergeEachJObject(target, item.Value, item.Key);
            }
        }

        private static void MergeEachJObject(JObject target, JToken needtomerge, string key)
        {
            if (target == null) return;
            switch (target[key]?.Type)
            {
                case JTokenType.Object:
                    MergeJObject(
                        target[key] is JObject jobject? jobject : [], (JObject)needtomerge);
                    break;
                case JTokenType.Array:
                    MergeJArray(
                        target[key] is JArray array? array : [], (JArray)needtomerge);
                    break;
                default:
                    target[key] = needtomerge;
                    break;
            }
        }

        /// <summary>
        /// The method merges the 2 JArrays.
        /// It will append the needtomerge array at the end of target array
        /// </summary>
        /// <param name="target">The target JArray</param>
        /// <param name="needtomerge">The array that needs to append</param>
        public static void MergeJArray(JArray target, JArray needtomerge)
        {
            foreach (var item in needtomerge)
                if (!target.Contains(item)) target.Add(item);
        }

        /// <summary>
        /// Tries to get a value from the JObject at the specified path and formats it.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <param name="obj">The JObject to search.</param>
        /// <param name="format">The function to format the JToken to the desired type.</param>
        /// <param name="Path">The path to the value in the JObject.</param>
        /// <param name="output">The output value if found and formatted successfully.</param>
        /// <returns>True if the value was found and formatted successfully, otherwise false.</returns>
        public static bool TryGetValue<T>(this JObject obj, Func<JToken, T> format, string Path, out T? output)
        {
            output = default;
            var tok = GetFromPath(obj, Path);
            if (tok == null) return false;
            try { output = format(tok); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Gets a value from the JObject at the specified path and formats it.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <param name="obj">The JObject to search.</param>
        /// <param name="format">The function to format the JToken to the desired type.</param>
        /// <param name="Path">The path to the value in the JObject.</param>
        /// <returns>The formatted value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the path is not found in the JObject.</exception>
        public static T GetValue<T>(this JObject obj, Func<JToken, T> format, string Path)
        {
            var tok = GetFromPath(obj, Path);

            return format(tok ?? throw new InvalidOperationException("" +
                "JSON Path Not Found!"));
        }

        /// <summary>
        /// Gets a value from the JObject at the specified path and formats it, or returns a default value if the path is not found.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <param name="obj">The JObject to search.</param>
        /// <param name="format">The function to format the JToken to the desired type.</param>
        /// <param name="Path">The path to the value in the JObject.</param>
        /// <param name="defaultVal">The default value to return if the path is not found.</param>
        /// <returns>The formatted value or the default value.</returns>
        public static T GetOrDefault<T>(this JObject obj, Func<JToken, T> format, string Path, T defaultVal)
        {
            var tok = GetFromPath(obj, Path);
            if (tok == null) return defaultVal;
            return format(tok);
        }

        /// <summary>
        /// Gets a JToken from the JObject at the specified path.
        /// </summary>
        /// <param name="obj">The JObject to search.</param>
        /// <param name="Path">The path to the value in the JObject.</param>
        /// <returns>The JToken at the specified path, or null if not found.</returns>
        public static JToken? GetFromPath(this JObject obj, string Path)
        {
            JToken? token = obj;
            string[] keys = Path.Split('/');
            int ind = 0;
            while (ind < keys.Length)
            {
                string key = keys[ind++];
                if (key == "") continue;
                switch (token.Type)
                {
                    case JTokenType.Object:
                        token = obj[key];
                        break;
                    case JTokenType.Array:
                        token = obj[int.Parse(key)];
                        break;
                    default:
                        return null;
                }
                if (token == null) return null;
            }
            return token;
        }

        public static T ConvertTo<T>(this JToken? token, Func<JToken, T> format, T defaultVal)
        {
            if (token == null) return defaultVal;
            return format(token);
        }

        /// <summary>
        /// This class provides some functions that converts JToken
        /// to other classes
        /// </summary>
        public static class Format
        {
            /// <summary>
            /// Convert the JToken to string
            /// </summary>
            /// <param name="tok">The token that needed to convert</param>
            /// <returns>the result of convert</returns>
            public static string ToString(JToken tok)
                => tok.ToString();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tok"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public static int ToInt32(JToken tok)
            {
                if (tok.Type == JTokenType.Integer) return (int)tok;
                throw new InvalidOperationException("Token is not an Integer");
            }

            public static DateTime ToDateTime(JToken tok)
                => DateTime.Parse(tok.ToString());

            public static bool ToBool(JToken tok)
            {
                if(tok.Type == JTokenType.Boolean) return (bool)tok;
                throw new InvalidOperationException("Token is not a bool");
            }

            public static double ToDouble(JToken tok)
            {
                if (tok.Type == JTokenType.Float) return (double)tok;
                throw new InvalidOperationException("Token is not a double");
            }

            public static JObject ToJObject(JToken tok)
            {
                if (tok.Type == JTokenType.Object)return (JObject)tok;
                throw new InvalidOperationException("Token is not an object");
            }

            public static JArray ToJArray(JToken tok)
            {
                if (tok.Type == JTokenType.Array) return (JArray)tok;
                throw new InvalidOperationException("Token is not an array");
            }
        }
    }
}
