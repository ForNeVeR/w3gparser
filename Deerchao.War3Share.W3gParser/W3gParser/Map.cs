using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Deerchao.War3Share.W3gParser
{
    public class W3gMap
    {
        public W3gMap(uint checksum, string path)
        {
            this.checkSum = checksum;
            this.path = path;
        }

        string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        long checkSum;

        public long CheckSum
        {
            get { return checkSum; }
            set { checkSum = value; }
        }

        public string GetName()
        {
            if (path == null)
                return null;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            return Regex.Replace(fileName, @"\(\d+\)", "");
        }
    }
}
