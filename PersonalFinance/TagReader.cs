using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersonalFinance
{
    static class TagReader
    {
        public static string GetParameter(string tag, string key)
        {
            return tag.Split(';').Select(x =>
            {
                var a = x.Split('=');
                return (key: a[0], value: a[1]);
            })
            .FirstOrDefault(x => x.key == key).value;
        }
    }
}
