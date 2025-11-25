using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace CollabSphere.Application.Common
{
    public class GithubInstallationHelper
    {
        public static string CreateJwt(string appId, string pemPrivateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pemPrivateKey);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var payload = new JwtPayload
            {
                { "iat", now },
                { "exp", now + 9 * 60 }, // 9 minutes
                { "iss", appId }
            };

            var token = new JwtSecurityToken(
                new JwtHeader(signingCredentials),
                payload
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static (string appId, string privateKey) GetInstallationConfig()
        {
            var appId = Environment.GetEnvironmentVariable("GITHUB_APP_ID");
            var privateKey = Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY");

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            {
                var valueString = $"app = '{appId}', privateKey = '{privateKey}'";
                throw new Exception($"Invalid config for github installation: {valueString}");
            }

            return (appId, privateKey);
        }

        private static readonly HttpClient _http = new();
        public static async Task<string> GetInstallations(string jwt)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.github.com/app/installations"
            );

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            req.Headers.UserAgent.ParseAdd("MyGitHubApp"); // Required by GitHub!

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }


        public static async Task<HttpResponseMessage> GetInstallationsResponse(string jwt)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.github.com/app/installations"
            );

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            req.Headers.UserAgent.ParseAdd("MyGitHubApp"); // Required by GitHub!

            var res = await _http.SendAsync(req);
            return res;
        }
    }
}
