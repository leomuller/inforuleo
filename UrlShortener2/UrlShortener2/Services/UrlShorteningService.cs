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

		public UrlShorteningService(AppDbContext db)
		{
			_db = db;
		}

		// Returns the existing mapping if this URL (by its normalized form) was already shortened,
		// otherwise creates a new one. wasCreated tells the caller which happened (201 vs 200).
		public async Task<(UrlMapping Mapping, bool WasCreated)> GetOrCreateAsync(string longUrl, Uri uri)
		{
			var normalized = UrlNormalizer.Normalize(uri);

			var existing = await _db.UrlMappings.SingleOrDefaultAsync(m => m.UrlLongNormalized == normalized);
			if (existing != null)
			{
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
				return (winner, false);
			}

			// Only now do we know MappingId, so only now can the short code be computed.
			mapping.UrlShort = Base62.Encode(mapping.MappingId);
			mapping.StatusId = UrlMappingStatus.Active;
			await _db.SaveChangesAsync();

			return (mapping, true);
		}

		// Looks up a mapping by its short code, for the redirect endpoint. Returns null if unknown.
		public async Task<UrlMapping?> ResolveAsync(string shortCode)
		{
			var mapping = await _db.UrlMappings.SingleOrDefaultAsync(m => m.UrlShort == shortCode);
			if (mapping == null)
			{
				return null;
			}

			mapping.DateLastUsed = DateTime.UtcNow;
			await _db.SaveChangesAsync();

			return mapping;
		}

		private static bool IsUniqueViolation(DbUpdateException ex)
		{
			return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
		}
	}
}
