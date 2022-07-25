using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.DataAccess
{
    public interface ISiteUrlRepository
    {
        void InsertNewSiteUrl(SiteUrl siteUrl);
        SiteUrl FindOriginalUrl(string url);
        SiteUrl FindShortUrl(string url);
    }
}
