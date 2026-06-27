namespace CinemaBooking.Web.Middlewares;

using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading.Tasks;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception has occurred while executing the request.");
            context.Response.Redirect("/Error/500");
        }
    }
}
