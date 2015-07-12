using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lngsplit
{
    public class Language
    {
        public int NameLength { get; set; }
        public string Name { get; set; }
        public short NumberOfEntries { get; set; }
        public List<Entry> Entries { get; set; }
        public short NumberOfIdValuePairs { get; set; }
        public List<IdValuePair> IdValuePairs { get; set; }
    }
}
