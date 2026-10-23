using Microsoft.EntityFrameworkCore;
using UrlShortener2.Models;

namespace UrlShortener2.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<UrlMapping> UrlMappings { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UrlMapping>(entity =>
			{
				entity.ToTable("url_mappings");

				entity.HasKey(u => u.MappingId);
				entity.Property(u => u.MappingId).HasColumnName("mapping_id").ValueGeneratedOnAdd();

				entity.Property(u => u.UrlShort).HasColumnName("url_short").HasMaxLength(50).IsRequired();
				entity.Property(u => u.UrlLong).HasColumnName("url_long").IsRequired();
				entity.Property(u => u.UrlLongNormalized).HasColumnName("url_long_normalized").IsRequired();
				entity.Property(u => u.DateCreated).HasColumnName("date_created").IsRequired();
				entity.Property(u => u.DateLastUsed).HasColumnName("date_last_used");

				entity.HasIndex(u => u.UrlShort).IsUnique();
				entity.HasIndex(u => u.UrlLongNormalized).IsUnique();
			});
		}
	}
}
