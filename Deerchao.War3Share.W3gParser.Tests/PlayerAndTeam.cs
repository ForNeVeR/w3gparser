using System;
using Deerchao.War3Share;
using Deerchao.War3Share.W3gParser;
using NUnit.Framework;

namespace Deerchao.War3Share.W3gParser.Tests
{
    [TestFixture]
    public class PlayerAndTeam
    {
        [Test]
        public void Rep118Ladder()
        {
            Replay rep = new Replay(@"data\1.18-ladder.w3g");
            Assert.AreEqual(2, rep.Players.Count);

            Assert.AreEqual("MKpowa", rep.Players[0].Name);
            Assert.AreEqual(0, rep.Players[0].TeamNo);
            Assert.AreSame(rep.Host, rep.Players[0]);
            Assert.AreEqual(Race.Random, rep.Players[0].Race);

            Assert.AreEqual("KrawieC.", rep.Players[1].Name);
            Assert.AreEqual(1, rep.Players[1].TeamNo);
            Assert.AreEqual(Race.NightElf, rep.Players[1].Race);
        }

        [Test]
        public void Rep118()
        {
            Replay rep = new Replay(@"data\1.18.w3g");
            Assert.AreEqual(6, rep.Players.Count);

            Assert.AreEqual("a-L.MSI.SeeK", rep.Players[0].Name);
            //Observer
            Assert.AreEqual(12, rep.Players[0].TeamNo);

            Assert.AreEqual("a-L.MSI.BrO", rep.Players[1].Name);
            //Observer
            Assert.AreEqual(12, rep.Players[1].TeamNo);

            Assert.AreEqual("omg_hi2u_asl", rep.Players[2].Name);
            //Observer
            Assert.AreEqual(12, rep.Players[2].TeamNo);

            Assert.AreEqual("a-L.MSI.ChuaEr", rep.Players[3].Name);
            //Player 1
            Assert.AreEqual(0, rep.Players[3].TeamNo);
            Assert.AreEqual(Race.Undead, rep.Players[3].Race);

            Assert.AreEqual("FoH)Bluedragoon", rep.Players[4].Name);
            //Player 2
            Assert.AreEqual(1, rep.Players[4].TeamNo);
            Assert.AreEqual(Race.Human, rep.Players[4].Race);

            Assert.AreEqual("FoH)Monocle-", rep.Players[5].Name);
            //Observer
            Assert.AreEqual(12, rep.Players[5].TeamNo);

            Assert.AreEqual(5794, rep.Players[3].Actions);
            Assert.AreEqual(4215, rep.Players[4].Actions);
            Assert.AreEqual(0, rep.Players[0].Actions);

            Assert.AreEqual(160.67131f, rep.Players[3].Apm);
        }

        [Test]
        public void Rep121FFA()
        {
            Replay rep = new Replay(@"data\1.21-ffa.w3g");

            Assert.AreEqual(4, rep.Players.Count);

            Assert.AreEqual("", rep.Players[0].Name);
            Assert.AreEqual(Race.Undead, rep.Players[0].Race);

            Assert.AreEqual("", rep.Players[1].Name);
            Assert.AreEqual(Race.Random, rep.Players[1].Race);

            Assert.AreEqual("", rep.Players[2].Name);
            Assert.AreEqual(Race.NightElf, rep.Players[2].Race);

            Assert.AreEqual("KoD-EleMenTaL", rep.Players[3].Name);
            Assert.AreEqual(Race.NightElf, rep.Players[3].Race);
        }

        [Test]
        public void Rep120()
        {
            Replay rep = new Replay(@"data\1.20.w3g");
            Assert.AreEqual(9, rep.Players.Count);

            Assert.AreEqual(Race.Human, rep.Players[1].Race);
            //this is right. w3g master is wrong.
            Assert.AreEqual(PlayerColor.Yellow, rep.Players[1].Color);
            Assert.AreEqual(100, rep.Players[1].HpPercentage);
            //this time may be wrong
            Assert.AreEqual(new TimeSpan(0, 0, 22, 29, 616), rep.Players[1].Time);

            Assert.AreEqual(4361, rep.Players[1].Actions);
            Assert.AreEqual(6260, rep.Players[7].Actions);

            Assert.AreEqual(Race.Undead, rep.Players[7].Race);
            Assert.AreEqual(PlayerColor.Gray, rep.Players[7].Color);
            Assert.AreEqual(100, rep.Players[2].HpPercentage);
        }
    }
}