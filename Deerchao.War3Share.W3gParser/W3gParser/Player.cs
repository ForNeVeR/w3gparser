using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gPlayer
    {
        byte recordId;
        byte playerId;
        string playerName;
        GameType gameType;
        uint playTime;
        Race race;

        public GameType GameType
        {
            get { return gameType; }
        }

        public byte PlayerId
        {
            get { return playerId; }
        }

        public string PlayerName
        {
            get { return playerName; }
        }

        public bool IsHost
        {
            get { return recordId == 0x00; }
        }

        public Race Race
        {
            get { return race; }
        }

        public uint PlayTime
        {
            get { return playTime; }
        }

        public override string ToString()
        {
            return PlayerName;
        }

        public static W3gPlayer Parse(BinaryReader reader)
        {
            W3gPlayer p = new W3gPlayer();
            p.recordId = reader.ReadByte();
            p.playerId = reader.ReadByte();
            p.playerName = BinaryHelper.ReadSZString(reader);
            p.gameType = (GameType)reader.ReadByte();

            if (p.gameType == GameType.Custom)
            {
                reader.ReadByte();
            }
            else if (p.gameType == GameType.Ladder)
            {
                p.playTime = reader.ReadUInt32();
                p.race = (Race)reader.ReadUInt32();
            }
            return p;
        }

        public static List<W3gPlayer> ParsePlayers(BinaryReader reader)
        {
            List<W3gPlayer> result = new List<W3gPlayer>();
            while (reader.PeekChar() == 0x16)
            {
                result.Add(Parse(reader));
                reader.ReadBytes(4);
            }
            return result;
        }
    }

    public enum Race : uint
    {
        Human = 0x01,
        Orc = 0x02,
        NightElf = 0x04,
        Undead = 0x08,
        Daemon = 0x10,
        Random = 0x20,
        Selectable = 0x40,
    }

    public enum GameType : byte
    {
        Custom = 0x01,
        Ladder = 0x08,
    }
}
#region 4.1 [PlayerRecord]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1 byte   | RecordID:
//       |           |  0x00 for game host
//       |           |  0x16 for additional players (see 4.9)
//0x0001 |  1 byte   | PlayerID
//0x0002 |  n bytes  | PlayerName (null terminated string)
//   n+2 |  1 byte   | size of additional data:
//       |           |  0x01 = custom
//       |           |  0x08 = ladder

//Depending on the game type one of these records follows:

//o For custom games:

//  offset | size/type | Description
//  -------+-----------+---------------------------------------------------------
//  0x0000 | 1 byte    | null byte (1 byte)

//o For ladder games:

//  offset | size/type | Description
//  -------+-----------+---------------------------------------------------------
//  0x0000 | 4 bytes   | runtime of players Warcraft.exe in milliseconds
//  0x0004 | 4 bytes   | player race flags:
//         |           |   0x01=human
//         |           |   0x02=orc
//         |           |   0x04=nightelf
//         |           |   0x08=undead
//         |           |  (0x10=daemon)
//         |           |   0x20=random
//         |           |   0x40=race selectable/fixed (see notes in section 4.11)

#endregion

#region 4.9 [PlayerList]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//The player list is an array of PlayerRecords for all additional players
//(excluding the game host and any computer players).
//If there is only one human player in the game it is not present at all!
//Per additional player there is the following structure in the file:

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 | 4/11 byte | PlayerRecord (see 4.1)
//0x000? |    4 byte | unknown
//       |           |  (always 0x00000000 for patch version >= 1.07
//       |           |   always 0x00000001 for patch version <= 1.06)

//This record is repeated as long as the first byte equals the additional
//player record ID (0x16).

#endregion