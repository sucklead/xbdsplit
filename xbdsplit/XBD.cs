using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xbdsplit
{
    public class XBD
    {
        public int Version { get; set; }
        public int NumberOfChunks { get; set; }
        public List<XBDChunk> Chunks { get; set; }
    }
}
