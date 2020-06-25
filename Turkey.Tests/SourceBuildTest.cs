using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Turkey.Tests
{
    public class SourceBuildTest
    {
        private static readonly Uri FAKE_FEED = new Uri("https://myget.org/my/secret/99.0/feed.json");

        public class MockHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken token)
            {
                Console.WriteLine($"{message.RequestUri.AbsoluteUri}: {message.Method}");
                if (message.Method == HttpMethod.Get)
                {

                    var url = message.RequestUri.AbsoluteUri.ToString();
                    if (url.Equals("https://raw.githubusercontent.com/dotnet/source-build/release/99.0/ProdConFeed.txt"))
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(FAKE_FEED.ToString())
                        });

                    }
                    else if (url.Equals(FAKE_FEED.ToString()))
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent("ignore")
                        });
                    }
                }
                Assert.True(false);
                return null;
            }
        }

        [Fact]
        public async Task VerifyProdConFeedIsLookedUpAndThenTheFeedIsVerifiedToResolve()
        {
            var messageHandler = new MockHandler();
            var client = new HttpClient(messageHandler);
            var sourceBuild = new SourceBuild(client);

            var feed = await sourceBuild.GetProdConFeedAsync(Version.Parse("99.0"));

            Assert.Equal(FAKE_FEED, feed);
        }

    }
}
