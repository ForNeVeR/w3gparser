using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deerchao.War3Share.W3gParser
{
    internal class W3gSubHeader
    {
        //"WAR3" for ROC, "W3XP" for TFT
        string w3string;
        int minorVersion;
        short buildNo;
        ushort gameFlags;
        //in ms
        int replayLength;
        uint checkSum;

        public int ReplayLength
        {
            get { return replayLength; }
        }

        public int MinorVersion
        {
            get { return minorVersion; }
        }

        public short BuildNo
        {
            get { return buildNo; }
        }

        public bool IsSingleUserGame
        {
            get { return gameFlags == 0x0000; }
        }

        public static W3gSubHeader Parse(BinaryReader reader)
        {
            W3gSubHeader result = new W3gSubHeader();
            result.w3string = GetW3Version(reader.ReadBytes(4));
            result.minorVersion = reader.ReadInt32();
            result.buildNo = reader.ReadInt16();
            result.gameFlags = reader.ReadUInt16();
            result.replayLength = reader.ReadInt32();
            result.checkSum = reader.ReadUInt32();
            return result;
        }

        private static string GetW3Version(byte[] ver)
        {
            string result = "";
            for (int i = 3; i >= 0; i--)
            {
                result += (char)ver[i];
            }

            return result;
        }
    }
}
#region 2.1 [SubHeader] for header version 0
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//This header was used for all replays saved with WarCraft III patch version
//v1.06 and below.

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1  word  | unknown (always zero so far)
//0x0002 |  1  word  | version number (corresponds to patch 1.xx)
//0x0004 |  1  word  | build number (see section 2.3)
//0x0006 |  1  word  | flags
//       |           |   0x0000 for single player games
//       |           |   0x8000 for multiplayer games (LAN or Battle.net)
//0x0008 |  1 dword  | replay length in msec
//0x000C |  1 dword  | CRC32 checksum for the header
//       |           | (the checksum is calculated for the complete header
//       |           |  including this field which is set to zero)

//Overall header size for version 0 is 0x40 bytes.
#endregion

#region 2.2 [SubHeader] for header version 1
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//This header is used for all replays saved with WarCraft III patch version
//v1.07 and above.

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1 dword  | version identifier string reading:
//       |           |  'WAR3' for WarCraft III Classic
//       |           |  'W3XP' for WarCraft III Expansion Set 'The Frozen Throne'
//       |           | (note that this string is saved in little endian format
//       |           |  in the replay file)
//0x0004 |  1 dword  | version number (corresponds to patch 1.xx so far)
//0x0008 |  1  word  | build number (see section 2.3)
//0x000A |  1  word  | flags
//       |           |   0x0000 for single player games
//       |           |   0x8000 for multiplayer games (LAN or Battle.net)
//0x000C |  1 dword  | replay length in msec
//0x0010 |  1 dword  | CRC32 checksum for the header
//       |           | (the checksum is calculated for the complete header
//       |           |  including this field which is set to zero)

//Overall header size for version 1 is 0x44 bytes.
#endregion

#region 2.3 Version information
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

// game version | version in replay | version of war3.exe |  release date
//--------------+-------------------+---------------------+----------------
//    1.00      |     1.00.4448     |      1.0. 0.4448    |   2002-07-03
//    1.01      |     1.01.4482     |      1.0. 1.4482    |   2002-07-05
//    1.01b     |     1.01.4482     |      1.0. 1.4483    |   2002-07-10
//    1.01c     |     1.01.4482     |                ?    |   2002-07-28
//    1.02      |     1.02.4531     |      1.0. 1.4531    |   2002-08-15
//    1.02a     |     1.02.4531     |      1.0. 1.4563    |   2002-09-06
//    1.03      |     1.03.4572     |      1.0. 3.4653    |   2002-10-09
//    1.04      |     1.04.4654     |      1.0. 3.4709    |   2002-11-04
//    1.04b     |     1.04.4654     |      1.0. 3.4709    |   2002-11-07
//    1.04c     |     1.04.4654     |      1.0. 4.4905    |   2003-01-30
//    1.05      |     1.05.4654     |      1.0. 5.4944    |   2003-01-30
//    1.06      |     1.06.4656     |      1.0. 6.5551    |   2003-06-03
//    1.07      |     1.07.6031     |      1.0. 7.5535    |   2003-07-01
//    1.10      |     1.10.6034     |      1.0.10.5610    |   2003-06-30
//    1.11      |     1.11.6035     |      1.0.11.5616    |   2003-07-15
//    1.12      |     1.12.6036     |      1.0.12.5636    |   2003-07-31
//    1.13      |     1.13.6037     |      1.0.13.5816    |   2003-12-16
//    1.13b     |     1.13.6037     |      1.0.13.5818    |   2003-12-19
//    1.14      |     1.14.6039     |      1.0.14.5840    |   2004-01-07
//    1.14b     |     1.14.6040     |      1.0.14.5846    |   2004-01-10
//    1.15      |     1.15.6043     |      1.0.15.5917    |   2004-04-14
//    1.16      |     1.16.6046     |      1.0.16.5926    |   2004-05-10
//    1.17      |     1.17.6050     |      1.0.17.5988    |   2004-09-20
//    1.18      |     1.18.6051     |      1.0.18.6030    |   2005-03-01


// Notes on specific patches:
//  o The mpq file for patch 1.02a is named 1.02c.
//  o Patch 1.04b was only available as standalone patch (not on bnet)
//    and solely adds a new copy protection to war3.exe.
//  o The minor version number of the 'war3.exe' is wrong for patches
//    1.02, 1.02a, 1.04, 1.04b.
//  o Blizzard released no standalone versions of the patches:
//    1.01c, 1.04c, 1.10.
//  o Replays of patch 1.14 and 1.14b are incompatible

// General notes:
//  o There are no differences in replays between minor versions
//    (except for patch 1.14 and 1.14b).
//  o Check the file properties of the existing war3.exe to get the current
//    installed version of Warcraft III (see column 3).
//  o There are no differences in version and build numbers between RoC and TFT.
//  o There are no differences between various language versions.
//  o You can identify the replay version of the installed game by extracting
//    the 'product version number' from the version resource of the 'war3.exe'.
//  o You can identify the version of the 'war3.exe' by extracting the
//    'file version number' from the version resource of the file.
//  o On early patches build number of replays and 'war3.exe' are equal.
//    Later on they differ.

#endregion