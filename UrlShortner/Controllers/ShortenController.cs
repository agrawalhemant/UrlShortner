using Microsoft.AspNetCore.Mvc;
using UrlShortner.Process.Contracts;

namespace UrlShortner.Controllers
{
    [ApiController]
    public class ShortenController : Controller
    {
        private readonly IShortenProcess _shortenProcess;
        public ShortenController(IShortenProcess shortenProcess)
        {
            _shortenProcess = shortenProcess;
        }

        /// <summary>
        /// get health status of app
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public IActionResult Status()
        {
            return Ok($"I am up and running");
        }

        [HttpPost("")]
        public async Task<IActionResult> GetShortUrl([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("empty url");
            }
            string shortUrl = string.Empty;
            try
            {
                shortUrl = await _shortenProcess.GetShortenUrl(url);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(shortUrl);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetOriginalUrl(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("empty code");
            }
            try
            {
                var (status, originalUrl) = await _shortenProcess.GetOriginalUrl(code);
                if (!status)
                {
                    return NotFound("Not found");
                }
                return Redirect(originalUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
