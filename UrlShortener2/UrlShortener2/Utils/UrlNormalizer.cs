namespace UrlShortener2.Utils
{
	public static class UrlNormalizer
	{
		// trim + lowercase scheme/host already guaranteed by the caller having parsed a valid Uri;
		// this strips the trailing slash on the root path only, e.g. "https://Example.com/" -> "https://example.com".
		public static string Normalize(Uri uri)
		{
			var scheme = uri.Scheme.ToLowerInvariant();
			var host = uri.Host.ToLowerInvariant();
			var portPart = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";

			var path = uri.AbsolutePath;
			if (path == "/")
			{
				path = string.Empty;
			}

			return $"{scheme}://{host}{portPart}{path}{uri.Query}{uri.Fragment}";
		}
	}
}
