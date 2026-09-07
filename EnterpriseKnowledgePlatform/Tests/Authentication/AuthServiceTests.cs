using Domain.Dto.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.Implementation;

namespace Tests.Authentication;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();

        _userManagerMock = new Mock<UserManager<User>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        _configurationMock = new Mock<IConfiguration>();

        SetupJwtConfiguration();
    }

    private void SetupJwtConfiguration()
    {
        var jwtSectionMock = new Mock<IConfigurationSection>();

        jwtSectionMock
            .Setup(x => x["Key"])
            .Returns(
                "ThisIsASuperSecretJwtKeyThatIsLongEnoughForTesting12345");

        jwtSectionMock
            .Setup(x => x["Issuer"])
            .Returns("EnterpriseKnowledgePlatform");

        jwtSectionMock
            .Setup(x => x["Audience"])
            .Returns("EnterpriseKnowledgePlatformClient");

        jwtSectionMock
            .Setup(x => x["ExpirationMinutes"])
            .Returns("60");

        _configurationMock
            .Setup(x => x.GetSection("Jwt"))
            .Returns(jwtSectionMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken()
    {
        // Arrange

        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<User>(),
                dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(
                It.IsAny<User>(),
                "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>
            {
                "User"
            });

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var result = await authService.RegisterAsync(dto);

        // Assert

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal("User", result.Role);

        _userManagerMock.Verify(
            x => x.CreateAsync(
                It.Is<User>(u =>
                    u.UserName == dto.Username &&
                    u.Email == dto.Email &&
                    u.Name == dto.Name),
                dto.Password),
            Times.Once);

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<User>(),
                "User"),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange

        var dto = new RegisterDto
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Password123!",
            Name = "Existing User"
        };

        var existingUser = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            Name = dto.Name
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => authService.RegisterAsync(dto));

        // Assert

        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        _userManagerMock.Verify(
            x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenIdentityCreationFails()
    {
        // Arrange

        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weak",
            Name = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var errors = new[]
        {
            new IdentityError
            {
                Description = "Password is too weak."
            }
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<User>(),
                dto.Password))
            .ReturnsAsync(
                IdentityResult.Failed(errors));

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => authService.RegisterAsync(dto));

        // Assert

        Assert.Contains(
            "Password is too weak.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange

        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            Name = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(dto.Username))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(
                user,
                dto.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                "User"
            });

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var result = await authService.LoginAsync(dto);

        // Assert

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);

        Assert.Equal(
            "testuser",
            result.Username);

        Assert.Equal(
            "test@example.com",
            result.Email);

        Assert.Equal(
            "User",
            result.Role);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange

        var dto = new LoginDto
        {
            Username = "unknownuser",
            Password = "Password123!"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(dto.Username))
            .ReturnsAsync((User?)null);

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => authService.LoginAsync(dto));

        // Assert

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);

        _userManagerMock.Verify(
            x => x.CheckPasswordAsync(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange

        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "WrongPassword123!"
        };

        var user = new User
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(dto.Username))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(
                user,
                dto.Password))
            .ReturnsAsync(false);

        var authService = new AuthService(
            _userManagerMock.Object,
            _configurationMock.Object);

        // Act

        var exception =
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => authService.LoginAsync(dto));

        // Assert

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);
    }
}