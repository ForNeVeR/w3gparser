using System.Collections.Generic;
using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gGameSetting
    {
        byte[] options;
        string creator;
        W3gMap map;

        public W3gMap Map
        {
            get { return map; }
        }

        public static W3gGameSetting Parse(BinaryReader reader)
        {
            byte[] decoded = GetDecodedBytes(reader);

            W3gGameSetting s = new W3gGameSetting();
            s.options = new byte[9];
            for (int i = 0; i < 9; i++)
                s.options[i] = decoded[i];

            //uint checksum = BinaryHelper.GetUInt32(decoded, 9);
            uint checksum = decoded[9];
            checksum |= (uint)decoded[10] << 8;
            checksum |= (uint)decoded[11] << 16;
            checksum |= (uint)decoded[12] << 24;

            string path = BinaryHelper.GetString(decoded, 13);
            s.map = new W3gMap(checksum, path);

            s.creator = BinaryHelper.GetString(decoded, 13 + path.Length + 1);
            return s;
        }

        private static byte[] GetDecodedBytes(BinaryReader reader)
        {
            List<byte> decoded = new List<byte>();
            int pos = 0;
            byte mask = 0;

            byte b = reader.ReadByte();
            while (b != 0)
            {
                if (pos % 8 == 0)
                {
                    mask = b;
                }
                else
                {
                    if ((mask & (0x1 << (pos % 8))) == 0)
                        decoded.Add((byte)(b - 1));
                    else
                        decoded.Add(b);
                }

                b = reader.ReadByte();
                pos++;
            }
            return decoded.ToArray();
        }
    }

    public enum GameSpeed : byte
    {
        Slow = 0,
        Normal = 1,
        Fast = 2,
        Unused = 3,
    }
}
#region 4.3 [Encoded String]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//There are the GameSetting and three null-terminated strings here back-to-back,
//all encoded into a single null terminated string.
//(But we don't know the reason for this encoding!)

//Every even byte-value was incremented by 1. So all encoded bytes are odd.
//A control-byte stores the transformations for the next 7 bytes.

//Since all NullBytes were transformed to 1, they will never occure inside the
//encoded string. But a NullByte marks the end of the encoded string.

//The encoded string starts with a control byte.
//The control byte holds a bitfield with one bit for each byte of the next 7
//bytes block. Bit 1 (not Bit 0) corresponds to the following byte right after
//the control-byte, bit 2 to the next, and so on.
//Only Bit 1-7 contribute to encoded string. Bit 0 is unused and always set.

//Decoding these bytes works as follows:

//If the corresponding bit is a '1' then the character is moved over directly.
//If the corresponding bit is a '0' then subtract 1 from the character.

//After a control-byte and the belonging 7 bytes follows a new control-byte
//until you find a NULL character in the stream.


//Example decompression code (in 'C'):

//char* EncodedString;
//char* DecodedString;
//char  mask;
//int   pos=0;
//int   dpos=0;

//while (EncodedString[pos] != 0)
//{
//  if (pos%8 == 0) mask=EncodedString[pos];
//  else
//  {
//    if ((mask & (0x1 << (pos%8))) == 0)
//      DecodedString[dpos++] = EncodedString[pos] - 1;
//    else
//      DecodedString[dpos++] = EncodedString[pos];
//  }
//  pos++;
//}


//Alternatively one could interprete the encoding scheme as follow:
//Bit 0 of every character was moved to the control byte and set to 1 afterwards.

// Maybe this gives a simpler decoding algorithm.
#endregion

#region 4.4 [GameSettings]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//Make sure you have decoded the GameSettings (see 4.3).

//The game settings (extended options on create game screen) are packed using
//various flags distributed over 13 bytes.
//For details about the single options read the file
// "support/Readme/(PC)UIMainMenus.html"
//in your WarCraft III installation directory.

//Denoted below are only nonzero flags.

//offset | bitnr | Description
//-------+-------+---------------------------------------------------------------
//0x0000 |  0,1  | Game Speed: 0 = slow, 1 = normal, 2 = fast, 3 = unused
//-------+-------+---------------------------------------------------------------
//0x0001 |   0   | Visibility: 'hide terrain'
//       |   1   | Visibility: 'map explored'
//       |   2   | Visibility: 'always visible' (no fog of war)
//       |   3   | Visibility: 'default'
//       |  4,5  | Observer  : 0 = off or 'Referees' (see 0x0003 Bit6)
//       |       |             1 = unused
//       |       |             2 = 'Obs on Defeat'
//       |       |             3 = on or 'Referees'
//       |   6   | Teams Together (team members are placed at neighbored places)
//-------+-------+---------------------------------------------------------------
//0x0002 |  1,2  | Fixed teams: 0 = off, 1 = unused, 2 = unused, 3 = on
//-------+-------+---------------------------------------------------------------
//0x0003 |   0   | Full Shared Unit Control
//       |   1   | Random Hero
//       |   2   | Random Races
//       |   6   | Observer: Referees (other observer bits are 0 or 3)
//-------+-------+---------------------------------------------------------------
//0x0004 |       | 0
//0x0005 |       | unknown (0 in ladder games, but not in custom)
//0x0006 |       | 0
//0x0007 |       | unknown (0 in ladder games, but not in custom)
//0x0008 |       | 0
//-------+-------+---------------------------------------------------------------
//0x0009 | 4Byte | Map Checksum  
// -  0C |       |
//-------+-------+---------------------------------------------------------------
#endregion

#region 4.5 [Map&CreatorName]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//Make sure you have decoded the three Strings (see 4.3).

//First is the map name, second is the game creators name (can be "Battle.Net"
//for ladder) and a third is an always empty string.

//Here ends the encoded string. There should not be any unprocessed decoded
//bytes left.
#endregion