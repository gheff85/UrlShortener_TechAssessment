using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.Exceptions
{
    public class UrlAlreadyPresentException : Exception
    {
        public SiteUrl SiteUrl { get; } = null!;
        public UrlAlreadyPresentException(SiteUrl siteUrl)
        {
            SiteUrl = siteUrl;
        }
    }
}
