using System;
using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    internal class W3gHeader
    {
        static readonly string HeaderString = "Warcraft III recorded game\u001A\0";

        int headerSize;
        int dataSizec;
        int versionFlag;
        int dataSize;
        int nBlocks;
        W3gSubHeader subHeader;

        internal int HeaderSize
        {
            get { return headerSize; }
        }

        internal int NBlocks
        {
            get { return nBlocks; }
        }
        internal int MinorVersion
        {
            get { return subHeader.MinorVersion; }
        }
        internal short BuildNo
        {
            get { return subHeader.BuildNo; }
        }
        internal TimeSpan GameLength
        {
            get
            {
                return new TimeSpan(0, 0, 0, 0, subHeader.ReplayLength);
            }
        }

        public static W3gHeader Parse(BinaryReader reader)
        {
            ValidateHeaderString(reader.ReadBytes(28));

            W3gHeader result = new W3gHeader();
            result.headerSize = reader.ReadInt32();
            result.dataSizec = reader.ReadInt32();
            result.versionFlag = reader.ReadInt32();
            result.dataSize = reader.ReadInt32();
            result.nBlocks = reader.ReadInt32();

            if (result.versionFlag == 0)
            {
                throw new ParserException("暂不支持1.06及更低版本录像。");
            }
            else if (result.versionFlag == 1)
            {
               result.subHeader = W3gSubHeader.Parse(reader);
            }

            return result;
        }

        private static void ValidateHeaderString(byte[] header)
        {
            for (int i = 0; i < 28; i++)
            {
                if (HeaderString[i] != (char)header[i])
                    throw new ParserException("指定的文件不是合法的Warcraft III Replay文件。");
            }
        }
    }
}
#region Header
//2.0 [Header]
//===============================================================================

//The replay file consist of a header followed by a variable number of compressed
//data blocks. The header has the following format:

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 | 28 chars  | zero terminated string "Warcraft III recorded game\0x1A\0"
//0x001c |  1 dword  | fileoffset of first compressed data block (header size)
//       |           |  0x40 for WarCraft III with patch <= v1.06
//       |           |  0x44 for WarCraft III patch >= 1.07 and TFT replays
//0x0020 |  1 dword  | overall size of compressed file
//0x0024 |  1 dword  | replay header version:
//       |           |  0x00 for WarCraft III with patch <= 1.06
//       |           |  0x01 for WarCraft III patch >= 1.07 and TFT replays
//0x0028 |  1 dword  | overall size of decompressed data (excluding header)
//0x002c |  1 dword  | number of compressed data blocks in file
//0x0030 |  n bytes  | SubHeader (see section 2.1 and 2.2)

//The size of the header excluding the subheader is 0x30 bytes so far.

#endregion