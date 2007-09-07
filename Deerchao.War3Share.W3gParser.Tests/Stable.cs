using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Deerchao.War3Share.W3gParser.Tests
{
    [TestFixture]
    public class Stable
    {
        string path;
        private int count = 0;
        private readonly Stopwatch watch = new Stopwatch();

        [Test]
        public void ParseReplays()
        {
            watch.Start();
            path = @"E:\work\dowloaded replays\error";
            ParseDir();
            path = @"E:\work\dowloaded replays\wcreplays";
            ParseDir();
            path = @"E:\work\dowloaded replays\ogame";
            ParseDir();
            watch.Stop();

            Console.WriteLine("{0} replays parsed, in {1}s, average {2} ms per replay", count, watch.Elapsed.TotalSeconds, watch.Elapsed.TotalMilliseconds / count);
        }

        private void ParseDir()
        {
            string[] files = Directory.GetFiles(path, "*.w3g");

            foreach (string file in files)
            {
                Replay replay = new Replay(file);
                Console.WriteLine(file + ":" + replay.Hash);
                count++;
            }
        }
    }
}