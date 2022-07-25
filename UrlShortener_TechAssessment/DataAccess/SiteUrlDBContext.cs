using MongoDB.Driver;
using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.DataAccess
{
    public class SiteUrlDBContext: ISiteUrlDBContext
    {
        private IMongoDatabase _db { get; set; }
        private MongoClient _mongoClient { get; set; }

        private string CollectionName { get; set; }
        public SiteUrlDBContext(IDatabaseSettings settings)
        {
            _mongoClient = new MongoClient(settings.ConnectionString);
            _db = _mongoClient.GetDatabase(settings.DatabaseName);
            CollectionName = settings.CollectionName;
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>(CollectionName);
        }
    }
}
