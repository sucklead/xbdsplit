using System.Collections.Generic;
using System.Xml.Serialization;

namespace nafsplit
{
    public class Model
    {
        public string ModelName { get; set; }
        public int Length { get; set; }
        public string FileName { get; set; }
        public List<Faces> FacesList { get; set; }
        public List<Vertices> VerticesList { get; set; }
        [XmlIgnore]
        public byte[] Content { get; set; }

        public Model()
        {
            FacesList = new List<Faces>();
            VerticesList = new List<Vertices>();
        }
    }
}