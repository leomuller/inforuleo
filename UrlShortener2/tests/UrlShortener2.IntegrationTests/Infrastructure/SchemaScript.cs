namespace UrlShortener2.IntegrationTests.Infrastructure
{
	// Mirrors the hand-written DB creation script for the real "inforu" database -
	// kept here as a literal so the test container's schema matches it exactly.
	internal static class SchemaScript
	{
		public const string Sql = """
			CREATE TABLE lt_status (
			    status_id   SMALLINT PRIMARY KEY,
			    status_name VARCHAR(50) NOT NULL
			);

			INSERT INTO lt_status (status_id, status_name) VALUES
			    (2, 'initializing'),
			    (3, 'active'),
			    (4, 'archived');

			CREATE TABLE url_mappings (
			    mapping_id           BIGSERIAL PRIMARY KEY,
			    url_short            VARCHAR(50)  NULL,
			    url_long             TEXT         NOT NULL,
			    url_long_normalized  TEXT         NOT NULL,
			    date_created         TIMESTAMPTZ  NOT NULL,
			    date_last_used       TIMESTAMPTZ  NULL,
			    status_id            SMALLINT     NOT NULL REFERENCES lt_status (status_id)
			);

			CREATE UNIQUE INDEX ux_url_mappings_url_long_normalized ON url_mappings (url_long_normalized);
			CREATE UNIQUE INDEX ux_url_mappings_url_short ON url_mappings (url_short) WHERE url_short IS NOT NULL;
			""";
	}
}
