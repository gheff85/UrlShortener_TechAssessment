using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.Services
{
    public interface IShortUrlsService
    {
        SiteUrl CreateShortUrl(string siteUrl);
        string RetrieveOriginalUrl(string shortUrl);
    }
}
