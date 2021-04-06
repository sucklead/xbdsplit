using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nafsplit
{
    public class Face
    {
        public ushort Index1 { get; set; }
        public ushort Index2 { get; set; }
        public ushort Index3 { get; set; }
        public ushort FaceIndex { get; internal set; }
    }
}
