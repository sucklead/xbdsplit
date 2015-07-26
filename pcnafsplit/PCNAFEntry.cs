using System.Xml.Serialization;

namespace pcnafsplit
{
    public class PCNAFEntry
    {
        [XmlIgnore]
        public short EntryNumber { get; set; }
        public string EntryType { get; set; }
        [XmlIgnore]
        public int Length { get; set; }

        public string FileName { get; set; }
        [XmlIgnore]
        public byte[] Content { get; set; }
    }
}