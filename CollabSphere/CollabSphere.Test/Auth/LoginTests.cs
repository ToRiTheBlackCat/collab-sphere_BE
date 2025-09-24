using CollabSphere.Application;
using CollabSphere.Application.Auth.Commands;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CollabSphere.Test.Auth
{
    public class LoginTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<LoginHandler>> _mockLogger;
        private readonly JWTAuthentication _jwtAuth;
        private readonly LoginHandler _handler;

        public LoginTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<LoginHandler>>();

            _jwtAuth = new JWTAuthentication(_mockConfig.Object, new MemoryCache(new MemoryCacheOptions()));
            _handler = new LoginHandler(_mockUnitOfWork.Object, _mockConfig.Object, _jwtAuth, _mockLogger.Object);

            //Setup config
            _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("a19f7c3e4b82d6f59e1c0b748aa2de90f3c1a45b6e9d8245c7d9f23ea47b1d6c\r\n");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test-audience");

            //Setup UnitOfWork to mock repo
            _mockUnitOfWork.Setup(c => c.UserRepo).Returns(_mockUserRepo.Object);

            //Set up memory cache
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            //Setup JWTAuthentication
            _jwtAuth = new JWTAuthentication(_mockConfig.Object, memoryCache);
        }

        [Fact]
        public void JWTAuthentication_ShouldReturnTokens_WhenFoundUser()
        {
            //Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1,
                Role = new Role { RoleName = "ADMIN" }
            };

            //Act
            var (accessToken, refreshToken) = _jwtAuth.GenerateToken(user);

            //Assert
            Assert.False(string.IsNullOrEmpty(accessToken));
            Assert.False(string.IsNullOrEmpty(refreshToken));
        }

        [Fact]
        public void JWTAuthentication_ShouldReturnBothToken_WhenValidRefreshToken()
        {
            // Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1
            };

            var (accessToken, refreshToken) = _jwtAuth.GenerateToken(user);

            // Act
            var (newAccessToken, newRefreshToken) = _jwtAuth.RefreshToken(user, refreshToken);

            // Assert
            Assert.NotNull(newAccessToken);
            Assert.NotNull(newRefreshToken);
        }

        [Fact]
        public void JWTAuthentication_ShouldReturnEmpty_WhenRefreshTokenExpired()
        {
            // Arrange
            var user = new User
            {
                UId = 1,
                RoleId = 1
            };
            var refreshToken = _jwtAuth.GenerateRefreshToken();

            // Refresh Token expired in cache
            _jwtAuth.StoreRefreshTokenInCache(user.UId, refreshToken, DateTime.UtcNow.AddMinutes(-1));

            // Act
            var result = _jwtAuth.RefreshToken(user, refreshToken);

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
