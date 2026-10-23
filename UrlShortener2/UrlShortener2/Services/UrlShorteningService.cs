using Microsoft.EntityFrameworkCore;
using Npgsql;
using UrlShortener2.Data;
using UrlShortener2.Models;
using UrlShortener2.Utils;

namespace UrlShortener2.Services
{
	public class UrlShorteningService
	{
		private readonly AppDbContext _db;
		private readonly ILogger<UrlShorteningService> _logger;

		public UrlShorteningService(AppDbContext db, ILogger<UrlShorteningService> logger)
		{
			_db = db;
			_logger = logger;
		}

		// Returns the existing mapping if this URL (by its normalized form) was already shortened,
		// otherwise creates a new one. wasCreated tells the caller which happened (201 vs 200).
		public async Task<(UrlMapping Mapping, bool WasCreated)> GetOrCreateAsync(string longUrl, Uri uri)
		{
			var normalized = UrlNormalizer.Normalize(uri);

			var existing = await _db.UrlMappings.SingleOrDefaultAsync(m => m.UrlLongNormalized == normalized);
			if (existing != null)
			{
				_logger.LogInformation(
					"Idempotent hit for normalized URL {NormalizedUrl}: returning existing ShortCode={ShortCode}",
					normalized, existing.UrlShort);
				return (existing, false);
			}

			var mapping = new UrlMapping
			{
				UrlLong = longUrl,
				UrlLongNormalized = normalized,
				DateCreated = DateTime.UtcNow,
				StatusId = UrlMappingStatus.Initializing,
				UrlShort = null,
			};

			_db.UrlMappings.Add(mapping);

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException ex) when (IsUniqueViolation(ex))
			{
				// Someone else inserted the same normalized URL between our check and our insert -
				// the DB's unique index is what actually decided this; fetch and return their row instead.
				var winner = await _db.UrlMappings.SingleAsync(m => m.UrlLongNormalized == normalized);
				_logger.LogInformation(
					"Lost creation race for normalized URL {NormalizedUrl}: using concurrently-created ShortCode={ShortCode}",
					normalized, winner.UrlShort);
				return (winner, false);
			}

			// Only now do we know MappingId, so only now can the short code be computed.
			mapping.UrlShort = Base62.Encode(mapping.MappingId);
			mapping.StatusId = UrlMappingStatus.Active;
			await _db.SaveChangesAsync();

			_logger.LogInformation(
				"Created new short URL: MappingId={MappingId} ShortCode={ShortCode} NormalizedUrl={NormalizedUrl}",
				mapping.MappingId, mapping.UrlShort, normalized);

			return (mapping, true);
		}

		// Looks up a mapping by its short code, for the redirect endpoint. Returns null if unknown,
		// malformed, or not Active (e.g. still Initializing or already Archived).
		public async Task<UrlMapping?> ResolveAsync(string shortCode)
		{
			long mappingId;
			try
			{
				mappingId = Base62.Decode(shortCode);
			}
			catch (ArgumentException)
			{
				// not valid base62 (or empty) - can't possibly be a real short code.
				_logger.LogWarning("Resolve failed: {ShortCode} is not a valid base62 short code", shortCode);
				return null;
			}

			var mapping = await _db.UrlMappings.SingleOrDefaultAsync(
				m => m.MappingId == mappingId && m.StatusId == UrlMappingStatus.Active);

			if (mapping == null)
			{
				_logger.LogInformation(
					"Resolve miss: no active mapping for ShortCode={ShortCode} (decoded MappingId={MappingId})",
					shortCode, mappingId);
				return null;
			}

			//save date last date used - even though this is not really in scope of assignment.
			mapping.DateLastUsed = DateTime.UtcNow;
			await _db.SaveChangesAsync();

			_logger.LogInformation("Resolved ShortCode={ShortCode} to MappingId={MappingId}", shortCode, mappingId);

			return mapping;
		}

		// Diagnostic lookup - the full stored row behind a short code, regardless of status, no side effects.
		public async Task<UrlMapping?> GetDetailsAsync(string shortCode)
		{
			long mappingId;
			try
			{
				mappingId = Base62.Decode(shortCode);
			}
			catch (ArgumentException)
			{
				return null;
			}

			return await _db.UrlMappings.SingleOrDefaultAsync(m => m.MappingId == mappingId);
		}

		private static bool IsUniqueViolation(DbUpdateException ex)
		{
			return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
		}
	}
}
