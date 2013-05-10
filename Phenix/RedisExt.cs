using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phenix
{
    class RedisExt
    {
        static public string GetString(byte[] stringBytes)
        {
            return Encoding.UTF8.GetString(stringBytes);
        }
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static List<string> ToStringList(byte[][] multiDataList)
        {
            if (multiDataList == null)
                return new List<string>();

            var results = new List<string>();
            foreach (var multiData in multiDataList)
            {
                results.Add(GetString(multiData));
            }
            return results;
        }
    }
}
