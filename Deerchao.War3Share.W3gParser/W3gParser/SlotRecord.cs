using System.IO;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gSlotRecord
    {
        byte playerId;
        byte mapDownloadPercent;
        SlotStatus slotStatus;
        bool isComputer;
        byte teamNo;
        PlayerColor color;
        Race race;
        AIStrength strength;
        byte hpPercent;

        public byte HpPercent
        {
            get { return hpPercent; }
        }

        public byte PlayerId
        {
            get { return playerId; }
        }

        public bool IsComputer
        {
            get { return isComputer; }
        }

        public byte TeamNo
        {
            get { return teamNo; }
        }

        public PlayerColor Color
        {
            get { return color; }
        }

        public Race Race
        {
            get { return race; }
        }

        public AIStrength Strength
        {
            get { return strength; }
        }

        public SlotStatus SlotStatus
        {
            get { return slotStatus; }
        }

        public byte MapDownloadPercent
        {
            get { return mapDownloadPercent; }
        }

        public override string ToString()
        {
            return PlayerId.ToString();
        }

        public static W3gSlotRecord Parse(BinaryReader reader)
        {
            W3gSlotRecord s = new W3gSlotRecord();
            s.playerId = reader.ReadByte();
            s.mapDownloadPercent = reader.ReadByte();
            s.slotStatus = (SlotStatus)reader.ReadByte();
            s.isComputer = reader.ReadByte() == 0x01;
            s.teamNo = reader.ReadByte();
            s.color = (PlayerColor)reader.ReadByte();
            s.race = (Race)reader.ReadByte();
            //for custom game, race flag is combined with 0x40 - race selectable/fixed
            if (s.race > Race.Selectable)
                s.race -= Race.Selectable;
            s.strength = (AIStrength)reader.ReadByte();
            s.hpPercent = reader.ReadByte();
            return s;
        }
    }

    public enum PlayerColor : byte
    {
        Red = 0x00,
        Blue = 0x01,
        Cyan = 0x02,
        Purple = 0x03,
        Yellow = 0x04,
        Orange = 0x05,
        Green = 0x06,
        Pink = 0x07,
        Gray = 0x08,
        LightBlue = 0x09,
        DarkGreen = 0x0A,
        Brown = 0x0B,
        Observer = 0x0C,
    }

    public enum AIStrength : byte
    {
        Easy = 0x00,
        Normal = 0x01,
        Insane = 0x02,
    }

    public enum SlotStatus : byte
    {
        Empty = 0x00,
        Closed = 0x01,
        Used = 0x02,
    }

}
//4.11 [SlotRecord]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1 byte   | player id (0x00 for computer players)
//0x0001 |  1 byte   | map download percent: 0x64 in custom, 0xff in ladder
//0x0002 |  1 byte   | slotstatus:
//       |           |   0x00 empty slot
//       |           |   0x01 closed slot
//       |           |   0x02 used slot
//0x0003 |  1 byte   | computer player flag:
//       |           |   0x00 for human player
//       |           |   0x01 for computer player
//0x0004 |  1 byte   | team number:0 - 11
//       |           | (team 12 == observer or referee)
//0x0005 |  1 byte   | color (0-11):
//       |           |   value+1 matches player colors in world editor:
//       |           |   (red, blue, cyan, purple, yellow, orange, green,
//       |           |    pink, gray, light blue, dark green, brown)
//       |           |   color 12 == observer or referee
//0x0006 |  1 byte   | player race flags (as selected on map screen):
//       |           |   0x01=human
//       |           |   0x02=orc
//       |           |   0x04=nightelf
//       |           |   0x08=undead
//       |           |   0x20=random
//       |           |   0x40=race selectable/fixed (see notes below)
//0x0007 |  1 byte   | computer AI strength: (only present in v1.03 or higher)
//       |           |   0x00 for easy
//       |           |   0x01 for normal
//       |           |   0x02 for insane
//       |           | for non-AI players this seems to be always 0x01
//0x0008 |  1 byte   | player handicap in percent (as displayed on startscreen)
//       |           | valid values: 0x32, 0x3C, 0x46, 0x50, 0x5A, 0x64
//       |           | (field only present in v1.07 or higher)

// Notes:
//  o This record is only 7 bytes in pre 1.03 replays.
//    The last two fields are missing there.

//  o For pre v1.07 replays this record is only 8 bytes.
//    The last field is missing there.

//  o For open and closed slots team and color fields are undetermined.

//  o For WarCraft III patch version <= 1.06:
//    If bit 6 of player race flags is additionally set (0x40 added) then the
//    race is fixed by the map (see also section 4.10).

//  o For WarCraft III patch version >= 1.07:
//    If bit 6 of player race flags is additionally set (0x40 added) then the
//    race is selectable by the player - otherwise it is a ladder game or the
//    race is fixed by the map (see also section 4.10).
