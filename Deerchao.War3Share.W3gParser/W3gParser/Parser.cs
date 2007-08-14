using System.IO;
using Deerchao.War3Share.Utility;

namespace Deerchao.War3Share.W3gParser
{
    public class Parser
    {
        public static W3gFile Parse(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return Parse(file);
            }
        }

        public static W3gFile Parse(Stream stream)
        {
            W3gFile file = new W3gFile();
            try
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    file.Header = W3gHeader.Parse(reader);

                    reader.BaseStream.Seek(file.Header.HeaderSize, SeekOrigin.Begin);
                    for (int i = 0; i < file.Header.NBlocks; i++)
                    {
                        file.Blocks.Add(W3gBlock.Parse(reader));
                    }
                }
            }
            catch (IOException)
            {
                throw new ParserException("文件不是Replay文件,或者不完整.");
            }

            try
            {
                using (BinaryReader mreader = file.GetBlocksReader())
                {
                    #region 4.0 [Decompressed data]
                    //===============================================================================

                    //Decompressed data is a collection of data items that appear back to back in
                    //the stream. The offsets for these items vary depending on the size of every
                    //single item.

                    //This section describes the records that always appear at the beginning of
                    //a replay data stream. They hold information about settings and players right
                    //before the start of the game. Data about the game in progress is described
                    //in section 5.

                    //The order of the start up items is as follows:

                    // # |   Size   | Name
                    //---+----------+--------------------------
                    // 1 |   4 byte | Unknown (0x00000110 - another record id?)
                    // 2 | variable | PlayerRecord (see 4.1)
                    // 3 | variable | GameName (null terminated string) (see 4.2)
                    // 4 |   1 byte | Nullbyte
                    // 5 | variable | Encoded String (null terminated) (see 4.3)
                    //   |          |  - GameSettings (see 4.4)
                    //   |          |  - Map&CreatorName (see 4.5)
                    // 6 |   4 byte | PlayerCount (see 4.6)
                    // 7 |   4 byte | GameType (see 4.7)
                    // 8 |   4 byte | LanguageID (see 4.8)
                    // 9 | variable | PlayerList (see 4.9)
                    //10 | variable | GameStartRecord (see 4.11)

                    //The following sections describe these items in detail.
                    //After the static items (as described above) there follow variable information
                    //organized in blocks that are described in section 5.

                    #endregion
                    mreader.ReadBytes(4);
                    file.Host = W3gPlayer.Parse(mreader);
                    file.Game = W3gGame.Parse(mreader);
                    file.Players.AddRange(W3gPlayer.ParsePlayers(mreader));
                    file.GameStartReord = W3gGameStartRecord.Parse(mreader);
                    while (mreader.BaseStream.Length - mreader.BaseStream.Position > 0)
                    {
                        file.ActionBlocks.Add(W3gActionBlock.Parse(mreader));
                    }
                }

                return file;
            }
            catch (IOException)
            {
                throw new ParserException("解析器出错,请向作者报告正在解析的replay.");
            }
        }

    }
}
