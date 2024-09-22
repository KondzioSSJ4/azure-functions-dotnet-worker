// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AspNetIntegration
{
    public class SimpleHttpTriggerHttpData
    {
        [Function("SimpleHttpTriggerHttpData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            await response.WriteStringAsync("Welcome to Azure Functions (HttpData)");

            return response;
        }

        [Function("WithOutput")]
        public OutputClass WithOutput([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            return new OutputClass();
        }

        public class OutputClass
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }
    }
}
