using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace CollabSphere.Application.Common
{
    public class GithubInstallationHelper
    {
        public static string CreateJwt(string appId, string pemPrivateKey)
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(pemPrivateKey);
            var rsaParams = rsa.ExportParameters(true);
            rsa.Dispose();

            var key = RSA.Create();
            key.ImportParameters(rsaParams);

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(key),
                SecurityAlgorithms.RsaSha256
            );

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var payload = new JwtPayload
            {
                { "iat", now - 60 }, // Issued at (trừ 60s để tránh lệch giờ server)
                { "exp", now + 9 * 60 },
                { "iss", appId }
            };

            return new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(new JwtHeader(signingCredentials), payload)
            );
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

            privateKey = privateKey.Replace("\\n", "\n");

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
            req.Headers.UserAgent.ParseAdd("collabsphere-ai-reviewer"); // Required by GitHub

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        private static async Task<string> GetAccessTokenByInstallationId(long installationId, string jwt)
        {
            // Get the access token for installtionId
            var accessTokenReq = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.github.com/app/installations/{installationId}/access_tokens"
            );

            accessTokenReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            accessTokenReq.Headers.UserAgent.ParseAdd("MyGitHubApp"); // Required by GitHub

            var accessTokenRes = await _http.SendAsync(accessTokenReq);
            accessTokenRes.EnsureSuccessStatusCode();
            var json = await accessTokenRes.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var token = obj.GetProperty("token").GetString() ?? "NOT FOUND";

            return token;
        }

        public class GithubRepositoryModel
        {
            [JsonPropertyName("id")]
            public long RepositoryId { get; set; }

            [JsonPropertyName("full_name")]
            public string FullName { get; set; } = null!;
        }

        public static async Task<List<GithubRepositoryModel>> GetRepositoresByInstallationId(long installationId, string jwt)
        {
            // Request for access token
            var accessToken = await GetAccessTokenByInstallationId(installationId, jwt);

            // Contruct get repositories request using access token
            var reposRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.github.com/installation/repositories"
            );

            reposRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            reposRequest.Headers.UserAgent.ParseAdd("MyGitHubApp"); // Required by GitHub

            // Send request & Get reponse
            var reposResult = await _http.SendAsync(reposRequest);
            reposResult.EnsureSuccessStatusCode();

            // Deserialize Json response
            var json = await reposResult.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var repositoriesJson = obj.GetProperty("repositories").ToString() ?? "";
            var repositories = JsonSerializer.Deserialize<List<GithubRepositoryModel>>(repositoriesJson);

            return repositories ?? new List<GithubRepositoryModel>();
        }
    }
}
