using FP.Fonts.Cryptor.FileSystem.Compression;
using FP.Fonts.Cryptor.FileSystem.Encryption;

using System.Buffers.Binary;

namespace FP.Fonts.Cryptor.FileSystem.Package
{
    internal class FontEncrypt
    {
        public static void iDoIt(String m_SrcFile, String m_DstFile)
        {
            if (!File.Exists(m_SrcFile))
            {
                Utils.iSetError("[ERROR]: Input file -> " + m_SrcFile + " <- does not exist");
                return;
            }

            var lpSrcBuffer = File.ReadAllBytes(m_SrcFile);

            var lpTemp = LZSS.iCompress(lpSrcBuffer);

            Cipher.iEncryptData(lpTemp);

            Byte[] lpDstBuffer = new Byte[lpTemp.Length + 48];

            var m_Header = new SectionBinaryHeader
            {
                dwMagic = 0x42444553,
                dwSubMagic = 0x6E666975,
                dwBinaryVersion = 1,
                bEndianType = 0,
                bAlignmentBits = 0,
                sOffset = 48,
                dwSize = (UInt64)lpDstBuffer.Length,
                dwDateTime = 0,
                lpGuid = new Byte[16],
            };

            Span<Byte> lpHeaderSpan = lpDstBuffer.AsSpan(0, 48);

            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(0, 4), m_Header.dwMagic);
            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(4, 4), m_Header.dwSubMagic);
            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(8, 4), m_Header.dwBinaryVersion);

            UInt32 packedMeta = (UInt32)((Byte)m_Header.bEndianType | (m_Header.bAlignmentBits << 8) | (m_Header.sOffset << 16));

            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(12, 4), packedMeta);

            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(16, 4), (UInt32)(m_Header.dwSize & 0xFFFFFFFF));
            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(20, 4), (UInt32)(m_Header.dwSize >> 32));

            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(24, 4), (UInt32)(m_Header.dwDateTime & 0xFFFFFFFF));
            BinaryPrimitives.WriteUInt32LittleEndian(lpHeaderSpan.Slice(28, 4), (UInt32)(m_Header.dwDateTime >> 32));

            Buffer.BlockCopy(lpTemp, 0, lpDstBuffer, 48, lpTemp.Length);

            File.WriteAllBytes(m_DstFile, lpDstBuffer);
        }
    }
}
