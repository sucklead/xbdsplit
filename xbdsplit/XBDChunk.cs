using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace xbdsplit
{
    public class XBDChunk
    {
        public string ChunkType { get; set; }
        //public int Length { get; set; }
        public string FileName { get; set; }
        [XmlIgnore]
        public byte[] Content { get; set; } 
    }
}
