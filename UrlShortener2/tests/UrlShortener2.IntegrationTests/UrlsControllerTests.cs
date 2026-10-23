using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UrlShortener2.Dtos;
using UrlShortener2.IntegrationTests.Infrastructure;
using Xunit;

namespace UrlShortener2.IntegrationTests
{
	[Collection("Integration")]
	public class UrlsControllerTests
	{
		private readonly HttpClient _client;

		public UrlsControllerTests(CustomWebApplicationFactory factory)
		{
			// Don't auto-follow the 302 from Resolve - we need to inspect the redirect response itself.
			_client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		}

		private static string UniqueUrl() => $"https://example.com/test/{Guid.NewGuid()}";

		[Fact]
		public async Task Create_NewUrl_Returns201WithShortCode()
		{
			var longUrl = UniqueUrl();

			var response = await _client.PostAsJsonAsync("/api/v1/urls", new { longUrl });

			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			Assert.NotNull(response.Headers.Location);

			var body = await response.Content.ReadFromJsonAsync<ShortenUrlResponse>();
			Assert.NotNull(body);
			Assert.False(string.IsNullOrEmpty(body!.ShortCode));
			Assert.Equal(longUrl, body.LongUrl);
		}

		[Fact]
		public async Task Create_SameUrlAgain_ReturnsSameShortCode()
		{
			var longUrl = UniqueUrl();

			var first = await _client.PostAsJsonAsync("/api/v1/urls", new { longUrl });
			var firstBody = await first.Content.ReadFromJsonAsync<ShortenUrlResponse>();

			var second = await _client.PostAsJsonAsync("/api/v1/urls", new { longUrl });
			var secondBody = await second.Content.ReadFromJsonAsync<ShortenUrlResponse>();

			Assert.Equal(HttpStatusCode.Created, first.StatusCode);
			Assert.Equal(HttpStatusCode.OK, second.StatusCode);
			Assert.Equal(firstBody!.ShortCode, secondBody!.ShortCode);
		}

		[Fact]
		public async Task Create_InvalidUrl_Returns400()
		{
			var response = await _client.PostAsJsonAsync("/api/v1/urls", new { longUrl = "not a url" });

			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
		}

		[Fact]
		public async Task Resolve_KnownShortCode_RedirectsToLongUrl()
		{
			var longUrl = UniqueUrl();
			var createResponse = await _client.PostAsJsonAsync("/api/v1/urls", new { longUrl });
			var created = await createResponse.Content.ReadFromJsonAsync<ShortenUrlResponse>();

			var resolveResponse = await _client.GetAsync($"/{created!.ShortCode}");

			Assert.Equal(HttpStatusCode.Found, resolveResponse.StatusCode);
			Assert.Equal(longUrl, resolveResponse.Headers.Location?.ToString());
		}

		[Fact]
		public async Task Resolve_UnknownShortCode_Returns404()
		{
			var response = await _client.GetAsync("/zzzzzz");

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
			Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
		}
	}
}
