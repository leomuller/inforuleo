using Xunit;

namespace UrlShortener2.IntegrationTests.Infrastructure
{
	[CollectionDefinition("Integration")]
	public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
	{
	}
}
