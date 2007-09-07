using NUnit.Framework;

namespace Deerchao.War3Share.W3gParser.Tests
{
    [TestFixture]
    public class HeroAndAbility
    {
        [Test]
        public void Hero118()
        {
            Replay rep = new Replay(@"data\1.18.w3g");
            Assert.AreEqual(6, rep.Players.Count);

            Player player1 = null;
            Player player2 = null;

            foreach (Player p in rep.Players)
            {
                if (p.IsObserver)
                    continue;
                if (player1 == null)
                    player1 = p;
                else
                    player2 = p;
            }


            Assert.AreEqual(3, player1.Heroes.Count);

            Assert.AreEqual(5, player1.Heroes["Udea"].Level);

            Assert.AreEqual(2, player2.Heroes["Npbm"].Level);
        }
    }
}