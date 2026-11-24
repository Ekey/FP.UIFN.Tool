using FP.Fonts.Cryptor.FileSystem.Compression;
using FP.Fonts.Cryptor.FileSystem.Encryption;

namespace FP.Fonts.Cryptor.FileSystem.Package
{
    internal class FontDecrypt
    {
        public static void iDoIt(String m_SrcFile, String m_DstFile)
        {
            if (!File.Exists(m_SrcFile))
            {
                Utils.iSetError("[ERROR]: Input file -> " + m_SrcFile + " <- does not exist");
                return;
            }

            using (FileStream TFileStream = File.OpenRead(m_SrcFile))
            {
                var m_Header = new SectionBinaryHeader();

                m_Header.dwMagic = TFileStream.ReadUInt32();

                if (m_Header.dwMagic != 0x42444553)
                {
                    throw new Exception("[ERROR]: Invalid magic of Section Binary file!");
                }

                m_Header.dwSubMagic = TFileStream.ReadUInt32();

                if (m_Header.dwSubMagic != 0x6E666975)
                {
                    throw new Exception("[ERROR]: Invalid submagic of Section Binary file!");
                }

                m_Header.dwBinaryVersion = TFileStream.ReadUInt32();

                if (m_Header.dwBinaryVersion != 1)
                {
                    throw new Exception("[ERROR]: Invalid Binary Version!");
                }

                m_Header.bEndianType = (BinaryEndianType)TFileStream.ReadByte();
                m_Header.bAlignmentBits = TFileStream.ReadByte();
                m_Header.sOffset = TFileStream.ReadUInt16();
                m_Header.dwSize = TFileStream.ReadUInt64();
                m_Header.dwDateTime = TFileStream.ReadUInt64();
                m_Header.lpGuid = TFileStream.ReadBytes(16);

                var lpSrcBuffer = TFileStream.ReadBytes((Int32)TFileStream.Length - 48);
                Cipher.iDecryptData(lpSrcBuffer);

                var lpDstBuffer = LZSS.iDecompress(lpSrcBuffer);

                File.WriteAllBytes(m_DstFile, lpDstBuffer);

                TFileStream.Dispose();
            }
        }
    }
}
