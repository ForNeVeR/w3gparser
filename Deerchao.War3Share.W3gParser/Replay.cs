using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Deerchao.War3Share.W3gParser;

namespace Deerchao.War3Share
{
    public class Replay
    {
        private readonly W3gFile file;
        private string filePath;
        private TimeSpan gameLength;

        private GameType gameType;
        private int gameVersion;
        private long hash;
        private Player host;
        private W3gMap map;
        private List<Player> players;

        public Replay(string filePath)
        {
            file = Parser.Parse(filePath);
            this.filePath = filePath;
            Load();
        }

        public Replay(Stream stream)
        {
            file = Parser.Parse(stream);
            Load();
        }

        public KeyValuePair<int, long> GetMapKey()
        {
            return new KeyValuePair<int, long>(GameVersion, Map.CheckSum);
        }

        public W3gMap Map
        {
            get { return map; }
            set { map = value; }
        }

        /// <summary>
        /// 比如21代表1.21版
        /// </summary>
        public int GameVersion
        {
            get { return gameVersion; }
            set { gameVersion = value; }
        }

        public GameType GameType
        {
            get { return gameType; }
            set { gameType = value; }
        }

        public List<Player> Players
        {
            get { return players; }
        }

        public Player Host
        {
            get { return host; }
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        public long Hash
        {
            get { return hash; }
        }

        public TimeSpan GameLength
        {
            get { return gameLength; }
            set { gameLength = value; }
        }

        private void Load()
        {
            gameVersion = file.Header.MinorVersion;
            gameLength = file.Header.GameLength;
            LoadMap();
            LoadPlayers();
            LoadActions();
        }

        private void LoadMap()
        {
            map = file.Game.Setting.Map;
        }

        private void LoadActions()
        {
            int time = 0;
            APMContext context = new APMContext();

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode))
            {
                writer.Write(gameVersion);
                writer.Write(map.CheckSum);

                int maxActionsForHash = 500;

                foreach (W3gActionBlock actionblock in file.ActionBlocks)
                {
                    W3gTimeSlotBlock timeslot = actionblock as W3gTimeSlotBlock;
                    if (timeslot != null)
                    {
                        if (!context.IsPaused)
                            time += timeslot.IncreasedTime;

                        foreach (W3gCommonDataBlock datablock in timeslot.Blocks)
                        {
                            context.Refresh();

                            Player p = GetPlayerById(datablock.PlayerId);
                            p.Time = new TimeSpan(0, 0, 0, 0, time);

                            if (!p.IsComputer && !p.IsObserver)
                            {
                                if (maxActionsForHash > 0 && datablock.Actions.Count > 0)
                                    writer.Write(p.Time.Ticks);

                                foreach (W3gPlayerAction action in datablock.Actions)
                                {
                                    int actionCount = action.ActionsCountForAPM(context);
                                    p.Actions += actionCount;

                                    if (maxActionsForHash > 0 && actionCount > 0)
                                    {
                                        action.WriteBytes(writer);
                                        maxActionsForHash--;
                                    }
                                }
                            }
                        }
                    }
                }

                writer.Flush();

                MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
                byte[] hashedData = provider.ComputeHash(stream.ToArray());
                stream.Close();
                hash = 0;
                for (int i = 4; i < 12; i++)
                {
                    hash = hash << 8;
                    hash |= hashedData[i];
                }
            }
            //note: hash algo is based on the first 500 actions which should be count in apm
            //note: first MD5 hash, then get the 4~11 (zero based) bytes from the result to form an Int64
            //note: the main concern is that different savers of a game may quit at different time, but their replay should be treat as one
            //note: I choose long but not Guid because long is easier to store in sqlite
        }

        private void LoadPlayers()
        {
            players = new List<Player>();
            foreach (W3gPlayer p in file.Players)
            {
                gameType = p.GameType;

                Player player = new Player();
                player.Id = p.PlayerId;
                player.Name = p.PlayerName;

                player.SlotNo = file.GetSlotNoByPlayer(p);
                //don't know why, but there are some players not in any slot in some replays
                if (player.SlotNo == -1)
                    continue;

                W3gSlotRecord slot = file.GameStartReord.Slots[player.SlotNo];
                player.TeamNo = slot.TeamNo;
                player.Race = slot.Race;
                player.Color = slot.Color;
                player.HpPercentage = slot.HpPercent;
                player.IsComputer = slot.IsComputer;
                player.IsObserver = slot.TeamNo == 12;

                if (p.IsHost)
                    host = player;

                players.Add(player);
            }
        }

        public Player GetPlayerById(byte id)
        {
            foreach (Player p in players)
            {
                if (p.Id == id)
                    return p;
            }
            return null;
        }
    }
}