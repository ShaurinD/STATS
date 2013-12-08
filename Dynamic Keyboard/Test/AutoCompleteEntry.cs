using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class AutoCompleteEntry
    {
        public string value;
        public int count;
        public AutoCompleteEntry(string val)
        {
            value = val;
            count = 1;
        }
    }
}
