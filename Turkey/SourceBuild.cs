using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Turkey
{
    /// <summary>
    ///   Work with https://github.com/dotnet/source-build
    /// </summary>
    public class SourceBuild
    {
        private readonly HttpClient _client;

        public SourceBuild(HttpClient client)
        {
            this._client = client;
        }

        public Task<Uri> GetProdConFeedAsync(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            var branchName = "release/" + version.MajorMinor;
            return GetProdConFeedAsync(branchName);
        }

        public async Task<Uri> GetProdConFeedAsync(string branchName)
        {
            var url = new Uri($"https://raw.githubusercontent.com/dotnet/source-build/{branchName}/ProdConFeed.txt");
            var feedUrl = new Uri(await _client.GetStringAsync(url));

            using(var response = await _client.GetAsync(feedUrl))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
            }
            return feedUrl;
        }
    }
}
