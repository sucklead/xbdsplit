using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace xbdcharsplit
{
    public class Character
    {
        public string CharacterName { get; set; }
        public int Length { get; set; }
        public string FileName { get; set; }
        public List<Faces> FacesList { get; set; }
        public List<Vertices> VerticesList { get; set; }
        [XmlIgnore]
        public byte[] Content { get; set; }

        public Character()
        {
            FacesList = new List<Faces>();
            VerticesList = new List<Vertices>();
        }
    }
}
