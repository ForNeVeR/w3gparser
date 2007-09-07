using System;

namespace Deerchao.War3Share.W3gParser
{
    public class ChatInfo
    {
        private readonly TimeSpan time;
        private readonly Player from;
        private readonly TalkTo to;
        private readonly Player toPlayer;
        private readonly string message;

        public ChatInfo(int time, Player from, TalkTo to, Player toPlayer, string message)
        {
            this.time = new TimeSpan(0, 0, 0, 0, time);
            this.from = from;
            this.to = to;
            this.toPlayer = toPlayer;
            this.message = message;
        }

        public TimeSpan Time
        {
            get { return time; }
        }

        public Player From
        {
            get { return from; }
        }

        public TalkTo To
        {
            get { return to; }
        }

        public string Message
        {
            get { return message; }
        }

        public Player ToPlayer
        {
            get { return toPlayer; }
        }
    }
}