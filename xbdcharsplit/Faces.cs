using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace xbdcharsplit
{
    public class Faces
    {
        public long Offset { get; set; }
        public ushort Submesh { get; set; }
        public List<Face> Items { get; set; }
        public ushort HighestValue { get; set; }
        public ushort HighestVertex { get; set; }
        public string Filename { get; internal set; }

        public Faces()
        {
            Items = new List<Face>();
        }
    }
}
