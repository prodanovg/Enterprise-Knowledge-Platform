using Domain.Dto.Authentication;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;

namespace Tests.Authentication;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();

        _controller = new AuthController(
            _authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        // Arrange

        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            Name = "Test User"
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "test-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            Username = dto.Username,
            Email = dto.Email,
            Role = "User"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(dto))
            .ReturnsAsync(expectedResponse);

        // Act

        var result = await _controller.Register(dto);

        // Assert

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        var response =
            Assert.IsType<AuthResponseDto>(
                okResult.Value);

        Assert.Equal(
            expectedResponse.Token,
            response.Token);

        Assert.Equal(
            "User",
            response.Role);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenServiceThrowsInvalidOperation()
    {
        // Arrange

        var dto = new RegisterDto
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Password123!",
            Name = "Existing User"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(dto))
            .ThrowsAsync(
                new InvalidOperationException(
                    "A user with this email already exists."));

        // Act

        var result = await _controller.Register(dto);

        // Assert

        Assert.IsType<BadRequestObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginSucceeds()
    {
        // Arrange

        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "jwt-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(dto))
            .ReturnsAsync(expectedResponse);

        // Act

        var result = await _controller.Login(dto);

        // Assert

        var okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        var response =
            Assert.IsType<AuthResponseDto>(
                okResult.Value);

        Assert.Equal(
            expectedResponse.Token,
            response.Token);

        Assert.Equal(
            expectedResponse.Username,
            response.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange

        var dto = new LoginDto
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(dto))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "Invalid username or password."));

        // Act

        var result = await _controller.Login(dto);

        // Assert

        Assert.IsType<UnauthorizedObjectResult>(
            result.Result);
    }
}