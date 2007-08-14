using System;
using Deerchao.War3Share.W3gParser;

namespace Deerchao.War3Share
{
    public class Player
    {
        private int actions;
        private PlayerColor color;
        private int hpPercentage;
        private byte id;
        private bool isComputer;
        private bool isObserver;
        private string name;
        private Race race;
        private int slotNo;
        private int teamNo;
        private TimeSpan time;

        public float Apm
        {
            get { return (float) (actions/time.TotalMinutes); }
        }

        public int Actions
        {
            get { return actions; }
            set { actions = value; }
        }

        public byte Id
        {
            get { return id; }
            set { id = value; }
        }

        public int SlotNo
        {
            get { return slotNo; }
            set { slotNo = value; }
        }

        public TimeSpan Time
        {
            get { return time; }
            set { time = value; }
        }

        public int HpPercentage
        {
            get { return hpPercentage; }
            set { hpPercentage = value; }
        }

        public PlayerColor Color
        {
            get { return color; }
            set { color = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Race Race
        {
            get { return race; }
            set { race = value; }
        }

        public int TeamNo
        {
            get { return teamNo; }
            set { teamNo = value; }
        }

        public bool IsObserver
        {
            get { return isObserver; }
            set { isObserver = value; }
        }

        public bool IsComputer
        {
            get { return isComputer; }
            set { isComputer = value; }
        }
    }
}