using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dotnet0Auth.ApiService.Controllers
{
    [Route("oauth")]
    public class OAuthController : Controller
    {
        private const string Code = "BABAABABABA"; // Simulated authorization code
        private const string RefreshTokenSample = "RefreshTokenSampleValueSomething77"; // Simulated refresh token
        
        [HttpGet("authorize")]
        public IActionResult Authorize(
            string response_type,
            string client_id,
            string redirect_uri,
            string scope,
            string state)
        {
            var query = new QueryBuilder
            {
                { "redirectUri", redirect_uri },
                { "state", state }
            };

            return View(model: query.ToString());
        }

        [HttpPost("authorize")]
        public IActionResult Authorize(string username, string redirectUri, string state)
        {
            var query = new QueryBuilder
            {
                { "code", Code },
                { "state", state }
            };

            return Redirect($"{redirectUri}{query.ToString()}");
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token(
            string grant_type,
            string code,
            string redirect_uri,
            string client_id,
            string refresh_token)
        {
            var accessToken = GenerateAccessToken(grant_type);

            var responseObject = new
            {
                access_token = accessToken,
                token_type = "Bearer",
                raw_claim = "oauthTutorial",
                refresh_token = RefreshTokenSample
            };

            return Ok(responseObject); // Return the response directly as JSON        }
        }

        [Authorize]
        [HttpGet("validate")]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                return Ok();
            }

            return BadRequest("Access token is missing or invalid.");
        }

        private string GenerateAccessToken(string grant_type)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")
            };

            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Constants.Issuer,
                Constants.Audiance,
                claims,
                notBefore: DateTime.UtcNow,
                expires: grant_type == "refresh_token" ? DateTime.UtcNow.AddMinutes(5) : DateTime.UtcNow.AddMinutes(1),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

