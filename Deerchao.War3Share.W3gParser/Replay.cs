using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Deerchao.War3Share.W3gParser
{
    public class Replay
    {
        static readonly string HeaderString = "Warcraft III recorded game\u001A\0";

        private long hash;
        private MapInfo map;
        private int length;
        private int version;
        private int buildNo;
        private GameType type;
        private string name;
        private Player host;
        private GameSettings settings;
        private readonly List<Team> teams = new List<Team>();
        private readonly List<Player> players = new List<Player>();
        private readonly List<ChatInfo> chats = new List<ChatInfo>();

        private readonly long size;
        public long Size
        {
            get { return size; }
        }

        public Player Host
        {
            get
            {
                return host;
            }
        }
        public MapInfo Map
        {
            get { return map; }
        }
        public string GameName
        {
            get { return name; }
        }
        public GameType GameType
        {
            get { return type; }
        }
        public TimeSpan GameLength
        {
            get { return new TimeSpan(0, 0, 0, 0, length); }
        }
        public int Version
        {
            get { return version; }
        }
        public List<Player> Players
        {
            get
            {
                return players;
            }
        }
        public List<Team> Teams
        {
            get { return teams; }
        }
        public List<ChatInfo> Chats
        {
            get { return chats; }
        }

        public long Hash
        {
            get { return hash; }
        }

        private BinaryWriter hashWriter;

        public KeyValuePair<int, long> GetMapKey()
        {
            return new KeyValuePair<int, long>(version, Map.Hash);
        }

        public Replay(string fileName)
            : this(File.OpenRead(fileName))
        {
        }

        public Replay(Stream stream)
        {
            using (stream)
            {
                try { size = stream.Length; }
                catch (NotSupportedException)
                { }
                Load(stream);
            }
        }

        private void Load(Stream stream)
        {
            MemoryStream blocksData = LoadHeader(stream);
            hashWriter = new BinaryWriter(new MemoryStream());
            using (BinaryReader reader = new BinaryReader(blocksData))
            {
                LoadPlayers(reader);
                LoadActions(reader);
            }
            //strangely, I found a ladder replay with -49 seconds length..
            if (length < 0)
            {
                foreach (Player player in players)
                {
                    if (player.Time > length)
                        length = player.Time;
                }
            }

            hashWriter.Write(version);
            hashWriter.Write(map.Hash);
            hashWriter.Flush();

            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] hashedData = provider.ComputeHash(((MemoryStream)hashWriter.BaseStream).ToArray());
            hashWriter.Close();
            hash = 0;
            for (int i = 4; i < 12; i++)
            {
                hash = hash << 8;
                hash |= hashedData[i];
            }
        }

        private void LoadActions(BinaryReader reader)
        {
            int time = 0;
            bool isPaused = false;
            while (reader.BaseStream.Length - reader.BaseStream.Position > 0)
            {
                byte blockId = reader.ReadByte();
                switch (blockId)
                {
                    case 0x1A:
                    case 0x1B:
                    case 0x1C:
                        reader.BaseStream.Seek(reader.BaseStream.Position + 4, SeekOrigin.Begin);
                        break;
                    case 0x22:
                        reader.BaseStream.Seek(reader.BaseStream.Position + 5, SeekOrigin.Begin);
                        break;
                    case 0x23:
                        reader.BaseStream.Seek(reader.BaseStream.Position + 10, SeekOrigin.Begin);
                        break;
                    case 0x2F:
                        reader.BaseStream.Seek(reader.BaseStream.Position + 8, SeekOrigin.Begin);
                        break;
                    //leave game
                    case 0x17:
                        reader.ReadInt32();
                        byte playerId = reader.ReadByte();
                        Player p = GetPlayerById(playerId);
                        p.Time = time;
                        reader.ReadInt64();
                        break;
                    //chat
                    case 0x20:
                        byte fromId = reader.ReadByte();
                        reader.ReadBytes(2);
                        byte chatType = reader.ReadByte();
                        TalkTo to = TalkTo.All;
                        if (chatType != 0x10)
                        {
                            to = (TalkTo)reader.ReadInt32();
                        }
                        string message = ParserUtility.ReadString(reader);
                        if (chatType != 0x10)
                        {
                            ChatInfo chat = new ChatInfo(time, GetPlayerById(fromId), to, GetPlayerById((byte)(to - 3)), message);
                            chats.Add(chat);
                        }
                        break;
                    //time slot
                    case 0x1E:
                    case 0x1F:
                        short rest = reader.ReadInt16();
                        short increasedTime = reader.ReadInt16();
                        if (!isPaused)
                            time += increasedTime;
                        rest -= 2;
                        LoadTimeSlot(reader, rest, time, ref isPaused);
                        break;
                    case 0:
                        return;
                    default:
                        throw new W3gParserException("未知的数据块ID.");
                }
            }
        }

        private void LoadTimeSlot(BinaryReader reader, short rest, int time, ref bool isPaused)
        {
            bool wasDeselect = false;

#pragma warning disable TooWideLocalVariableScope
            short flag;
            uint itemId;
            int x;
            int y;
            int objectId1;
            int objectId2;
            uint itemId1;
            uint itemId2;
            int x2;
            int y2;
            short unitCount;
            byte groupNo;
            byte slotNo;
            int len;
#pragma warning restore TooWideLocalVariableScope

            while (rest > 0)
            {
                byte playerId = reader.ReadByte();
                Player player = GetPlayerById(playerId);
                player.Time = time;
                short playerBlockRest = reader.ReadInt16();
                rest -= 3;
                short prest = playerBlockRest;

                while (prest > 0)
                {
                    #region
                    byte actionId = reader.ReadByte();
                    switch (actionId)
                    {
                        //pause game
                        case 0x01:
                            isPaused = true;
                            prest--;
                            break;
                        //resume game
                        case 0x02:
                            isPaused = false;
                            prest--;
                            break;
                        //set game speed
                        case 0x03:
                            prest -= 2;
                            break;
                        //icrease, decrease game speed
                        case 0x04:
                        case 0x05:
                            prest--;
                            break;
                        //save game
                        case 0x06:
                            len = 0;
                            while (reader.ReadByte() != 0)
                                len++;
                            prest -= (short)(len + 2);
                            break;
                        //game saved
                        case 0x07:
                            reader.ReadInt32();
                            prest -= 5;
                            break;
                        //unit ability without target
                        case 0x10:
                            flag = reader.ReadInt16();
                            itemId = reader.ReadUInt32();
                            //unknownA, unknownB
                            reader.ReadInt64();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(flag);
                            hashWriter.Write(itemId);

                            OrderItem(player, itemId, time);
                            prest -= 15;
                            break;
                        //unit ability with target position
                        case 0x11:
                            flag = reader.ReadInt16();
                            itemId = reader.ReadUInt32();
                            //unknownA, unknownB
                            reader.ReadInt64();
                            x = reader.ReadInt32();
                            y = reader.ReadInt32();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(flag);
                            hashWriter.Write(itemId);
                            hashWriter.Write(x);
                            hashWriter.Write(y);

                            OrderItem(player, itemId, time);
                            prest -= 23;
                            break;
                        //unit ability with target position and target object
                        case 0x12:
                            flag = reader.ReadInt16();
                            itemId = reader.ReadUInt32();
                            //unknownA, unknownB
                            reader.ReadInt64();
                            x = reader.ReadInt32();
                            y = reader.ReadInt32();
                            objectId1 = reader.ReadInt32();
                            objectId2 = reader.ReadInt32();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(flag);
                            hashWriter.Write(itemId);
                            hashWriter.Write(x);
                            hashWriter.Write(y);
                            hashWriter.Write(objectId1);
                            hashWriter.Write(objectId2);
                            prest -= 31;
                            break;
                        //unit ability with target position, target object, and target item (give item action)
                        case 0x13:
                            flag = reader.ReadInt16();
                            itemId = reader.ReadUInt32();
                            //unknownA, unknownB
                            reader.ReadInt64();
                            x = reader.ReadInt32();
                            y = reader.ReadInt32();
                            objectId1 = reader.ReadInt32();
                            objectId2 = reader.ReadInt32();
                            itemId1 = reader.ReadUInt32();
                            itemId2 = reader.ReadUInt32();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(flag);
                            hashWriter.Write(itemId);
                            hashWriter.Write(x);
                            hashWriter.Write(y);
                            hashWriter.Write(objectId1);
                            hashWriter.Write(objectId2);
                            hashWriter.Write(itemId1);
                            hashWriter.Write(itemId2);
                            prest -= 39;
                            break;
                        //unit ability with two target positions and two item IDs
                        case 0x14:
                            flag = reader.ReadInt16();
                            itemId = reader.ReadUInt32();
                            //unknownA, unknownB
                            reader.ReadInt64();
                            x = reader.ReadInt32();
                            y = reader.ReadInt32();
                            itemId1 = reader.ReadUInt32();
                            reader.ReadBytes(9);
                            x2 = reader.ReadInt32();
                            y2 = reader.ReadInt32();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(flag);
                            hashWriter.Write(itemId);
                            hashWriter.Write(x);
                            hashWriter.Write(y);
                            hashWriter.Write(itemId1);
                            hashWriter.Write(x2);
                            hashWriter.Write(y2);
                            prest -= 44;
                            break;
                        //change selection
                        case 0x16:
                            byte selectMode = reader.ReadByte();
                            unitCount = reader.ReadInt16();
                            //object ids
                            reader.ReadBytes(unitCount * 8);

                            //if is deselect
                            if (selectMode == 2)
                            {
                                wasDeselect = true;
                                player.Actions++;
                            }
                            else
                            {
                                if (!wasDeselect)
                                    player.Actions++;
                                wasDeselect = false;
                            }
                            player.Units.Multiplier = unitCount;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(selectMode);
                            hashWriter.Write(unitCount);
                            prest -= (short)(unitCount * 8 + 4);
                            break;
                        //create group
                        case 0x17:
                            groupNo = reader.ReadByte();
                            unitCount = reader.ReadInt16();
                            //unit ids
                            reader.ReadBytes(unitCount * 8);

                            player.Actions++;
                            player.Groups.SetGroup(groupNo, unitCount);
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(groupNo);
                            hashWriter.Write(unitCount);
                            prest -= (short)(unitCount * 8 + 4);
                            break;
                        //select group
                        case 0x18:
                            groupNo = reader.ReadByte();
                            //unknown
                            reader.ReadByte();

                            player.Actions++;
                            player.Units.Multiplier = player.Groups[groupNo];
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(groupNo);
                            prest -= 3;
                            break;
                        //select sub group
                        case 0x19:
                            //itemId, objectId1, objectId2
                            reader.ReadInt64();
                            reader.ReadInt32();

                            //no way to find how many buildings is in a subgroup..
                            player.Units.Multiplier = 1;
                            prest -= 13;
                            break;
                        //pre select sub group
                        case 0x1A:
                            prest--;
                            break;
                        //unknown
                        case 0x1B:
                            //unknown, objectid1, objectid2
                            reader.ReadByte();
                            reader.ReadInt64();
                            prest -= 10;
                            break;
                        //select ground item
                        case 0x1C:
                            //unknown, objectid1, objectid2
                            reader.ReadByte();
                            reader.ReadInt64();

                            player.Actions++;
                            prest -= 10;
                            break;
                        //cancel hero revival
                        case 0x1D:
                            reader.ReadInt64();
                            player.Actions++;
                            prest -= 9;
                            break;
                        //remove unit from order queue
                        case 0x1E:
                            slotNo = reader.ReadByte();
                            itemId = reader.ReadUInt32();

                            player.Actions++;
                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(slotNo);
                            hashWriter.Write(itemId);
                            CancelItem(player, itemId, time);
                            prest -= 6;
                            break;
                        //unknown
                        case 0x21:
                            reader.ReadInt64();
                            prest -= 9;
                            break;
                        //cheats
                        case 0x20:
                        case 0x22:
                        case 0x23:
                        case 0x24:
                        case 0x25:
                        case 0x26:
                        case 0x29:
                        case 0x2A:
                        case 0x2B:
                        case 0x2C:
                        case 0x2F:
                        case 0x30:
                        case 0x31:
                        case 0x32:
                            prest--;
                            break;
                        //cheats
                        case 0x27:
                        case 0x28:
                        case 0x2D:
                            reader.ReadByte();
                            reader.ReadInt32();
                            prest -= 6;
                            break;
                        //cheats
                        case 0x2E:
                            reader.ReadInt32();
                            prest -= 5;
                            break;
                        //change ally option
                        case 0x50:
                            reader.ReadByte();
                            reader.ReadInt32();
                            prest -= 6;
                            break;
                        //transfer resource
                        case 0x51:
                            slotNo = reader.ReadByte();
                            int gold = reader.ReadInt32();
                            int lumber = reader.ReadInt32();

                            hashWriter.Write(time);
                            hashWriter.Write(actionId);
                            hashWriter.Write(slotNo);
                            hashWriter.Write(gold);
                            hashWriter.Write(lumber);
                            prest -= 10;
                            break;
                        //trigger chat
                        case 0x60:
                            //unknownA, unknownB
                            reader.ReadInt64();
                            len = 0;
                            while (reader.ReadByte() != 0)
                                len++;

                            prest -= (short)(10 + len);
                            break;
                        //esc pressed
                        case 0x61:
                            player.Actions++;
                            prest--;
                            break;
                        //Scenario Trigger
                        case 0x62:
                            //unknownABC
                            reader.ReadInt32();
                            reader.ReadInt64();

                            prest -= 13;
                            break;
                        //begin choose hero skill
                        case 0x66:
                            player.Actions++;
                            prest--;
                            break;
                        //begin choose building
                        case 0x67:
                            player.Actions++;
                            prest--;
                            break;
                        //ping mini map
                        case 0x68:
                            //x, y
                            reader.ReadInt64();
                            //unknown
                            reader.ReadInt32();

                            prest -= 13;
                            break;
                        //continue game
                        case 0x69:
                        case 0x6A:
                            reader.ReadInt64();
                            reader.ReadInt64();
                            prest -= 17;
                            break;
                        case 0x75:
                            reader.ReadByte();
                            prest -= 2;
                            break;
                        default:
                            throw new W3gParserException("未知的ActionId.");
                    }
                    #endregion
                }
                rest -= playerBlockRest;
            }
        }

        private void CancelItem(Player player, uint itemId, int time)
        {
            if (itemId < 0x41000000 || itemId > 0x7a000000)
                return;
            string stringId = ParserUtility.StringFromUInt(itemId);
            switch (ParserUtility.ItemTypeFromId(stringId))
            {
                case ItemType.None:
                    break;
                case ItemType.Hero:
                    player.Heroes.Cancel(stringId, time);
                    break;
                case ItemType.Building:
                    player.Buildings.Cancel(new OrderItem(stringId, time, true)); ;
                    break;
                case ItemType.Research:
                    player.Researches.Cancel(new OrderItem(stringId, time, true)); ;
                    break;
                case ItemType.Unit:
                    player.Units.Cancel(new OrderItem(stringId, time, true)); ;
                    break;
                case ItemType.Upgrade:
                    player.Upgrades.Cancel(new OrderItem(stringId, time, true)); ;
                    break;
            }
        }

        private void OrderItem(Player player, uint itemId, int time)
        {
            if (itemId < 0x41000000 || itemId > 0x7a000000)
                return;
            string stringId = ParserUtility.StringFromUInt(itemId);
            switch (ParserUtility.ItemTypeFromId(stringId))
            {
                case ItemType.None:
                    if ((itemId >> 16) == 0x00000233)
                        player.Units.Order(new OrderItem("ubsp", time));
                    break;
                case ItemType.Hero:
                    player.Heroes.Order(stringId, time);
                    break;
                case ItemType.HeroAbility:
                    player.Heroes.Train(stringId, time);
                    break;
                case ItemType.Building:
                    player.Buildings.Order(new OrderItem(stringId, time));
                    break;
                case ItemType.Item:
                    player.Items.Order(new OrderItem(stringId, time));
                    if (stringId == "tret")
                        player.Heroes.PossibleRetrained(time);
                    break;
                case ItemType.Unit:
                    player.Units.Order(new OrderItem(stringId, time));
                    break;
                case ItemType.Upgrade:
                    player.Upgrades.Order(new OrderItem(stringId, time)); ;
                    break;
                case ItemType.Research:
                    player.Researches.Order(new OrderItem(stringId, time)); ;
                    break;
            }
        }

        private MemoryStream LoadHeader(Stream stream)
        {
            MemoryStream blocksData = new MemoryStream();
            using (BinaryReader reader = new BinaryReader(stream))
            {
                #region 2.0 [Header]

                #region doc

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

                #endregion

                ValidateHeaderString(reader.ReadBytes(28));

                int headerSize = reader.ReadInt32();
                //overall size of compressed file
                reader.ReadInt32();
                int versionFlag = reader.ReadInt32();
                //overall size of decompressed data (excluding header)
                reader.ReadInt32();
                int nBlocks = reader.ReadInt32();

                #endregion

                #region SubHeader

                if (versionFlag == 0)
                {
                    throw new W3gParserException("本软件不支持1.06及更低版本录像.");
                }
                else if (versionFlag == 1)
                {
                    #region 2.2 [SubHeader] for header version 1

                    #region doc

                    //offset | size/type | Description
                    //-------+-----------+-----------------------------------------------------------
                    //0x0000 |  1 dword  | version identifier string reading:
                    //       |           |  'WAR3' for WarCraft III Classic
                    //       |           |  'W3XP' for WarCraft III Expansion Set 'The Frozen Throne'
                    //0x0004 |  1 dword  | version number (corresponds to patch 1.xx so far)
                    //0x0008 |  1  word  | build number (see section 2.3)
                    //0x000A |  1  word  | flags
                    //       |           |   0x0000 for single player games
                    //       |           |   0x8000 for multiplayer games (LAN or Battle.net)
                    //0x000C |  1 dword  | replay length in msec
                    //0x0010 |  1 dword  | CRC32 checksum for the header
                    //       |           | (the checksum is calculated for the complete header
                    //       |           |  including this field which is set to zero)

                    #endregion

                    string war3string = ParserUtility.StringFromUInt(reader.ReadUInt32());
                    if (war3string != "W3XP")
                        throw new W3gParserException("本软件只支持冰封王座录像,不支持混乱之治录像.");

                    version = reader.ReadInt32();
                    buildNo = reader.ReadInt32();
                    //flags
                    reader.ReadInt16();
                    length = reader.ReadInt32();
                    //CRC32 checksum for the header
                    reader.ReadInt32();

                    #endregion
                }

                #endregion

                reader.BaseStream.Seek(headerSize, SeekOrigin.Begin);
                for (int i = 0; i < nBlocks; i++)
                {
                    #region [Data block header]

                    #region doc

                    //offset | size/type | Description
                    //-------+-----------+-----------------------------------------------------------
                    //0x0000 |  1  word  | size n of compressed data block (excluding header)
                    //0x0002 |  1  word  | size of decompressed data block (currently 8k)
                    //0x0004 |  1 dword  | unknown (probably checksum)
                    //0x0008 |  n bytes  | compressed data (decompress using zlib)

                    #endregion

                    ushort compressedSize = reader.ReadUInt16();
                    ushort decompressedSize = reader.ReadUInt16();
                    //unknown (probably checksum)
                    reader.ReadInt32();

                    byte[] decompressed = new byte[decompressedSize];
                    byte[] compressed = reader.ReadBytes(compressedSize);
                    using (InflaterInputStream zipStream = new InflaterInputStream(new MemoryStream(compressed)))
                    {
                        zipStream.Read(decompressed, 0, decompressedSize);
                    }
                    blocksData.Write(decompressed, 0, decompressedSize);

                    #endregion
                }
            }
            blocksData.Seek(0, SeekOrigin.Begin);
            return blocksData;
        }

        private void LoadPlayers(BinaryReader reader)
        {
            #region 4.0 [Decompressed data]

            #region doc

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

            #endregion
            //Unknown
            reader.ReadInt32();

            host = new Player();
            host.Load(reader);
            players.Add(host);
            type = host.GameType;

            name = ParserUtility.ReadString(reader);
            //nullbyte
            reader.ReadByte();

            byte[] decoded = ParserUtility.GetDecodedBytes(reader);

            settings = new GameSettings(decoded);
            map = new MapInfo(ParserUtility.GetUInt(decoded, 9), ParserUtility.GetString(decoded, 13));
            #endregion

            //playerCount, gameType, langId
            reader.ReadBytes(12);

            #region Player List
            while (reader.PeekChar() == 0x16)
            {
                #region doc
                //offset | size/type | Description
                //-------+-----------+-----------------------------------------------------------
                //0x0000 | 4/11 byte | PlayerRecord (see 4.1)
                //0x000? |    4 byte | unknown
                //       |           |  (always 0x00000000 for patch version >= 1.07
                //       |           |   always 0x00000001 for patch version <= 1.06)
                #endregion

                Player p = new Player();
                p.Load(reader);
                players.Add(p);
                reader.ReadBytes(4);
            }
            #endregion

            #region 4.10 [GameStartRecord]
            #region doc
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

            //RecordId, number of data bytes following
            reader.ReadBytes(3);
            byte nSlots = reader.ReadByte();
            for (byte i = 0; i < nSlots; i++)
            {
                #region 4.11 [SlotRecord]

                #region doc
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
                #endregion

                #region
                byte playerId = reader.ReadByte();
                reader.ReadByte();
                byte slotStatus = reader.ReadByte();
                byte computerFlag = reader.ReadByte();
                byte teamNo = reader.ReadByte();
                PlayerColor color = (PlayerColor)reader.ReadByte();
                Race race = (Race)reader.ReadByte();
                if ((uint)race > 0x40)
                    race -= 0x40;
                AIStrength strength = (AIStrength)reader.ReadByte();
                int handicap = reader.ReadByte();
                #endregion

                #region
                if (slotStatus == 0x02)
                {
                    Player player = GetPlayerById(playerId);
                    if (player == null && computerFlag == 0x01)
                    {
                        player = new Player();
                        player.Race = race;
                        player.Id = i;
                        if (strength == AIStrength.Easy)
                            player.Name = "电脑(简单的)";
                        else if (strength == AIStrength.Normal)
                            player.Name = "电脑(中等的)";
                        else
                            player.Name = "电脑(令人发狂的)";
                        players.Add(player);
                    }
                    else if (player == null)
                    {
                        continue;
                    }
                    player.SlotNo = i;
                    player.Color = color;
                    player.Handicap = handicap;
                    player.Id = playerId;
                    player.IsComputer = computerFlag == 0x01;
                    player.IsObserver = teamNo == 12;
                    player.Race = race;
                    player.TeamNo = teamNo + 1;
                }
                #endregion
                #endregion
            }


            if (buildNo == 0 && nSlots == 0)
            {
                #region 6.1 Notes on official Blizzard Replays
                #region doc
                //o Since the lack of all slot records, one has to generate these data:
                //iterate slotNumber from 1 to number of PlayerRecords (see 4.1)
                //  player id    = slotNumber
                //  slotstatus   = 0x02                   (used)
                //  computerflag = 0x00                   (human player)
                //  team number  = (slotNumber -1) mod 2  (team membership alternates in
                //                                                          PlayerRecord)
                //  color        = unknown                (player gets random colors)
                //  race         = as in PlayerRecord
                //  computerAI   = 0x01                   (non computer player)
                //  handicap     = 0x64                   (100%)
                #endregion
                foreach (Player player in players)
                {
                    player.SlotNo = player.Id;
                    player.TeamNo = (player.SlotNo - 1) % 2;
                    player.Handicap = 100;
                }
                #endregion
            }

            foreach (Player player in players)
            {
                Team team = GetTeamByNo(player.TeamNo);
                team.Add(player);
            }
            //random seed, select mode, startspotcount
            reader.ReadBytes(6);
            #endregion
        }

        private Team GetTeamByNo(int teamNo)
        {
            foreach (Team team in teams)
                if (team.TeamNo == teamNo)
                    return team;
            Team t = new Team(teamNo);
            teams.Add(t);
            return t;
        }

        private Player GetPlayerById(byte playerId)
        {
            foreach (Player player in players)
            {
                if (player.Id == playerId)
                    return player;
            }
            return null;
        }

        private void ValidateHeaderString(byte[] header)
        {
            for (int i = 0; i < 28; i++)
            {
                if (HeaderString[i] != (char)header[i])
                    throw new W3gParserException("指定的文件不是合法的Warcraft III Replay文件。");
            }
        }
    }
}