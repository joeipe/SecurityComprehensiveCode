using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SimpleSpa.NgClient.ViewModels;

namespace SimpleSpa.NgClient.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            var url = HttpContext.Session.GetString("LogoutUrl");

            return Ok(url);
        }

        [HttpGet]
        [Authorize(Policy = "JoeAdmin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetToken()
        {
            var tokenDetails = new TokenDetails();

            var claims = await GetClaimsAsync();

            tokenDetails.IdToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            tokenDetails.AccessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            //tokenDetails.RefreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            tokenDetails.FirstName = claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            tokenDetails.LastName = claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            tokenDetails.Address = claims.FirstOrDefault(c => c.Type == "address")?.Value;
            tokenDetails.Roles = string.Join(",", claims.Where(c => c.Type == "role").Select(x => x.Value));

            return Ok(tokenDetails);
        }

        #region Private start

        private async Task<List<Claim>> GetClaimsAsync()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var disco = await idpClient.GetDiscoveryDocumentAsync();

            if (disco.IsError)
            {
                throw new Exception(
                        "Problem accessing the discovery endpoint."
                        , disco.Exception);
            }

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var response = await idpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = accessToken
            });

            if (response.IsError)
            {
                throw new Exception(
                    "Problem accessing the UserInfo endpoint."
                    , response.Exception);
            }

            return response.Claims.ToList(); ;
        }

        #endregion end


    }
}
