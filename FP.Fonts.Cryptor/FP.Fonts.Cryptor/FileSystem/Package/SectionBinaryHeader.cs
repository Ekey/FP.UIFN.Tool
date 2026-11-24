using System;
using System.Collections.Generic;
using System.Text;

namespace FP.Fonts.Cryptor.FileSystem.Package
{
    //Based on https://github.com/Kizari/Flagrum/blob/main/src/Flagrum.Core/Data/Binary/SectionDataBinary.cs

    public enum BinaryEndianType : byte
    {
        LittleEndian = 0,
        BigEndian = 1

    }

    internal class SectionBinaryHeader
    {
        public UInt32 dwMagic { get; set; } // SEDB
        public UInt32 dwSubMagic { get; set; } // uifn
        public UInt32 dwBinaryVersion { get; set; } // 1
        public BinaryEndianType bEndianType { get; set; } // 0
        public Int32 bAlignmentBits { get; set; } // 0 (Byte)
        public UInt16 sOffset { get; set; } // 48
        public UInt64 dwSize { get; set; }
        public UInt64 dwDateTime { get; set; }
        public Byte[] lpGuid { get; set; } // 20 bytes
    }
}
