using System.Xml.Serialization;

namespace pcxbdsplit
{
    public class PCXBDEntry
    {
        public short EntryNumber { get; set; }
        public int TypeNumber { get; set; }
        public string EntryType { get; set; }
        public short UnknownShort1 { get; set; }
        public short UnknownShort2 { get; set; }
        public int Length { get; set; }

        public string FileName { get; set; }
        [XmlIgnore]
        public byte[] Content { get; set; }
    }
}