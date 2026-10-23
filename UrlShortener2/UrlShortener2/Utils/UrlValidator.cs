namespace UrlShortener2.Utils
{
	public static class UrlValidator
	{
		private const int MaxLength = 2048;

		public static bool TryValidate(string? url, out string? error)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				error = "longUrl is required.";
				return false;
			}

			if (url.Length > MaxLength)
			{
				error = $"longUrl must not exceed {MaxLength} characters.";
				return false;
			}

			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
				(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
			{
				error = "longUrl must be an absolute http or https URL.";
				return false;
			}

			error = null;
			return true;
		}
	}
}
