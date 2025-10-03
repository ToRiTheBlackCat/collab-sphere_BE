using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Auth.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
using Role = CollabSphere.Domain.Entities.Role;

namespace CollabSphere.Test.Auth
{
    public class LoginTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<LoginHandler>> _mockLogger;
        private readonly JWTAuthentication _jwtAuth;
        private readonly Mock<IDatabase> _mockRedis;
        private readonly LoginHandler _handler;

        public LoginTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<LoginHandler>>();
            _mockRedis = new Mock<IDatabase>();

            _jwtAuth = new JWTAuthentication(_mockConfig.Object, _mockRedis.Object);
            _handler = new LoginHandler(_mockUnitOfWork.Object, _mockConfig.Object, _jwtAuth, _mockLogger.Object);

            //Setup config
            _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("a19f7c3e4b82d6f59e1c0b748aa2de90f3c1a45b6e9d8245c7d9f23ea47b1d6c\r\n");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test-audience");

            //Setup UnitOfWork to mock repo
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
        public async Task JWTAuthentication_ShouldReturnTokens_WhenFoundUser()
        {
            //Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role { RoleName = "ADMIN" }
            };

            //Act
            var (accessToken, refreshToken) = await _jwtAuth.GenerateToken(user);

            //Assert
            Assert.False(string.IsNullOrEmpty(accessToken));
            Assert.False(string.IsNullOrEmpty(refreshToken));
        }

        [Fact]
        public async Task JWTAuthentication_ShouldReturnBothToken_WhenValidRefreshToken()
        {
            // Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1
            };

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
            Assert.NotNull(newAccess);
            Assert.NotNull(newRefresh);
        }

        [Fact]
        public async Task JWTAuthentication_ShouldReturnEmpty_WhenRefreshTokenExpired()
        {
            // Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1
            };
            var refreshToken = _jwtAuth.GenerateRefreshToken();

            // Refresh Token expired in cache
            await _jwtAuth.StoreRefreshTokenInCache(user.UId, refreshToken, DateTime.UtcNow.AddMinutes(-1));

            // Act
            var result = await _jwtAuth.RefreshTokenAsync(user, refreshToken);

            // Assert
            Assert.Empty(result.GetType().GetProperties());
        }

        [Fact]
        public async Task LoginHandler_ShouldReturnDTO_WhenFoundUser()
        {
            //Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role { RoleName = "ADMIN" }
            };

            _mockUserRepo.Setup(r => r.GetOneByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(user);

            var command = new LoginCommand(new LoginRequestDTO
            {
                Email = "test@gmail.com",
                Password = "123"
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsAuthenticated);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task LoginHandler_ShouldRenturnIsAuthenticated_FALSE_WhenUserNotFound()
        {
            //Arrange
            _mockUserRepo.Setup(r => r.GetOneByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync((User?)null);

            var command = new LoginCommand(new LoginRequestDTO
            {
                Email = "notfounduser@gmail.com",
                Password = "123"
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsAuthenticated);
        }

        [Fact]
        public async Task LoginHandler_ShouldReturnIsAuthenticated_WhenFoundUser()
        {
            //Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role
                {
                    RoleName = "ADMIN"
                }
            };

            _mockUserRepo.Setup(r => r.GetOneByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(user);

            var command = new LoginCommand(new LoginRequestDTO
            {
                Email = "notfounduser@gmail.com",
                Password = "123"
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsAuthenticated);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task LoginHandler_ShouldSaveRefreshTokenToDB_WhenLoginSuccess()
        {
            //Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role
                {
                    RoleName = "ADMIN"
                }
            };

            _mockUserRepo.Setup(r => r.GetOneByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(user);

            var command = new LoginCommand(new LoginRequestDTO
            {
                Email = "validuser@gmail.com",
                Password = "123"
            });

            //Act
            await _handler.Handle(command, CancellationToken.None);

            //Assert
            _mockUserRepo.Verify(r => r.UpdateUser(It.Is<User>(u => u.RefreshToken != null)), Times.Once);
        }

        [Fact]
        public async Task LoginHandler_ShouldRollbacksTransaction_Exception_WhenUpdateRefreshToken()
        {
            //Arrange
            var foundUser = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role
                {
                    RoleName = "ADMIN"
                }
            };
            foundUser.RefreshToken = "123123asdfadfasdfcxvzcxv";
            foundUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _mockUserRepo.Setup(r => r.GetOneByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(foundUser);

            _mockUserRepo.Setup(r => r.UpdateUser(It.IsAny<User>()))
                         .Throws(new Exception());

            var command = new LoginCommand(new LoginRequestDTO
            {
                Email = "foundUser@gmail.com",
                Password = "123"
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsAuthenticated);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
