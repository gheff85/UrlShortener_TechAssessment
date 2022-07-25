using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace UrlShortener_TechAssessment.Models
{
    [ExcludeFromCodeCoverage]
    public class SiteUrl
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string ShortUrl { get; set; } = null!;
        public string OriginalUrl { get; set; } = null!;
    }
}
