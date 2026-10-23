namespace UrlShortener2.Utils
{
	public static class UrlValidator
	{
		private const int MaxLength = 2048;

		public static bool TryValidate(string? url, out string? error)
		{
			//check not empty.
			if (string.IsNullOrWhiteSpace(url))
			{
				error = "longUrl is required.";
				return false;
			}

			//check raw input length too - a cheap early reject before parsing, and relevant on its own
			//since this is also the length of what we'd eventually put in the redirect's Location header.
			if (url.Length > MaxLength)
			{
				error = $"longUrl must not exceed {MaxLength} characters.";
				return false;
			}

			//make sure it is a valid URL (asbolute, starting with HTTP or HTTPS
			//IsWellFormedOriginalString() rejects raw spaces/invalid characters that Uri.TryCreate
			//would otherwise silently auto-escape (e.g. "https://x.com/my path" would parse "successfully").
			if (Uri.TryCreate(url, UriKind.Absolute, out var uri) == false ||
				(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
				uri.IsWellFormedOriginalString() == false)
			{
				error = "longUrl must be an absolute http or https URL.";
				return false;
			}

			//check length against the canonical (fully-escaped) form, not the raw input -
			//escaping (e.g. spaces, unicode) can make the real URL longer than what was typed.
			if (ToCanonical(uri).Length > MaxLength)
			{
				error = $"normalized longUrl must not exceed {MaxLength} characters.";
				return false;
			}

			error = null;
			return true;
		}

		// The canonical, fully-escaped form of the URL - the actual value that gets stored/redirected to.
		private static string ToCanonical(Uri uri)
		{
			return uri.AbsoluteUri;
		}
	}
}
