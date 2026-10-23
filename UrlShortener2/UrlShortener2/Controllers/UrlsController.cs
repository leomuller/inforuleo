using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener2.Controllers
{
	[ApiController]
	public class UrlsController : ControllerBase
	{
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
