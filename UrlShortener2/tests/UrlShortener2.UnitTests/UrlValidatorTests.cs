using UrlShortener2.Utils;
using Xunit;

namespace UrlShortener2.UnitTests
{
	public class UrlValidatorTests
	{
		[Theory]
		[InlineData("https://example.com/path?query=1")]
		[InlineData("http://example.com")]
		public void TryValidate_AcceptsAbsoluteHttpOrHttpsUrl(string url)
		{
			var result = UrlValidator.TryValidate(url, out var uri, out var error);

			Assert.True(result);
			Assert.NotNull(uri);
			Assert.Null(error);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void TryValidate_RejectsEmpty(string? url)
		{
			var result = UrlValidator.TryValidate(url, out var uri, out var error);

			Assert.False(result);
			Assert.Null(uri);
			Assert.NotNull(error);
		}

		[Theory]
		[InlineData("ftp://example.com/file")]
		[InlineData("not a url at all")]
		[InlineData("example.com/no-scheme")]
		public void TryValidate_RejectsNonHttpAbsoluteUrl(string url)
		{
			var result = UrlValidator.TryValidate(url, out var uri, out var error);

			Assert.False(result);
			Assert.Null(uri);
			Assert.NotNull(error);
		}

		[Fact]
		public void TryValidate_RejectsOverMaxLength()
		{
			var url = "https://example.com/" + new string('a', 2048);

			var result = UrlValidator.TryValidate(url, out var uri, out var error);

			Assert.False(result);
			Assert.Null(uri);
			Assert.NotNull(error);
		}

		[Fact]
		public void TryValidate_RejectsRawSpaceEvenThoughUriWouldAutoEscapeIt()
		{
			// Uri.TryCreate alone would happily "fix" this by escaping the space - IsWellFormedOriginalString()
			// is what catches it and makes validation reject it instead.
			var url = "https://example.com/my path";

			var result = UrlValidator.TryValidate(url, out var uri, out var error);

			Assert.False(result);
			Assert.Null(uri);
			Assert.NotNull(error);
		}
	}
}
