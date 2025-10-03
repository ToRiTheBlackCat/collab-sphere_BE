using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Token;
using CollabSphere.Application.Features.Auth.Commands;
using CollabSphere.Application.Features.Token.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollabSphere.Test.Auth
{
    public class RefreshTokenTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<RefreshTokenHandler>> _mockLogger;
        private readonly JWTAuthentication _jwtAuth;
        private readonly Mock<IDatabase> _mockRedis;
        private readonly RefreshTokenHandler _handler;

        public RefreshTokenTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<RefreshTokenHandler>>();
            _mockRedis = new Mock<IDatabase>();

            _jwtAuth = new JWTAuthentication(_mockConfig.Object, _mockRedis.Object);
            _handler = new RefreshTokenHandler(_mockUnitOfWork.Object, _mockLogger.Object, _jwtAuth);

            //Set up configure jwt
            _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("a19f7c3e4b82d6f59e1c0b748aa2de90f3c1a45b6e9d8245c7d9f23ea47b1d6c\r\n");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test-audience");

            //Set up configure UnitOfWork and Repo
            _mockUnitOfWork.Setup(c => c.UserRepo).Returns(_mockUserRepo.Object);

            // Fake Redis cache storage (in-memory dictionary)
            var redisStorage = new Dictionary<string, string>();

            // Setup StringSetAsync
            _mockRedis.Setup(r => r.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(),
                                                  It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                     .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>((key, value, expiry, keepTtl, when, flags) =>
                     {
                         redisStorage[key] = value;
                     })
                     .ReturnsAsync(true);

            // Setup StringGetAsync
            _mockRedis.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                     .ReturnsAsync((RedisKey key, CommandFlags flags) =>
                     {
                         return redisStorage.ContainsKey(key) ? redisStorage[key] : RedisValue.Null;
                     });

            // Setup KeyDeleteAsync
            _mockRedis.Setup(r => r.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                     .Callback<RedisKey, CommandFlags>((key, flags) =>
                     {
                         if (redisStorage.ContainsKey(key)) redisStorage.Remove(key);
                     })
                     .ReturnsAsync(true);
        }

        [Fact]
        public async Task JWTRefreshToken_ShouldReturnTokens_ValidRefreshToken()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2 };

            var entry = new JWTAuthentication.RefreshTokenCacheEntry
            {
                RefreshToken = "valid-refresh",
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };

            var jsonEntry = JsonSerializer.Serialize(entry);

            _mockRedis
                .Setup(r => r.StringGetAsync("RefreshToken:1", It.IsAny<CommandFlags>()))
                .ReturnsAsync(jsonEntry);

            // Act
            var (newAccess, newRefresh) = await _jwtAuth.RefreshTokenAsync(user, "valid-refresh");

            // Assert
            Assert.False(string.IsNullOrEmpty(newAccess));
            Assert.False(string.IsNullOrEmpty(newRefresh));
            Assert.NotEqual("valid-refresh", newRefresh);
        }

        [Fact]
        public async Task JWTRefreshToken_ShouldReturnsEmptyTokensPair_RefreshTokenNotFoundInCache()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2 };
            _mockRedis
                .Setup(r => r.StringGetAsync("RefreshToken:1", It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var (newAccess, newRefresh) = await _jwtAuth.RefreshTokenAsync(user, "any");

            // Assert
            Assert.True(string.IsNullOrEmpty(newAccess));
            Assert.True(string.IsNullOrEmpty(newRefresh));
        }

        [Fact]
        public async Task JWTRefreshToken_ShouldReturnsEmptyTokensPair_RefreshTokenExpiredInCache()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2 };

            var entry = new JWTAuthentication.RefreshTokenCacheEntry
            {
                RefreshToken = "valid-refresh",
                Expiry = DateTime.UtcNow.AddMinutes(-1) 
            };

            var jsonEntry = JsonSerializer.Serialize(entry);

            _mockRedis
                .Setup(r => r.StringGetAsync("RefreshToken:1", It.IsAny<CommandFlags>()))
                .ReturnsAsync(jsonEntry);

            // Act
            var (newAccess, newRefresh) = await _jwtAuth.RefreshTokenAsync(user, "valid-refresh");

            // Assert
            Assert.True(string.IsNullOrEmpty(newAccess));
            Assert.True(string.IsNullOrEmpty(newRefresh));
        }

        [Fact]
        public async Task JWTRefreshToken_ShouldReturnsEmptyTokensPair_RefreshTokenNotMatchInCache()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2 };

            var entry = new JWTAuthentication.RefreshTokenCacheEntry
            {
                RefreshToken = "different-refresh",
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };

            var jsonEntry = JsonSerializer.Serialize(entry);

            _mockRedis
                .Setup(r => r.StringGetAsync("RefreshToken:1", It.IsAny<CommandFlags>()))
                .ReturnsAsync(jsonEntry);

            // Act
            var (newAccess, newRefresh) = await _jwtAuth.RefreshTokenAsync(user, "valid-refresh");

            // Assert
            Assert.True(string.IsNullOrEmpty(newAccess));
            Assert.True(string.IsNullOrEmpty(newRefresh));
        }

        [Fact]
        public async Task RefreshTokenHandler_ShouldReturnNotFound_WhenUserNotExists()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetOneByUserIdAsync(1))
                         .ReturnsAsync((User?)null);

            var command = new RefreshTokenCommand(new RefreshTokenRequestDto
            {
                UserId = 1,
                RefreshToken = "any"
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot find any user with that Id", result.Message);
        }

        [Fact]
        public async Task RefreshTokenHandler_ShouldReturnNotMatch_WhenRefreshTokenMismatch()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2, RefreshToken = "db-token" };
            _mockUserRepo.Setup(r => r.GetOneByUserIdAsync(1))
                         .ReturnsAsync(user);

            var command = new RefreshTokenCommand(new RefreshTokenRequestDto
            {
                UserId = 1,
                RefreshToken = "different"
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid Refresh token! Token not found", result.Message);
        }

        [Fact]
        public async Task RefreshTokenHandler_ShouldReturnNewTokens_WhenValid()
        {
            // Arrange
            var user = new User { UId = 1, RoleId = 2, RefreshToken = "valid-refresh" };
            _mockUserRepo.Setup(r => r.GetOneByUserIdAsync(1))
                         .ReturnsAsync(user);

            // Put valid refresh token into Redis
            var entry = new JWTAuthentication.RefreshTokenCacheEntry
            {
                RefreshToken = "valid-refresh",
                Expiry = DateTime.UtcNow.AddMinutes(10)
            };
            var jsonEntry = JsonSerializer.Serialize(entry);

            _mockRedis.Setup(r => r.StringGetAsync("RefreshToken:1", It.IsAny<CommandFlags>()))
                      .ReturnsAsync(jsonEntry);

            var command = new RefreshTokenCommand(new RefreshTokenRequestDto
            {
                UserId = 1,
                RefreshToken = "valid-refresh"
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Create new tokens successfully", result.Message);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));

            _mockUserRepo.Verify(r => r.UpdateUser(user), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRollbackTransaction_WhenExceptionThrown()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetOneByUserIdAsync(1))
                         .ThrowsAsync(new Exception("DB error"));

            var command = new RefreshTokenCommand(new RefreshTokenRequestDto
            {
                UserId = 1,
                RefreshToken = "any"
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Exception when create new tokens", result.Message);

            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
