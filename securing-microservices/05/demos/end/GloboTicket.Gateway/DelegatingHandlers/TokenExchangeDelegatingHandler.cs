﻿using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GloboTicket.Gateway.DelegatingHandlers
{
    public class TokenExchangeDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenExchangeDelegatingHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // extract the current token
            var incomingToken = request.Headers.Authorization.Parameter;

            // exchange it
            var newToken = await ExchangeToken(incomingToken);

            // replace the incoming bearer token with our new one
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> ExchangeToken(string incomingToken)
        {
            var client = _httpClientFactory.CreateClient();

            var discoveryDocumentResponse = await client
                .GetDiscoveryDocumentAsync("https://localhost:5010/");
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token"},
                { "subject_token", incomingToken},
                { "scope", "openid profile eventcatalog.fullaccess" }
            };

            var tokenResponse = await client.RequestTokenAsync(new TokenRequest()
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = customParams,
                ClientId = "gatewaytodownstreamtokenexchangeclient",
                ClientSecret = "0cdea0bc-779e-4368-b46b-09956f70712c"
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            return tokenResponse.AccessToken;

        }
    }
}
