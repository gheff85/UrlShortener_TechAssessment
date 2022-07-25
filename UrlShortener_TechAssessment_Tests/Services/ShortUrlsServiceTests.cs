using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener_TechAssessment.DataAccess;
using UrlShortener_TechAssessment.Exceptions;
using UrlShortener_TechAssessment.Models;
using UrlShortener_TechAssessment.Services;

namespace UrlShortener_TechAssessment_Tests.Services
{
    [TestClass()]
    public class ShortUrlsServiceTests
    {
        [TestMethod()]
        public void CreateShortUrl_HappyPathTest_ReturnsSiteUrlObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);
            
            var mockSiteRepository = new Mock<ISiteUrlRepository>();

            
            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            response.OriginalUrl.Should().Be(originalURL);
            response.ShortUrl.Should().HaveLength(6);
            mockSiteRepository.Verify(repo => repo.FindOriginalUrl(It.IsAny<string>()), Times.Once);
            mockSiteRepository.Verify(repo => repo.FindShortUrl(It.IsAny<string>()), Times.Once);
            mockSiteRepository.Verify(repo => repo.InsertNewSiteUrl(It.IsAny<SiteUrl>()), Times.Once);

        }

        [TestMethod()]
        public void CreateShortUrl_OriginalUrlDoesNotStartWithHttpOrHttps_ThrowsInvalidUrlFormatException()
        {
            //Arrange
           
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockSiteRepository = new Mock<ISiteUrlRepository>();


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var ex = Assert.ThrowsException<InvalidUrlFormatException>(() => shortUrlsService.CreateShortUrl(originalURL));

            //Assert
            ex.Message.Should().Be("URL must begin with http:// or https://");
        }

        [TestMethod()]
        public void CreateShortUrl_QueryParamsRemovedFromUrl_ReturnsSiteUrlObjectWithSanitisedOriginalAndShortUrl()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);
            
            var mockSiteRepository = new Mock<ISiteUrlRepository>();


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut?IAm=ANastySurprise";
            string sanitisedOriginalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            response.OriginalUrl.Should().Be(sanitisedOriginalURL);
            response.ShortUrl.Should().HaveLength(6);

        }

        [TestMethod()]
        public void CreateShortUrl_NewLineAndLineFeedRemovedFromUrl_ReturnsSiteUrlObjectWithSanitisedOriginalAndShortUrl()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);
            
            var mockSiteRepository = new Mock<ISiteUrlRepository>();


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeSh\rortened/B\necauseIDontWantToTypeAllThisOut?IAm=ANastySurprise";
            string sanitisedOriginalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            response.OriginalUrl.Should().Be(sanitisedOriginalURL);
            response.ShortUrl.Should().HaveLength(6);

        }

        [TestMethod()]
        public void CreateShortUrl_OriginalUrlAlreadyExists_ThrowsUrlAlreadyPresentException()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);
            
            var mockSiteRepository = new Mock<ISiteUrlRepository>();
            mockSiteRepository.Setup(repo => repo.FindOriginalUrl(It.IsAny<String>()))
                .Returns(new SiteUrl() { Id="123456789", OriginalUrl= originalURL, ShortUrl= "d7mE33" });
       


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);


            //Act
            var ex = Assert.ThrowsException<UrlAlreadyPresentException>(() => shortUrlsService.CreateShortUrl(originalURL));

            //Assert
            ex.SiteUrl.Id.Should().Be("123456789");
            ex.SiteUrl.OriginalUrl.Should().Be(originalURL);
            ex.SiteUrl.ShortUrl.Should().Be("d7mE33");

        }

        [TestMethod()]
        public void CreateShortUrl_RandomUrlLengthIsVariableControlledByTheShortUrlLengthValue_ReturnsSiteUrlObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var shortUrlLength1 = "3";
            var shortUrlLength2 = "8";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value)
                .Returns(allowableCharacterSet)
                .Returns(shortUrlLength1)
                .Returns(allowableCharacterSet)
                .Returns(shortUrlLength2);
            var mockSiteRepository = new Mock<ISiteUrlRepository>();


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response1 = shortUrlsService.CreateShortUrl(originalURL);
            var response2 = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            response1.ShortUrl.Should().HaveLength(3);
            response2.ShortUrl.Should().HaveLength(8);

        }

        [TestMethod()]
        public void CreateShortUrl_RandomUrlShouldOnlyContainCharactersFromTheAllowableCharacterSet_ReturnsSiteUrlObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var shortUrlLength = "6";
            var allowableCharacterSet1 = "0123456789";
            var allowableCharacterSet2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value)
                .Returns(allowableCharacterSet1)
                .Returns(shortUrlLength)
                .Returns(allowableCharacterSet2)
                .Returns(shortUrlLength);
            var mockSiteRepository = new Mock<ISiteUrlRepository>();


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response1 = shortUrlsService.CreateShortUrl(originalURL);
            var response2 = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            response1.ShortUrl.Should().NotContainAny(allowableCharacterSet2);
            response2.ShortUrl.Should().NotContainAny(allowableCharacterSet1);

        }

        [TestMethod()]
        public void CreateShortUrl_WilRegenerateRandomUrlIfDuplicate_ReturnsSiteUrlObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);

            var mockSiteRepository = new Mock<ISiteUrlRepository>();

            mockSiteRepository.SetupSequence(repo => repo.FindShortUrl(It.IsAny<string>()))
                .Returns(new SiteUrl() { Id = "123456789", OriginalUrl = originalURL, ShortUrl = "d7mE33" }).Returns(value: null);


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            

            //Act
            var response = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            mockSiteRepository.Verify(repo => repo.FindShortUrl(It.IsAny<string>()), Times.Exactly(2));

        }

        [TestMethod()]
        public void CreateShortUrl_DatabaseOperationFailsThenSucessfullyRetries_ReturnsSiteUrlObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);

            var mockSiteRepository = new Mock<ISiteUrlRepository>();
            mockSiteRepository.SetupSequence(repo => repo.FindOriginalUrl(It.IsAny<string>()))
                .Throws(() => new MongoException("I wasn't paying attention.  Can you try again"));


            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            //Act
            var response = shortUrlsService.CreateShortUrl(originalURL);

            //Assert
            mockSiteRepository.Verify(repo => repo.FindOriginalUrl(It.IsAny<string>()), Times.Exactly(2));

        }

        [TestMethod()]
        public void CreateShortUrl_DatabaseOperationAttemptsExceeded_ThrowsMongoException()
        {
            //Arrange
            var urlLength = "6";
            var allowableCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";

            var mockIConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockIConfiguration.Setup(conf => conf.GetSection(It.IsAny<string>()))
                .Returns(mockConfigurationSection.Object);

            mockConfigurationSection.SetupSequence(confSec => confSec.Value).Returns(allowableCharacterSet).Returns(urlLength);

            var mockSiteRepository = new Mock<ISiteUrlRepository>();
            mockSiteRepository.SetupSequence(repo => repo.FindOriginalUrl(It.IsAny<String>()))
                .Throws(() => new MongoException("Nobodys home"))
                .Throws(() => new MongoException("Still not here"))
                .Throws(() => new MongoException("I'm back... Only joking"));

            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);


            //Act
            var ex = Assert.ThrowsException<MongoException>(() => shortUrlsService.CreateShortUrl(originalURL));

            //Assert
            ex.Message.Should().Be("I'm back... Only joking");
            mockSiteRepository.Verify(repo => repo.FindOriginalUrl(It.IsAny<string>()), Times.Exactly(3));
        }

        [TestMethod()]
        public void RetrieveOriginalUrl_HappyPathTest_ReturnsOriginalUrl()
        {
            //Arrange
            var shortUrl = "G7rzi0";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockSiteRepository = new Mock<ISiteUrlRepository>();
            mockSiteRepository.Setup(repo => repo.FindShortUrl(It.IsAny<string>()))
                .Returns(new SiteUrl() { Id = "123", OriginalUrl = "https://MyOriginalUrl.com", ShortUrl = shortUrl });

            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);

            //Act
            var response = shortUrlsService.RetrieveOriginalUrl(shortUrl);

            //Assert
            response.Should().Be("https://MyOriginalUrl.com");
            mockSiteRepository.Verify(repo => repo.FindShortUrl(It.IsAny<string>()), Times.Once);

        }

        [TestMethod()]
        public void RetrieveOriginalUrl_ShortUrlNotFound_ThrowsUrlNotFoundException()
        {
            //Arrange
            var shortUrl = "G7rzi0";
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockSiteRepository = new Mock<ISiteUrlRepository>();
            mockSiteRepository.Setup(repo => repo.FindShortUrl(It.IsAny<string>()))
                .Returns(value: null);

            var shortUrlsService = new ShortUrlsService(mockSiteRepository.Object, mockIConfiguration.Object);

            //Act & Assert
            Assert.ThrowsException<UrlNotFoundException>(() => shortUrlsService.RetrieveOriginalUrl(shortUrl));
        }

    }
}
