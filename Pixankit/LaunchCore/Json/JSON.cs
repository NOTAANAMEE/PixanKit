using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Json
{
    public static class JSON
    {
        public static JObject ReadFromFile(string file)
        {
            StreamReader sr = new(file);
            JsonTextReader reader = new(sr);
            JObject ret = JObject.Load(reader);
            reader.Close();
            sr.Close();
            return ret;
        }

        public static void SaveFromFile(string file, JObject obj) 
        {
            StreamWriter sw = new(file);
            JsonTextWriter writer = new(sw);
            obj.WriteTo(writer);
            sw.Close();
            writer.Close();
        }

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
                    MergeJObject((JObject)target[key], (JObject)needtomerge);
                    break;
                case JTokenType.Array:
                    MergeJArray((JArray)target[key], (JArray)needtomerge);
                    break;
                default:
                    target[key] = needtomerge;
                    break;
            }
        }

        public static void MergeJArray(JArray target, JArray needtomerge)
        {
            foreach (var item in needtomerge)
                if (!target.Contains(item)) target.Add(item);
        }
    }
}
