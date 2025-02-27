using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Tomlyn.Model;

namespace PixanKit.ModController
{
    static class TOML
    {
        public static object? GetPath(this TomlTable table, string path) 
        {
            string[] keys = path.Split('/');
            object value = table;
            foreach (var key in keys) 
            {
                if (key == "") continue;
                switch (value) 
                {
                    case TomlTable tomltable:
                        if (!tomltable.ContainsKey(key)) return null;
                        value = tomltable[key];
                        break;
                    case TomlTableArray tomlarray:
                        var sccss = int.TryParse(key, out var index);
                        if (!sccss) return null;
                        if (tomlarray.Count <= index) return null;
                        value = tomlarray[index];
                        break;
                    default:
                        return null;
                }
            }
            return value;
        }

        public static T GetValue<T>(this TomlTable table, string key)
        {
            var val = table[key];
            if (val == null || val.GetType() != typeof(T))
                throw new InvalidOperationException("Not valid");
            return (T)val;
        }

        public static T GetOrDefault<T>(this TomlTable table, string key, T defaultVal)
        {
            var val = table[key];
            if (val == null || val.GetType() != typeof(T))
                return defaultVal;
            return (T)val;
        }

        public static bool TryGet<T>(this TomlTable table, string key, out T? output)
        {
            output = default;
            var val = table[key];
            if (val == null || val.GetType() != typeof(T))
                return false;
            output = (T)val;
            return true;
        }
    }
}
