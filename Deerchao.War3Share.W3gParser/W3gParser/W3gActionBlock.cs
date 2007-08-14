using System.Collections.Generic;
using System.IO;
using Deerchao.War3Share.Utility;

#pragma warning disable 219

namespace Deerchao.War3Share.W3gParser
{
    public class W3gActionBlock
    {
        byte blockId;

        public byte BlockId
        {
            get { return blockId; }
        }

        public static W3gActionBlock Parse(BinaryReader reader)
        {
            byte id = reader.ReadByte();
            W3gActionBlock result = null;
            switch (id)
            {
                case 0x17:
                    result = W3gLeaveGameBlock.Parse(reader);
                    break;
                case 0x1A:
                case 0x1B:
                case 0x1C:
                    result = W3gUnknownBlockA.Parse(reader);
                    break;
                case 0x1E:
                case 0x1F:
                    result = W3gTimeSlotBlock.Parse(reader);
                    break;
                case 0x20:
                    result = W3gChatBlock.Parse(reader);
                    break;
                case 0x22:
                    result = W3gCheckSumBlock.Parse(reader);
                    break;
                case 0x23:
                    result = W3gUnknownBlockB.Parse(reader);
                    break;
                case 0x2F:
                    result = W3gMapRevealeCountDownBlock.Parse(reader);
                    break;
            }
            if (result != null)
                result.blockId = id;
            return result;
        }
    }

    public class W3gLeaveGameBlock : W3gActionBlock
    {
        int reason;
        byte playerId;
        int result;
        int unknown;

        internal new static W3gLeaveGameBlock Parse(BinaryReader reader)
        {
            W3gLeaveGameBlock block = new W3gLeaveGameBlock();
            block.reason = reader.ReadInt32();
            block.playerId = reader.ReadByte();
            block.result = reader.ReadInt32();
            block.unknown = reader.ReadInt32();
            return block;
        }
    }

    public class W3gChatBlock : W3gActionBlock
    {
        byte sendPlayerId;
        short nFollowedBytes;
        byte flag;
        int chatMode;
        string message;

        internal new static W3gChatBlock Parse(BinaryReader reader)
        {
            W3gChatBlock block = new W3gChatBlock();
            block.sendPlayerId = reader.ReadByte();
            block.nFollowedBytes = reader.ReadInt16();
            block.flag = reader.ReadByte();
            if (block.flag != 0x10)
                block.chatMode = reader.ReadInt32();
            block.message = BinaryHelper.ReadSZString(reader);
            return block;
        }
    }

    public class W3gTimeSlotBlock : W3gActionBlock
    {
        short length;
        short increasedTime;
        List<W3gCommonDataBlock> blocks;

        public short Length
        {
            get { return length; }
        }

        public short IncreasedTime
        {
            get { return increasedTime; }
        }

        public List<W3gCommonDataBlock> Blocks
        {
            get { return blocks; }
        }

        internal new static W3gTimeSlotBlock Parse(BinaryReader reader)
        {
            W3gTimeSlotBlock block = new W3gTimeSlotBlock();
            block.length = reader.ReadInt16();
            block.increasedTime = reader.ReadInt16();

            block.blocks = ParseCommonDataBlocks(reader, block.length - 2);

            return block;
        }

        private static List<W3gCommonDataBlock> ParseCommonDataBlocks(BinaryReader reader, int length)
        {
            List<W3gCommonDataBlock> result = new List<W3gCommonDataBlock>();
            while (length > 0)
            {
                int nPointerMoved;
                result.Add(W3gCommonDataBlock.Parse(reader, out nPointerMoved));
                length -= nPointerMoved;
            }
            return result;
        }
    }

    public class W3gUnknownBlockA : W3gActionBlock
    {
        int unknown;

        internal new static W3gUnknownBlockA Parse(BinaryReader reader)
        {
            W3gUnknownBlockA block = new W3gUnknownBlockA();
            block.unknown = reader.ReadInt32();
            return block;
        }
    }

    public class W3gCheckSumBlock : W3gActionBlock
    {
        byte length;
        int unknown;

        internal new static W3gCheckSumBlock Parse(BinaryReader reader)
        {
            W3gCheckSumBlock block = new W3gCheckSumBlock();
            block.length = reader.ReadByte();
            block.unknown = reader.ReadInt32();
            return block;
        }
    }

    public class W3gUnknownBlockB : W3gActionBlock
    {
        int unknownA;
        byte unknownB;
        int unknownC;
        byte unknownD;

        internal new static W3gUnknownBlockB Parse(BinaryReader reader)
        {
            W3gUnknownBlockB block = new W3gUnknownBlockB();
            block.unknownA = reader.ReadInt32();
            block.unknownB = reader.ReadByte();
            block.unknownC = reader.ReadInt32();
            block.unknownD = reader.ReadByte();
            return block;
        }
    }

    public class W3gMapRevealeCountDownBlock : W3gActionBlock
    {
        int mode;
        int time;

        internal static new W3gMapRevealeCountDownBlock Parse(BinaryReader reader)
        {
            W3gMapRevealeCountDownBlock block = new W3gMapRevealeCountDownBlock();
            block.mode = reader.ReadInt32();
            block.time = reader.ReadInt32();
            return block;
        }
    }
}
#pragma warning restore 219
