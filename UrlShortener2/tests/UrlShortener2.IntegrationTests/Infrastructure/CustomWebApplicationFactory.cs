using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace UrlShortener2.IntegrationTests.Infrastructure
{
	// One Postgres container, shared across every integration test class via the "Integration" collection -
	// starting a fresh container per test would work too but is much slower for little extra isolation here,
	// since each test uses its own unique (Guid-based) URL and never touches another test's rows.
	public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
	{
		private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
			.WithImage("postgres:16")
			.WithDatabase("inforu_test")
			.WithUsername("postgres")
			.WithPassword("postgres")
			.Build();

		public async Task InitializeAsync()
		{
			await _postgres.StartAsync();

			await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
			await connection.OpenAsync();
			await using var command = new NpgsqlCommand(SchemaScript.Sql, connection);
			await command.ExecuteNonQueryAsync();
		}

		// WebApplicationFactory already has a `ValueTask DisposeAsync()` (IAsyncDisposable); xUnit's IAsyncLifetime
		// wants `Task DisposeAsync()`. Same name, different return type can't coexist as a normal overload,
		// so this is implemented explicitly to avoid the clash.
		async Task IAsyncLifetime.DisposeAsync()
		{
			await _postgres.DisposeAsync();
			await base.DisposeAsync();
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureAppConfiguration((_, config) =>
			{
				config.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["ConnectionStrings:UrlMapperDB"] = _postgres.GetConnectionString(),
				});
			});
		}
	}
}
