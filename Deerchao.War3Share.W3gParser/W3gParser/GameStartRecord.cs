using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gGameStartRecord
    {
        byte recordId;
        short dataSize;
        byte nSlockRecords;
        List<W3gSlotRecord> slots;
        uint seed;
        byte selectMode;
        byte startSpotCount;

        public List<W3gSlotRecord> Slots
        {
            get { return slots; }
        }

        public static W3gGameStartRecord Parse(BinaryReader reader)
        {
            W3gGameStartRecord r = new W3gGameStartRecord();
            r.recordId = reader.ReadByte();
            r.dataSize = reader.ReadInt16();
            r.nSlockRecords = reader.ReadByte();
            r.slots = new List<W3gSlotRecord>(r.nSlockRecords);
            for (int i = 0; i < r.nSlockRecords; i++)
                r.slots.Add(W3gSlotRecord.Parse(reader));
            r.seed = reader.ReadUInt32();
            r.selectMode = reader.ReadByte();
            r.startSpotCount = reader.ReadByte();
            return r;
        }
    }
}
#region 4.10 [GameStartRecord]
//- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

//offset | size/type | Description
//-------+-----------+-----------------------------------------------------------
//0x0000 |  1 byte   | RecordID - always 0x19
//0x0001 |  1 word   | number of data bytes following
//0x0003 |  1 byte   | nr of SlotRecords following (== nr of slots on startscreen)
//0x0004 |  n bytes  | nr * SlotRecord (see 4.11)
//   n+4 |  1 dword  | RandomSeed (see 4.12)
//   n+8 |  1 byte   | SelectMode
//       |           |   0x00 - team & race selectable (for standard custom games)
//       |           |   0x01 - team not selectable
//       |           |          (map setting: fixed alliances in WorldEditor)
//       |           |   0x03 - team & race not selectable
//       |           |          (map setting: fixed player properties in WorldEditor)
//       |           |   0x04 - race fixed to random
//       |           |          (extended map options: random races selected)
//       |           |   0xcc - Automated Match Making (ladder)
//   n+9 |  1 byte   | StartSpotCount (nr. of start positions in map)
#endregion