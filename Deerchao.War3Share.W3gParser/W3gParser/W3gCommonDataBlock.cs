using System.Collections.Generic;
using System.IO;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gCommonDataBlock
    {
        byte playerId;
        short length;
        List<W3gPlayerAction> actions;

        public byte PlayerId
        {
            get { return playerId; }
        }

        public short Length
        {
            get { return length; }
        }


        public List<W3gPlayerAction> Actions
        {
            get { return actions; }
        }

        internal static W3gCommonDataBlock Parse(BinaryReader reader, out int nPointerMoved)
        {
            nPointerMoved = 0;
            W3gCommonDataBlock block = new W3gCommonDataBlock();

            block.playerId = reader.ReadByte();
            nPointerMoved += 1;

            block.length = reader.ReadInt16();
            nPointerMoved += 2;

            block.actions = new List<W3gPlayerAction>();

            int remaind = block.length;

            while (remaind > 0)
            {
                int used;

                W3gPlayerAction action = W3gPlayerAction.Parse(reader, out used);
                block.actions.Add(action);

                remaind -= used;
                nPointerMoved += used;
            }
            return block;
        }
    }
}
