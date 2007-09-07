namespace Deerchao.War3Share.W3gParser
{
    public class OrderItem
    {
        private readonly string name;
        private readonly int time;
        private readonly bool isCancel;

        internal OrderItem(string name, int time, bool isCancel)
        {
            this.name = name;
            this.time = time;
            this.isCancel = isCancel;
        }

        internal OrderItem(string name, int time)
            : this(name, time, false)
        {

        }

        public string Name
        {
            get { return name; }
        }

        public int Time
        {
            get { return time; }
        }

        public bool IsCancel
        {
            get { return isCancel; }
        }
    }
}
