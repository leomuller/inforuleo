using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener2.Dtos;
using UrlShortener2.Services;
using UrlShortener2.Utils;

namespace UrlShortener2.Controllers
{
	[ApiController]
	public class UrlsController : ControllerBase
	{
		private readonly UrlShorteningService _urlShorteningService;
		private readonly string _baseShortUrl;

		public UrlsController(UrlShorteningService urlShorteningService, IConfiguration configuration)
		{
			_urlShorteningService = urlShorteningService;
			_baseShortUrl = configuration["BaseShortUrl"]!;
		}

		// POST /api/v1/urls
		[HttpPost("api/v1/urls")]
		public async Task<ActionResult<ShortenUrlResponse>> ShortenUrl([FromBody] ShortenUrlRequest request)
		{
			if (UrlValidator.TryValidate(request.LongUrl, out var uri, out var error) == false)
			{
				return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid longUrl", detail: error);
			}

			var (mapping, wasCreated) = await _urlShorteningService.GetOrCreateAsync(request.LongUrl, uri!);

			var response = new ShortenUrlResponse
			{
				ShortCode = mapping.UrlShort!,
				ShortUrl = $"{_baseShortUrl}/{mapping.UrlShort}",
				LongUrl = mapping.UrlLong,
				CreatedAt = mapping.DateCreated,
			};

			return wasCreated ? Created(response.ShortUrl, response) : Ok(response);
		}

		// GET /{shortCode}
		[HttpGet("/{shortCode}")]
		public async Task<IActionResult> Resolve(string shortCode)
		{
			var mapping = await _urlShorteningService.ResolveAsync(shortCode);
			if (mapping == null)
			{
				return Problem(statusCode: StatusCodes.Status404NotFound, title: "Short code not found", detail: $"No URL found for code '{shortCode}'.");
			}

			return Redirect(mapping.UrlLong);
		}
	}
}
