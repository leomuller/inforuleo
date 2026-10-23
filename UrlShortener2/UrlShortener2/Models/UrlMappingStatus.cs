namespace UrlShortener2.Models
{
	// Matches the lt_status lookup table. 1 is reserved.
	public enum UrlMappingStatus : short
	{
		Initializing = 2,
		Active = 3,
		Archived = 4,
	}
}
