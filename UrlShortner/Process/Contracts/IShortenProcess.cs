namespace UrlShortner.Process.Contracts
{
    public interface IShortenProcess
    {
        Task<string> GetShortenUrl(string url);
        Task<(bool,string)> GetOriginalUrl(string code);
    }
}
