using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PlexureExercises.CouponApi.Utils
{
    public class Foo
    {
        private readonly HttpClient _httpClient;
        public Foo(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<long> TripleRequestsAsync(CancellationToken cancellationToken = default)
        {
            var results = await Task.WhenAll(
                _httpClient.GetAsync("/man/bear/pig", cancellationToken),
                _httpClient.GetAsync("/pig/bear/man", cancellationToken),
                _httpClient.GetAsync("/bear/pig/man", cancellationToken));

            return results
                .Sum(result => result.Content.Headers.ContentLength ?? 0);
        }
    }
}