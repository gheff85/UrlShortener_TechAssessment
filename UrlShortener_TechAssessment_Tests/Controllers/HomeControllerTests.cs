using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener_TechAssessment.Controllers;
using UrlShortener_TechAssessment.Exceptions;
using UrlShortener_TechAssessment.Models;
using UrlShortener_TechAssessment.Services;

namespace UrlShortener_TechAssessment_Tests.Controllers
{
    [TestClass()]
    public class HomeControllerTests
    {
        private static readonly string originalURL = "http://LongURLWhichNeedsToBeShortened/BecauseIDontWantToTypeAllThisOut";
        private static readonly string shortUrl = "U4sp7wB";

        [TestMethod()]
        public void Index_ReturnsView()
        {
            //Arrange
            var mockShortUrlsService = new Mock<IShortUrlsService>();
            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.Index();

            //Assert
            ViewResult viewResult = (ViewResult)response;
            viewResult.ViewName.Should().Be("Index");
        }

        [TestMethod()]
        public void GenerateShortUrl_HappyPathTest_ReturnsJsonObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var expectedResponse = new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl };

            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.CreateShortUrl(It.IsAny<string>()))
                .Returns(new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl });

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.GenerateShortUrl(originalURL);

            //Assert
            JsonResult objectResult = (JsonResult)response;

            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            mockShortUrlsService.Verify(f => f.CreateShortUrl(It.Is<string>(s => s == originalURL)));
        }

        [TestMethod()]
        public void GenerateShortUrl_CreateShortUrlThrowsUrlAlreadyPresentException_ReturnsJsonObjectWithOriginalAndShortUrl()
        {
            //Arrange
            var expectedResponse = new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl };

            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.CreateShortUrl(It.IsAny<string>()))
                .Throws(() => new UrlAlreadyPresentException(new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl }));

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.GenerateShortUrl(originalURL);

            //Assert
            JsonResult objectResult = (JsonResult)response;

            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
            mockShortUrlsService.Verify(f => f.CreateShortUrl(It.Is<string>(s => s == originalURL)));
        }

        [TestMethod()]
        public void GenerateShortUrl_CreateShortUrlThrowsInvalidUrlFormatException_ReturnsErrorMessage()
        {
            //Arrange
            var expectedResponse = new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl };

            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.CreateShortUrl(It.IsAny<string>()))
                .Throws(() => new InvalidUrlFormatException("Mistake!"));

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.GenerateShortUrl(originalURL);

            //Assert
            JsonResult objectResult = (JsonResult)response;

            objectResult.Value.Should().Be("Mistake!");
            mockShortUrlsService.Verify(f => f.CreateShortUrl(It.Is<string>(s => s == originalURL)));
        }

        [TestMethod()]
        public void GenerateShortUrl_CreateShortUrlThrowsException_ReturnsErrorMessage()
        {
            //Arrange
            var expectedResponse = new SiteUrl() { Id = "456123", OriginalUrl = originalURL, ShortUrl = shortUrl };

            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.CreateShortUrl(It.IsAny<string>()))
                .Throws(() => new MongoException("I'm on holiday - back in 2 weeks"));

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.GenerateShortUrl(originalURL);

            //Assert
            JsonResult objectResult = (JsonResult)response;

            objectResult.Value.Should().Be("Unable to generate your shortURL. Please try again later");
            mockShortUrlsService.Verify(f => f.CreateShortUrl(It.Is<string>(s => s == originalURL)));
        }

        [TestMethod()]
        public void RedirectToOriginalUrl_HappyPathTest_SucessfullyRedirected()
        {
            //Arrange
            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.RetrieveOriginalUrl(It.IsAny<string>()))
                .Returns("https://www.google.com");

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.RedirectToOriginalUrl(originalURL);

            //Assert
            RedirectResult objectResult = (RedirectResult)response;

            objectResult.Url.Should().Be("https://www.google.com");
        }


        [TestMethod()]
        public void RedirectToOriginalUrl_RedirectToOriginalUrlThrowsUrlNotFoundException_DisplayIndexView()
        {
            //Arrange

            var mockShortUrlsService = new Mock<IShortUrlsService>();
            mockShortUrlsService.Setup(service => service.RetrieveOriginalUrl(It.IsAny<string>()))
                .Throws(() => new UrlNotFoundException());

            var homeController = new HomeController(mockShortUrlsService.Object);

            //Act
            var response = homeController.RedirectToOriginalUrl(originalURL);

            //Assert
            ViewResult objectResult = (ViewResult)response;

            objectResult.ViewName.Should().Be("Index");
            objectResult.ViewData["Error"].Should().Be(true);
        }
    }
}
