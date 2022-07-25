using System.Diagnostics.CodeAnalysis;

namespace UrlShortener_TechAssessment.Models
{
    [ExcludeFromCodeCoverage]
    public class SiteUrlStoreDatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
    }
}
