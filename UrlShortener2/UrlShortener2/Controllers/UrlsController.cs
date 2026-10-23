using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlShortener2.Utils;

namespace UrlShortener2.Controllers
{
	[ApiController]
	public class UrlsController : ControllerBase
	{

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
			return UrlValidator.TryValidate(url, out var error) ? "valid" : error;
		}

		// POST /api/v1/urls
		[HttpPost("api/v1/urls")]
		public ActionResult<ShortenUrlResponse> ShortenUrl([FromBody] ShortenUrlRequest request)
		{
			// TODO: validate, normalize, generate/lookup short code, persist, return 201/200

			//Microsoft.AspNetCore.Mvc.ProblemDetails //for errors.


			throw new NotImplementedException();
		}

		// GET /{shortCode}
		[HttpGet("/{shortCode}")]
		public IActionResult Resolve(string shortCode)
		{
			// TODO: look up long URL by shortCode, 302 redirect or 404



			throw new NotImplementedException();
		}
	}

	public class ShortenUrlRequest
	{
		public string LongUrl { get; set; }
	}

	public class ShortenUrlResponse
	{
		public string ShortCode { get; set; }
		public string ShortUrl { get; set; }
		public string LongUrl { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	
}
