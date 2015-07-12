using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lngsplit
{
    public class LNGL
    {
        public int Version { get; set; }
        public short NumberOfLanguages { get; set; }
        public List<Language> Languages { get; set; }
    }
}
