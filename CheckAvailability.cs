using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CoreAvailabilityMiddleware {
    public class CheckAvailabilityMiddleware {

        private readonly RequestDelegate _next;
        public CheckAvailabilityMiddleware (RequestDelegate next) {
            _next = next;
        }

        private async Task<bool> isAPIAvailable() {
            var apiUrl = "http://demo2430837.mockable.io/Testing";
            using (var httpClient = new HttpClient()) {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var availabilityResult = await response.Content.ReadAsAsync<dynamic>();
                    return availabilityResult.available;
                }
                return false;
            }

        }

        public async Task Invoke (HttpContext httpContext){
            IConfiguration config = (IConfiguration)httpContext.RequestServices.
                GetService(typeof(IConfiguration));
            
            if (!await isAPIAvailable())
            {
                httpContext.Response.StatusCode = 403;
                    await httpContext.Response.WriteAsync(
                        $"<h1>API Not available</h1>");
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