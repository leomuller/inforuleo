namespace UrlShortener2.Dtos
{
	public class ShortenUrlResponse
	{
		public string ShortCode { get; set; }
		public string ShortUrl { get; set; }
		public string LongUrl { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
