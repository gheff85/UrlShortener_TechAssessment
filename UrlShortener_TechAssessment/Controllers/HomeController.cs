using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UrlShortener_TechAssessment.Exceptions;
using UrlShortener_TechAssessment.Models;
using UrlShortener_TechAssessment.Services;

namespace UrlShortener_TechAssessment.Controllers
{
    public class HomeController : Controller
    {
        private readonly IShortUrlsService _shortUrlsService;

        public HomeController(IShortUrlsService shortUrlsService)
        {
            _shortUrlsService = shortUrlsService;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost, Route("/")]
        public IActionResult GenerateShortUrl([FromBody] string url)
        {
            SiteUrl siteUrl;
            try
            {
                siteUrl = _shortUrlsService.CreateShortUrl(url);
                return Json(siteUrl);
            }
            catch (UrlAlreadyPresentException e)
            {
                return Json(e.SiteUrl);
            }
            catch(InvalidUrlFormatException e)
            {
                return Json(e.Message);
            }
            catch (Exception)
            {
                return Json("Unable to generate your shortURL. Please try again later");
            }
            
        }

        [HttpGet, Route("/{url}")]
        public IActionResult RedirectToOriginalUrl([FromRoute] string url)
        {
            try
            {
                return Redirect(_shortUrlsService.RetrieveOriginalUrl(url));

            }
            catch(Exception)
            {
                ViewData["Error"] = true;
                return View("Index");
            }
            
        }

    }
}