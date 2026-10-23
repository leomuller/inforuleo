using Microsoft.AspNetCore.Mvc;
using UrlShortener2.Models;
using UrlShortener2.Services;
using UrlShortener2.Utils;

namespace UrlShortener2.Controllers
{
	// Scratch endpoints for trying out the Utils functions directly - not part of the assignment's API contract.
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly UrlShorteningService _urlShorteningService;

		public TestController(UrlShorteningService urlShorteningService)
		{
			_urlShorteningService = urlShorteningService;
		}

		// GET /api/v1/urls/test/encode/{value} - for trying out Base62.Encode directly
		[HttpGet("api/v1/urls/test/encode/{value}")]
		public ActionResult<string> TestEncode(long value)
		{
			return Base62.Encode(value);
		}

		// GET /api/v1/urls/test/decode/{code} - for trying out Base62.Decode directly
		[HttpGet("api/v1/urls/test/decode/{code}")]
		public ActionResult<long> TestDecode(string code)
		{
			return Base62.Decode(code);
		}

		// GET /api/v1/urls/test/validate?url=... - for trying out UrlValidator.TryValidate directly
		[HttpGet("api/v1/urls/test/validate")]
		public ActionResult<string> TestValidate([FromQuery] string url)
		{
			return UrlValidator.TryValidate(url, out _, out var error) ? "valid" : error;
		}

		// GET /api/v1/urls/test/details/{shortCode} - full stored row behind a short code, any status, no side effects
		[HttpGet("api/v1/urls/test/details/{shortCode}")]
		public async Task<ActionResult<UrlMapping>> TestDetails(string shortCode)
		{
			var mapping = await _urlShorteningService.GetDetailsAsync(shortCode);
			return mapping == null ? NotFound() : Ok(mapping);
		}

		// GET /api/v1/urls/test/throw - deliberately throws, to verify unhandled exceptions become a 500 ProblemDetails with no stack trace
		[HttpGet("api/v1/urls/test/throw")]
		public IActionResult TestThrow()
		{
			throw new InvalidOperationException("this is a deliberate test exception");
		}
	}
}
