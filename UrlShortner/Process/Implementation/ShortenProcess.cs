using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using UrlShortner.Configurations;
using UrlShortner.Data;
using UrlShortner.Process.Contracts;

namespace UrlShortner.Process.Implementation
{
    public class ShortenProcess : IShortenProcess
    {
        private readonly Random _random = new();
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CacheManager _cacheManager;
        private readonly int maxValue;
        public ShortenProcess(IHttpContextAccessor httpContextAccessor, CacheManager cacheManager, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _cacheManager = cacheManager;
            maxValue = ShortLinkSettings.AlphaNumeric.Length;
        }
        public async Task<string> GetShortenUrl(string url)
        {
            var code = GenerateUniqueCode();
            var request = _httpContextAccessor.HttpContext.Request;
            var shortenedUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                Code = code,
                OriginalUrl = url,
                ShortUrl = $"{request.Scheme}://{request.Host}/{code}",
                CreatedOnIst = DateTime.Now,
            };
            _dbContext.ShortenedUrls.Add(shortenedUrl);
            await _dbContext.SaveChangesAsync();
            _cacheManager.Set(shortenedUrl.Code, shortenedUrl.OriginalUrl);
            return shortenedUrl.ShortUrl;
        }

        public async Task<(bool,string)> GetOriginalUrl(string code)
        {
            string originalUrl = _cacheManager.Get<string>(code);
            if (!string.IsNullOrEmpty(originalUrl))
            {
                return (true, originalUrl);
            }
            var res = await _dbContext.ShortenedUrls.SingleOrDefaultAsync(s => s.Code == code);
            if (res == null)
            {
                return (false, string.Empty);
            }
            else
            {
                _cacheManager.Set(res.Code, res.OriginalUrl);
                return (true, res.OriginalUrl);
            }
        }

        private string GenerateUniqueCode()
        {
            var codeChars = new char[ShortLinkSettings.Length];

            while (true)
            {
                for (var i = 0; i < ShortLinkSettings.Length; i++)
                {
                    var randomIndex = _random.Next(maxValue);

                    codeChars[i] = ShortLinkSettings.AlphaNumeric[randomIndex];
                }

                var code = new string(codeChars);

                if (!_dbContext.ShortenedUrls.Any(u => u.Code == code))
                {
                    return code;
                }
            }
        }
    }
}
