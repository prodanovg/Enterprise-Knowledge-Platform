using Domain.Dto.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Service.Implementation;
using Xunit;

namespace Tests.Authentication;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _configurationMock = new Mock<IConfiguration>();
        var section = new Mock<IConfigurationSection>();
        section.Setup(x => x["Key"])
            .Returns("ThisIsASuperSecretJwtKeyThatIsLongEnoughForTesting12345");
        section.Setup(x => x["Issuer"]).Returns("EnterpriseKnowledgePlatform");
        section.Setup(x => x["Audience"]).Returns("EnterpriseKnowledgePlatformClient");
        section.Setup(x => x["ExpirationMinutes"]).Returns("60");
        _configurationMock.Setup(x => x.GetSection("Jwt")).Returns(section.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponseDto()
    {
        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            Name = "Test User"
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" });

        var result = await new AuthService(
            _userManagerMock.Object, _configurationMock.Object).RegisterAsync(dto);

        Assert.IsType<AuthResponseDto>(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal("User", result.Role);
        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<User>(user => user.UserName == dto.Username &&
                user.Email == dto.Email && user.Name == dto.Name),
            dto.Password), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(
            It.IsAny<User>(), "User"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailExists()
    {
        var dto = new RegisterDto { Email = "existing@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new User());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AuthService(_userManagerMock.Object, _configurationMock.Object)
                .RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenIdentityCreationFails()
    {
        var dto = new RegisterDto { Email = "test@example.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Invalid password." }));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AuthService(_userManagerMock.Object, _configurationMock.Object)
                .RegisterAsync(dto));

        Assert.Contains("Invalid password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponseDto_WhenCredentialsAreValid()
    {
        var dto = new LoginDto { Username = "testuser", Password = "Password123!" };
        var user = new User
        {
            Id = "user-id",
            UserName = dto.Username,
            Email = "test@example.com"
        };
        _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var result = await new AuthService(
            _userManagerMock.Object, _configurationMock.Object).LoginAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
    {
        var dto = new LoginDto { Username = "testuser", Password = "wrong" };
        _userManagerMock.Setup(x => x.FindByNameAsync(dto.Username))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new AuthService(_userManagerMock.Object, _configurationMock.Object)
                .LoginAsync(dto));
    }
}



