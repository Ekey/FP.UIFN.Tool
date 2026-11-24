using System;
using System.Collections.Generic;
using System.Text;

namespace FP.Fonts.Cryptor.FileSystem.Encryption
{
    internal class Cipher
    {
        private static UInt32[] iGenerateIndices(UInt32 dwSize)
        {
            UInt32[] m_Indices = new UInt32[dwSize];

            UInt32 A = 1812433253U * (dwSize ^ (dwSize >> 30)) + 1;
            UInt32 B = 1812433253U * (A ^ (A >> 30)) + 2;
            UInt32 C = 1812433253U * (B ^ (B >> 30)) + 3;
            UInt32 D = 1812433253U * (C ^ (C >> 30)) + 4;

            for (UInt32 i = 0; i < dwSize; i++)
            {
                UInt32 t = A ^ (A << 11);
                A = B;
                B = C;
                C = D;
                D = (D ^ (D >> 19)) ^ (t ^ (t >> 8));
                m_Indices[i] = D % dwSize;
            }

            return m_Indices;
        }

        public static void iDecryptData(Span<Byte> lpBuffer)
        {
            UInt32 dwSize = (UInt32)lpBuffer.Length & 0x7FFFFFFF;
            
            if (dwSize <= 1)
                return;

            UInt32[] m_Indices = iGenerateIndices(dwSize);

            for (Int32 i = (Int32)dwSize - 1; i > 0; i--)
            {
                Int32 j = (Int32)m_Indices[i];
                (lpBuffer[i], lpBuffer[j]) = (lpBuffer[j], lpBuffer[i]);
            }
        }

        public static void iEncryptData(Span<Byte> lpBuffer)
        {
            UInt32 dwSize = (UInt32)lpBuffer.Length & 0x7FFFFFFF;
            
            if (dwSize <= 1)
                return;

            UInt32[] m_Indices = iGenerateIndices(dwSize);

            for (Int32 i = 1; i < (Int32)dwSize; i++)
            {
                Int32 j = (Int32)m_Indices[i];
                (lpBuffer[i], lpBuffer[j]) = (lpBuffer[j], lpBuffer[i]);
            }
        }
    }
}
