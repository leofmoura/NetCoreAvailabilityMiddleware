using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling;

namespace CoreAvailabilityMiddleware
{
    public class CheckAvailabilityMiddleware
    {

        private readonly RequestDelegate _next;
        public CheckAvailabilityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private async Task<bool> isAPIAvailable(IConfiguration config)
        {
            using (MiniProfiler.Current.Step("API Availability Check"))
            {
                var apiUrl = config.GetValue<string>("ApiAvailability");
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var availabilityResult = await response.Content.ReadAsAsync<dynamic>();
                        return availabilityResult.available;
                    }
                    return false;
                }
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {
            IConfiguration config = (IConfiguration)httpContext.RequestServices.
                GetService(typeof(IConfiguration));

            if (!await isAPIAvailable(config))
            {
                httpContext.Response.StatusCode = 403;
                await httpContext.Response.WriteAsync(
                    $"<h1>API Not available, please try again later.</h1>");
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    public static class CheckAvailabilityMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckAvailability(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckAvailabilityMiddleware>();
        }
    }
}