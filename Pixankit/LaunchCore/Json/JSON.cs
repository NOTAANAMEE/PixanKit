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
    }
}
