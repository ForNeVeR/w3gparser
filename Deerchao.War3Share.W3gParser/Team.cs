using System.Collections.Generic;

namespace Deerchao.War3Share.W3gParser
{
    public class Team
    {
        private readonly int teamNo;
        private bool isObserver;
        public Team(int no)
        {
            teamNo = no;
        }

        public int TeamNo
        {
            get { return teamNo; }
        }

        public bool IsObserver
        {
            get { return isObserver; }
        }

        public List<Player> Players
        {
            get { return players; }
        }

        readonly List<Player> players = new List<Player>();

        public void Add(Player p)
        {
            players.Add(p);
            isObserver = p.IsObserver;
        }
    }
}