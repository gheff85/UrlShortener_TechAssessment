using MongoDB.Driver;

namespace UrlShortener_TechAssessment.DataAccess
{
    public interface ISiteUrlDBContext
    {
        IMongoCollection<T> GetCollection<T>();
    }
}
