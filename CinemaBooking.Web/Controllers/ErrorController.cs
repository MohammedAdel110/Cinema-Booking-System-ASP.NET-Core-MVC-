namespace CinemaBooking.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        switch (statusCode)
        {
            case 403:
                ViewBag.ErrorMessage = "Sorry, you don't have permission to access this page.";
                return View("403");
            case 404:
                ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found.";
                return View("404");
            case 500:
                ViewBag.ErrorMessage = "Sorry, an unexpected error occurred on the server.";
                return View("500");
            default:
                ViewBag.ErrorMessage = "Sorry, an error occurred while processing your request.";
                return View("Error");
        }
    }
}
