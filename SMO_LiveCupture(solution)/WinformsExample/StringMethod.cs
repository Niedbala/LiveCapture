using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformsExample
{
    public static class StringsMethod
    {
        public static string Map_Series(this string source)
        {
            string[] split_key = source.ToString().Split('_');

            var name = split_key[split_key.Count() - 1];

            if (name.Contains("AnalogIn")) { name = name + "_" + split_key[1]; }
            if (name.Contains("Analog")) { name = name + "_" + split_key[4]; }
            if (name.Split('_').Last() == "B") { name = name + "_" + split_key[5]; }
            // var new_name = "";
            return name;
        }
    }
}
