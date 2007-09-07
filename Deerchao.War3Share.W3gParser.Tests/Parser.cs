using NUnit.Framework;

namespace Deerchao.War3Share.W3gParser.Tests
{
    [TestFixture]
    public class Parser
    {
        [Test]
        public void ZipOK()
        {
            Replay replay1 = new Replay(@"data\1.20.w3g");
            Replay replay2 = new Replay(@"data\ladder.w3g");
        }
    }
}