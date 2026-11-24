using System;
using System.Buffers.Binary;

namespace FP.Fonts.Cryptor.FileSystem.Compression
{
    internal class LZSS
    {
        private const Int32 WINDOW_SIZE = 4096;
        private const Int32 MAX_MATCH = 18;
        private const Int32 MIN_MATCH = 3;
        private const Int32 HASH_SIZE = 4096;
        private const Int32 MAX_CHAIN = 64;

        private static Int32 iGetHash(ReadOnlySpan<Byte> lpBuffer, Int32 dwOffset)
        {
            if (dwOffset + 2 >= lpBuffer.Length)
                return -1;

            return ((lpBuffer[dwOffset] << 4) ^ (lpBuffer[dwOffset + 1] << 2) ^ lpBuffer[dwOffset + 2]) & (HASH_SIZE - 1);
        }

        public static Byte[] iCompress(ReadOnlySpan<Byte> lpSrcBuffer)
        {
            if (lpSrcBuffer.IsEmpty)
                return Array.Empty<Byte>();

            var output = new List<Byte>(lpSrcBuffer.Length / 2 + 16);

            for (Int32 i = 0; i < 8; i++)
                output.Add(0);

            Int32[] head = new Int32[HASH_SIZE];
            Int32[] prev = new Int32[WINDOW_SIZE];

            Array.Fill(head, -1);
            Array.Fill(prev, -1);

            Byte[] codeBuf = new Byte[17];
            Byte mask = 1;

            Int32 codeBufPtr = 1;
            codeBuf[0] = 0;

            Int32 dwSrcPos = 0;

            while (dwSrcPos < lpSrcBuffer.Length)
            {
                Int32 bestLen = 0;
                Int32 bestOffset = 0;

                Int32 dwMaxLen = Math.Min(MAX_MATCH, lpSrcBuffer.Length - dwSrcPos);
                Int32 dwHash = iGetHash(lpSrcBuffer, dwSrcPos);

                if (dwHash >= 0 && dwMaxLen >= MIN_MATCH && dwSrcPos > 0)
                {
                    Int32 checkPos = head[dwHash];
                    Int32 chainCount = 0;

                    while (checkPos != -1 && chainCount < MAX_CHAIN)
                    {
                        Int32 offset = dwSrcPos - checkPos;

                        if (offset >= WINDOW_SIZE)
                            break;

                        if (offset > 0 && offset <= dwSrcPos)
                        {
                            Int32 matchLen = 0;

                            while (matchLen < dwMaxLen && lpSrcBuffer[checkPos + matchLen] == lpSrcBuffer[dwSrcPos + matchLen])
                            {
                                matchLen++;
                            }

                            if (matchLen > bestLen)
                            {
                                bestLen = matchLen;
                                bestOffset = offset;
                                if (bestLen == dwMaxLen)
                                    break;
                            }
                        }

                        checkPos = prev[checkPos & (WINDOW_SIZE - 1)];
                        chainCount++;
                    }
                }

                if (bestLen < MIN_MATCH || bestOffset <= 0 || bestOffset > dwSrcPos)
                {
                    bestLen = 1;
                    codeBuf[0] |= mask;
                    codeBuf[codeBufPtr++] = lpSrcBuffer[dwSrcPos];
                }
                else
                {
                    Int32 rawPos = (dwSrcPos - bestOffset - 0x12) & 0xFFF;

                    Byte b0 = (Byte)(rawPos & 0xFF);
                    Byte b1 = (Byte)(((rawPos >> 4) & 0xF0) | ((bestLen - 3) & 0x0F));

                    codeBuf[codeBufPtr++] = b0;
                    codeBuf[codeBufPtr++] = b1;
                }

                mask <<= 1;

                if (mask == 0)
                {
                    for (Int32 i = 0; i < codeBufPtr; i++)
                        output.Add(codeBuf[i]);

                    codeBuf[0] = 0;
                    codeBufPtr = 1;
                    mask = 1;
                }

                for (Int32 i = 0; i < bestLen; i++)
                {
                    Int32 h = iGetHash(lpSrcBuffer, dwSrcPos);

                    if (h >= 0)
                    {
                        prev[dwSrcPos & (WINDOW_SIZE - 1)] = head[h];
                        head[h] = dwSrcPos;
                    }

                    dwSrcPos++;
                }
            }

            if (codeBufPtr > 1)
            {
                for (Int32 i = 0; i < codeBufPtr; i++)
                    output.Add(codeBuf[i]);
            }

            Byte[] lpDstBuffer = output.ToArray();

            Span<Byte> headerSpan = lpDstBuffer.AsSpan(0, 8);

            BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(0, 4), (UInt32)lpDstBuffer.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(headerSpan.Slice(4, 4), (UInt32)lpSrcBuffer.Length);

            return lpDstBuffer;
        }

        public static Byte[] iDecompress(ReadOnlySpan<Byte> lpSrcBuffer)
        {
            if (lpSrcBuffer.Length < 9)
                return Array.Empty<Byte>();

            UInt32 dwCompressedSize = BinaryPrimitives.ReadUInt32LittleEndian(lpSrcBuffer.Slice(0, 4));
            UInt32 dwDecompressedSize = BinaryPrimitives.ReadUInt32LittleEndian(lpSrcBuffer.Slice(4, 4));

            Int32 dwEnd = (Int32)Math.Min(dwCompressedSize, (UInt32)lpSrcBuffer.Length);

            if (dwEnd < 9)
                return Array.Empty<Byte>();

            Byte[] lpDstBuffer = new Byte[dwDecompressedSize];

            Int32 dwInPos = 8;
            Int32 dwOutPos = 0;

            Int32 dwFlags = 0;
            Int32 dwBitsLeft = 0;

            while (dwInPos < dwEnd && dwOutPos < lpDstBuffer.Length)
            {
                if (dwBitsLeft == 0)
                {
                    dwFlags = lpSrcBuffer[dwInPos++];
                    dwBitsLeft = 8;

                    if (dwInPos >= dwEnd)
                        break;
                }

                if ((dwFlags & 1) != 0)
                {
                    lpDstBuffer[dwOutPos++] = lpSrcBuffer[dwInPos++];
                }
                else
                {
                    if (dwInPos + 1 >= dwEnd)
                        break;

                    Byte b0 = lpSrcBuffer[dwInPos++];
                    Byte b1 = lpSrcBuffer[dwInPos++];

                    Int32 dwLength = (b1 & 0x0F) + 3;
                    Int32 dwOffset = (dwOutPos - (b0 | ((b1 & 0xF0) << 4)) - 0x12) & 0xFFF;

                    for (Int32 i = 0; i < dwLength && dwOutPos < lpDstBuffer.Length; i++)
                    {
                        Int32 refPos = dwOutPos - dwOffset;

                        if (refPos < 0)
                        {
                            lpDstBuffer[dwOutPos++] = 0;
                        }
                        else
                        {
                            lpDstBuffer[dwOutPos++] = lpDstBuffer[refPos];
                        }
                    }
                }

                dwFlags >>= 1;
                dwBitsLeft--;
            }

            return lpDstBuffer;
        }
    }
}
