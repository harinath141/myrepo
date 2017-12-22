using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR
{
    class CharChopper
    {
        public string trim(string s,IEnumerable<char> chars)
        {
            
            string removeChars = string.Concat(chars);

            var stripped = from c in s.ToCharArray()
                           where !removeChars.Contains(c)
                           select c;

            return new string(stripped.ToArray());
        }

        public IEnumerable<char> StringAsEnumerable(string s)
        {
            foreach (char c in s)
            {
                yield return c;
            }
        }
    }
}
