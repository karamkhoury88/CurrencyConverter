using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.ServiceDefaults.Handlers
{
    internal class HttpMessageLoggingHandler(ILogger<HttpMessageLoggingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log the request
            logger.LogInformation("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            // Log the response
            logger.LogInformation("Received HTTP response: {StatusCode}", response.StatusCode);

            return response;
        }
    }
}
