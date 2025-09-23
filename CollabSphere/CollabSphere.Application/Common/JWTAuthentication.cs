using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CollabSphere.Application.Common
{
    public class JWTAuthentication
    {
        private readonly IConfiguration _configure;
        private readonly IMemoryCache _cache;

        int _accessTokenLifeTime = 180;
        TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(7);


        public JWTAuthentication(IConfiguration configure, IMemoryCache cache)
        {
            _configure = configure;
            _cache = cache;
        }

        /// <summary>
        /// Function use for generating both tokens
        /// </summary>
        /// <param name="user">The found user after login</param>
        /// <returns> Object with two tokens </returns>
        public (string AccessToken, string RefreshToken) GenerateToken(User user)
        {
            //Clear existed cache that stored refresh token of that user
            RemoveRefreshTokenInCache(user.UId);

            //Create both token
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            //Store refresh token in cache
            StoreRefreshTokenInCache(user.UId, refreshToken);

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Function use for generating access token 
        /// </summary>
        /// <param name="user"> The found user after login, pass to this function to create JWT token</param>
        /// <returns> A access token string </returns>
        public string GenerateAccessToken(User user)
        {
            //Get key
            var jwtKey = _configure["Jwt:Secret"];
            //Encoding key
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""));
            //Create credential
            var credential = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            //Create claims
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UId.ToString().Trim()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString().Trim())
            };

            //Create token
            var token = new JwtSecurityToken(
                issuer: _configure["Jwt:Issuer"],
                audience: _configure["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenLifeTime),
                signingCredentials: credential
                );

            //Signing token
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return accessToken;
        }

        /// <summary>
        /// Function to generate refesh token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshToken()
        {
            var bytes = new byte[32];

            //Generate random number
            using var randomNum = RandomNumberGenerator.Create();
            randomNum.GetBytes(bytes);

            //Convert to string
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public object RefreshToken(User user, string refreshToken)
        {
            //Check valid of refresh token
            var cacheKey = $"RefreshToken:{user.UId}";

            if (_cache.TryGetValue(cacheKey, out RefreshTokenCacheEntry? entry))
            {
                // Check refresh token is valid and not expired
                if (entry?.RefreshToken == refreshToken && entry.Expiry > DateTime.UtcNow)
                {

                    //Create new tokens
                    var newAccessToken = GenerateAccessToken(user);
                    var newRefreshToken = GenerateRefreshToken();

                    //Store refresh token again in cache with old expired date
                    StoreRefreshTokenInCache(user.UId, newRefreshToken, entry.Expiry);

                    return new
                    {
                        accessToken = newAccessToken,
                        refreshToken = newRefreshToken
                    };
                }
            }

            return new();
        }

        /// <summary>
        /// Support function using for storing refresh token in cache
        /// </summary>
        public void StoreRefreshTokenInCache(int userId, string refreshToken, DateTime? expiry = null)
        {
            var entry = new RefreshTokenCacheEntry
            {
                RefreshToken = refreshToken,
                Expiry = expiry ?? DateTime.UtcNow.Add(_refreshTokenLifetime)
            };

            var cacheKey = $"RefreshToken:{userId}";

            _cache.Set(cacheKey, entry, entry.Expiry);
        }

        /// <summary>
        /// Support function using for remove 
        /// </summary>
        /// <param name="userId"></param>
        public void RemoveRefreshTokenInCache(int userId)
        {
            var cacheKey = $"RefreshToken:{userId}";
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Support entity using for storing refresh token + expire time of refresh token
        /// </summary>
        public class RefreshTokenCacheEntry
        {
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime Expiry { get; set; }
        }

    }
}
