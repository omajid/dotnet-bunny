using System;
using System.IO;
using System.Linq;

using Xunit;

namespace Turkey.Tests
{
    public class CleanerTest
    {
        [Fact]
        public void StarAtEndIsExpandedCorrectly()
        {
            var temp = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            var testRoot = Path.Combine(temp, "turkey-test-" + new Random().Next());
            Directory.CreateDirectory(testRoot);

            try
            {
                var testDir = Path.Combine(testRoot, ".dotnet");
                Directory.CreateDirectory(testDir + "1");
                Directory.CreateDirectory(testDir + "2");

                var expanded = Cleaner.ExpandPath(testDir + "*");

                Assert.Equal(new string[] { testDir + "1", testDir + "2" }.OrderBy(s => s),
                            expanded.OrderBy(s => s));
            }
            finally
            {
                Directory.Delete(testRoot, true);
            }
        }

        [Fact]
        public void TildeIsExpandedToUserHome()
        {
            var expanded = Cleaner.ExpandPath("~");
            Assert.Single<string>(expanded);
            Assert.Equal(Environment.GetEnvironmentVariable("HOME"), expanded.First());
        }
    }
}
