using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace nafsplit
{
    public class Vertices
    {
        public long Offset { get; set; }
        public List<Vertex> Items { get; set; }
        public string Filename { get; internal set; }

        public Vertices()
        {
            Items = new List<Vertex>();
        }
    }
}
