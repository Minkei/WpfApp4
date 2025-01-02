using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Helper
{
    public class Mediator
    {
        private static readonly Dictionary<string, object> Data = new();
        public static void Register(string key, object value)
        {
            if (Data.ContainsKey(key))
                Data[key] = value;
            else
                Data.Add(key, value);
        }
        public static T? Get<T>(string key)
        {
            if (Data.TryGetValue(key, out var value))
                return (T)value;
            return default;
        }
    }

}
