using System.IO;

namespace Deerchao.War3Share.Utility
{
    public static class BinaryHelper
    {
        public static string ReadSZString(BinaryReader reader)
        {
            string result = "";
            byte b = reader.ReadByte();
            while (b != 0)
            {
                result += (char)b;
                b = reader.ReadByte();
            }
            return result;
        }

        public static int ReadAllBytesFromStream(Stream stream, byte[] buffer)
        {
            int offset = 0;
            int totalCount = 0;
            while (true)
            {
                int bytesRead = stream.Read(buffer, offset, 100);
                if (bytesRead == 0)
                {
                    break;
                }
                offset += bytesRead;
                totalCount += bytesRead;
            }
            return totalCount;
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            if (stream.Length < int.MaxValue)
                return reader.ReadBytes((int)stream.Length);
            throw new ParserException("stream.Length > int.MaxValue");
        }

        public static uint GetUInt32(byte[] data, int position)
        {
            uint result = 0;
            result |= data[position];
            result |= (uint)data[position + 1] << 8;
            result |= (uint)data[position + 2] << 16;
            result |= (uint)data[position + 3] << 24;
            return result;
        }

        public static string GetString(byte[] stream, int position)
        {
            string result = "";
            while (stream[position] != 0)
            {
                result += (char)stream[position];
                position++;
            }
            return result;
        }
    }
}
