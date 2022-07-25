using MongoDB.Driver;
using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.DataAccess
{
    public class SiteUrlRepository : ISiteUrlRepository
    {
        private readonly ISiteUrlDBContext _dbContext;
        private readonly IMongoCollection<SiteUrl> _siteUrls;
        public SiteUrlRepository(ISiteUrlDBContext dBContext)
        {
            _dbContext = dBContext;
            _siteUrls = _dbContext.GetCollection<SiteUrl>();
        }
        public SiteUrl FindOriginalUrl(string url)
        {
            return _siteUrls.Find(rec => rec.OriginalUrl == url).FirstOrDefault();
        }

        public SiteUrl FindShortUrl(string url)
        {
            return _siteUrls.Find(rec => rec.ShortUrl == url).FirstOrDefault();
        }

        public void InsertNewSiteUrl(SiteUrl siteUrl)
        {
            _siteUrls.InsertOne(siteUrl);
        }
    }
}
