namespace UrlShortener2.Models
{
	public class UrlMapping
	{
		public long MappingId { get; set; }

		// Null while status is "initializing" - assigned once we know MappingId (status becomes "active").
		public string? UrlShort { get; set; }

		// Original URL as submitted.
		public string UrlLong { get; set; }

		// Normalized form (trimmed, lowercase scheme/host, no trailing slash on root path) used for the unique index.
		public string UrlLongNormalized { get; set; }

		public DateTime DateCreated { get; set; }

		public DateTime? DateLastUsed { get; set; }

		// FK to lt_status.
		public UrlMappingStatus StatusId { get; set; }
	}
}
