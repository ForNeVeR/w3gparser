using System;

namespace Deerchao.War3Share.Utility
{
    public class VersionHelper
    {
        public static Version FromString(string s)
        {
            string[] parts = s.Split('.');
            Version ver = new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            return ver;
        }
    }
}
