using System.Collections.Generic;
using System.IO;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gFile
    {
        W3gHeader header;
        List<W3gBlock> blocks=new List<W3gBlock>();
        W3gGame game;
        readonly List<W3gPlayer> players = new List<W3gPlayer>();
        W3gGameStartRecord gameStartReord;
        List<W3gActionBlock> actionBlocks=new List<W3gActionBlock>();

        public List<W3gActionBlock> ActionBlocks
        {
            get { return actionBlocks; }
            set { actionBlocks = value; }
        }

        internal W3gHeader Header
        {
            get { return header; }
            set { header = value; }
        }

        internal List<W3gBlock> Blocks
        {
            get { return blocks; }
            set { blocks = value; }
        }

        public W3gPlayer Host
        {
            get { return players[0]; }
            set
            {
                if (players.Count == 0)
                    players.Add(value);
                else
                    players[0] = value;
            }
        }

        public W3gGame Game
        {
            get { return game; }
            internal set { game = value; }
        }

        public List<W3gPlayer> Players
        {
            get { return players; }
        }

        public W3gGameStartRecord GameStartReord
        {
            get { return gameStartReord; }
            set { gameStartReord = value; }
        }

        public BinaryReader GetBlocksReader()
        {
            MemoryStream ms = new MemoryStream();
            foreach (W3gBlock block in Blocks)
            {
                ms.Write(block.DecopressedData, 0, block.DecopressedData.Length);
            }

            ms.Position = 0;
            BinaryReader mreader = new BinaryReader(ms);

            return mreader;
        }

        public int GetSlotNoByPlayer(W3gPlayer p)
        {
            for (int i = 0; i < GameStartReord.Slots.Count; i++)
            {
                if (GameStartReord.Slots[i].PlayerId == p.PlayerId)
                    return i;
            }
            return -1;
        }
    }
}
