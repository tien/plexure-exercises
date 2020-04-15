using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Moq;
using Moq.Protected;
using PlexureExercises.CouponApi.Utils;
using Xunit;

namespace PlexureExercises.CouponApi.Tests.Utils
{
    public class FooTests
    {
        private static Fixture _fixture = new Fixture();
        private static Uri _baseUrl = new Uri("http://test.com/");

        static FooTests()
        {
            _fixture.Customizations.Add(new TypeRelay(
                typeof(HttpContent),
                typeof(StringContent)
            ));
        }

        [Fact]
        public async Task ShouldReturnAggregatedContentLength()
        {
            var responses = _fixture.CreateMany<HttpResponseMessage>(3);

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock
                .Get(httpMessageHandler)
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(responses.ElementAt(0)))
                .Returns(Task.FromResult(responses.ElementAt(1)))
                .Returns(Task.FromResult(responses.ElementAt(2)));

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = _baseUrl
            };

            var expectedAggregatedContentLength = responses
                .Sum(response => response.Content.Headers.ContentLength);

            var foo = new Foo(httpClient);

            var sut = await foo.TripleRequestsAsync();

            sut.Should().Equals(expectedAggregatedContentLength);
        }

        [Fact]
        public async Task ShouldBeCancellable()
        {
            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            var httpClient = new HttpClient()
            {
                BaseAddress = _baseUrl
            };

            var foo = new Foo(httpClient);

            using var cts = new CancellationTokenSource();

            cts.Cancel();

            Func<Task<long>> act = () => foo.TripleRequestsAsync(cts.Token);

            await act.Should().ThrowExactlyAsync<TaskCanceledException>();
        }
    }
}