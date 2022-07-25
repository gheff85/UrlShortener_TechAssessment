using MongoDB.Driver;
using System.Text;
using UrlShortener_TechAssessment.DataAccess;
using UrlShortener_TechAssessment.Exceptions;
using UrlShortener_TechAssessment.Models;

namespace UrlShortener_TechAssessment.Services
{
    public class ShortUrlsService : IShortUrlsService
    {
        private readonly ISiteUrlRepository _siteUrlRepository;
        private readonly IConfiguration _configuration;

        private struct DBOperation
        {
            public DBOperationType OperationType;
            public SiteUrl DataForInsert;
            public string Url;
        }

        private enum DBOperationType
        {
            INSERT_NEW_SITEURL,
            CHECK_IF_ORIGINAL_EXISTS,
            CHECK_IF_SHORT_URL_EXISTS
        }

        public ShortUrlsService(ISiteUrlRepository siteUrlRepository, IConfiguration configuration)
        {
            _siteUrlRepository = siteUrlRepository;
            _configuration = configuration;
        }

        public SiteUrl CreateShortUrl(string originalUrl)
        {
            string sanitisedOriginalUrl = ValidateAndSanitiseUrl(originalUrl);
            SiteUrl siteUrl;
            while (true)
            {
                siteUrl = GenerateRandomShortUrl(sanitisedOriginalUrl);

                if (!IsShortUrlAlreadyPresent(siteUrl.ShortUrl))
                {
                    break;
                }
            }

            PerformDatabaseOperation(new() { OperationType = DBOperationType.INSERT_NEW_SITEURL , DataForInsert= siteUrl });
            return siteUrl;
        }

        public string RetrieveOriginalUrl(string shortUrl)
        {
            SiteUrl siteUrl = _siteUrlRepository.FindShortUrl(shortUrl);

            if (siteUrl is null)
            {
                throw new UrlNotFoundException();
            }

            return siteUrl.OriginalUrl;
        }

        private string ValidateAndSanitiseUrl(string originalUrl)
        {
            if (!originalUrl.StartsWith("http://") && !originalUrl.StartsWith("https://"))
            {
                throw new InvalidUrlFormatException("URL must begin with http:// or https://");
            }

            string sanitisedUrl = originalUrl;


            //Chose not to simplify below for readability.
            //Using IndexOf as length as we are starting at beginning of string,
            //therefore indexOf corresponds to length in this case
            if (sanitisedUrl.Contains('?'))
            {
                sanitisedUrl = sanitisedUrl.Substring(0, originalUrl.IndexOf('?'));
            }

            sanitisedUrl = sanitisedUrl.Replace("\n", "").Replace("\r", "");

            if (IsOriginalAlreadyPresent(sanitisedUrl, out SiteUrl siteUrl))
            {
                throw new UrlAlreadyPresentException(siteUrl);
            }


            return sanitisedUrl;
        }

        private bool IsOriginalAlreadyPresent(string originalUrl, out SiteUrl siteUrl)
        {
            siteUrl = PerformDatabaseOperation(new() { OperationType = DBOperationType.CHECK_IF_ORIGINAL_EXISTS, Url = originalUrl });

            return (siteUrl is not null);
        }

        private bool IsShortUrlAlreadyPresent(string shortUrl)
        {
            SiteUrl siteUrl = PerformDatabaseOperation(new() { OperationType = DBOperationType.CHECK_IF_SHORT_URL_EXISTS, Url = shortUrl });

            return (siteUrl is not null);
        }

        private SiteUrl GenerateRandomShortUrl(string originalUrl)
        {
            return new()
            {
                OriginalUrl = originalUrl,
                ShortUrl = GenerateRandom6CharacterStringFromAllowableCharacterSet()
            };
        }

        private string GenerateRandom6CharacterStringFromAllowableCharacterSet()
        {
            string allowableCharacterSet = _configuration.GetSection("AllowableCharacters").Value;
            short shortUrlLength = Convert.ToInt16(_configuration.GetSection("ShortUrlLength").Value);

            StringBuilder randomString = new();
            int randomIndex;

            for (int i = 0; i < shortUrlLength; i++)
            {
                randomIndex = new Random().Next(0, allowableCharacterSet.Length);
                randomString.Append(allowableCharacterSet[randomIndex]);
            }

            return randomString.ToString();
        }

        private SiteUrl PerformDatabaseOperation(DBOperation dbOperation)
        {
            int attempts = 3;

            while (attempts > 0)
            {
                try
                {
                    switch (dbOperation.OperationType)
                    {
                        case DBOperationType.INSERT_NEW_SITEURL:
                            _siteUrlRepository.InsertNewSiteUrl(dbOperation.DataForInsert);
                            return new();
                        case DBOperationType.CHECK_IF_ORIGINAL_EXISTS:
                            return _siteUrlRepository.FindOriginalUrl(dbOperation.Url);
                        case DBOperationType.CHECK_IF_SHORT_URL_EXISTS:
                            return _siteUrlRepository.FindShortUrl(dbOperation.Url);
                        default:
                            throw new InvalidOperationException();
                    }
                }
                catch (MongoException)
                {
                    attempts--;
                    if (attempts <= 0)
                    {
                        throw;
                    }
                }
            }

            return new();
        }
    }
}
