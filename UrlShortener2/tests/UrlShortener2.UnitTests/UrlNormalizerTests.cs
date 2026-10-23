using UrlShortener2.Utils;
using Xunit;

namespace UrlShortener2.UnitTests
{
	public class UrlNormalizerTests
	{
		[Fact]
		public void Normalize_LowercasesSchemeAndHost()
		{
			var uri = new Uri("HTTPS://Example.COM/Path");

			var normalized = UrlNormalizer.Normalize(uri);

			Assert.Equal("https://example.com/Path", normalized);
		}

		[Fact]
		public void Normalize_StripsTrailingSlashOnRootPathOnly()
		{
			var rootWithSlash = UrlNormalizer.Normalize(new Uri("https://example.com/"));
			var rootWithoutSlash = UrlNormalizer.Normalize(new Uri("https://example.com"));

			Assert.Equal(rootWithSlash, rootWithoutSlash);
			Assert.Equal("https://example.com", rootWithSlash);
		}

		[Fact]
		public void Normalize_KeepsTrailingSlashOnNonRootPath()
		{
			// Only the root path's trailing slash is stripped - "/foo/" and "/foo" stay different paths.
			var withSlash = UrlNormalizer.Normalize(new Uri("https://example.com/foo/"));
			var withoutSlash = UrlNormalizer.Normalize(new Uri("https://example.com/foo"));

			Assert.NotEqual(withSlash, withoutSlash);
		}

		[Fact]
		public void Normalize_PreservesQueryString()
		{
			var normalized = UrlNormalizer.Normalize(new Uri("https://example.com/path?a=1&b=2"));

			Assert.Equal("https://example.com/path?a=1&b=2", normalized);
		}

		[Fact]
		public void Normalize_OmitsDefaultPortButKeepsNonDefaultPort()
		{
			var defaultPort = UrlNormalizer.Normalize(new Uri("https://example.com:443/path"));
			var customPort = UrlNormalizer.Normalize(new Uri("https://example.com:8443/path"));

			Assert.Equal("https://example.com/path", defaultPort);
			Assert.Equal("https://example.com:8443/path", customPort);
		}

		[Fact]
		public void Normalize_TreatsCaseAndTrailingRootSlashVariantsAsEqual()
		{
			var a = UrlNormalizer.Normalize(new Uri("https://EXAMPLE.com/"));
			var b = UrlNormalizer.Normalize(new Uri("HTTPS://example.COM"));

			Assert.Equal(a, b);
		}
	}
}
