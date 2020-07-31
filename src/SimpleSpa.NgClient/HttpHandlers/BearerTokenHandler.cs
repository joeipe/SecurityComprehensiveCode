using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSpa.NgClient.HttpHandlers
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        public BearerTokenHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var accessToken = await GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                // set as Bearer token
                request.SetBearerToken(accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var expires_at = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");

            var expiresAtAsDateTimeOffset = DateTimeOffset.Parse(expires_at, CultureInfo.InvariantCulture);

            if (expiresAtAsDateTimeOffset.AddSeconds(-60).ToUniversalTime() > DateTime.UtcNow)
            {
                return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var disco = await idpClient.GetDiscoveryDocumentAsync();
            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var tokenResult = await idpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "simplespaclient",
                ClientSecret = "secret",
                RefreshToken = refreshToken
            });

            //store the tokens
            var updatedTokens = new List<AuthenticationToken>();
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = tokenResult.IdentityToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = tokenResult.AccessToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = tokenResult.RefreshToken
            });

            updatedTokens.Add(new AuthenticationToken
            {
                Name = "expires_at",
                Value = (DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn)).ToString("o", CultureInfo.InvariantCulture)
            });

            // get authenticate result, containing the current principal &
            // properties
            var currentAuthenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // store the updated tokens
            currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

            // sign in
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentAuthenticateResult.Principal, currentAuthenticateResult.Properties);

            // return the new access token
            return tokenResult.AccessToken;
        }
    }
}
