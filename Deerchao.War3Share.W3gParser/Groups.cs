using System.Collections.Generic;

namespace Deerchao.War3Share.W3gParser
{
    internal class Groups
    {
        private readonly Dictionary<byte, short> groups = new Dictionary<byte, short>();

        public void SetGroup(byte groupNo, short unitCount)
        {
            if (groups.ContainsKey(groupNo))
                groups[groupNo] = unitCount;
            else
                groups.Add(groupNo, unitCount);
        }

        public short this[byte groupNo]
        {
            get
            {
                if (!groups.ContainsKey(groupNo))
                    return 0;
                return groups[groupNo];
            }
        }
    }
}