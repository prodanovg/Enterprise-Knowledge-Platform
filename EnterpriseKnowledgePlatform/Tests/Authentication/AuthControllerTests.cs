using Domain.Dto.Authentication;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.Authentication;
using Xunit;

namespace Tests.Authentication;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IAuthService>();
        _controller = new AuthController(new AuthMapper(_serviceMock.Object));
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequest { Username = "testuser" };
        _serviceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(new AuthResponseDto { Token = "token" });

        var result = await _controller.Register(request);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        _serviceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new InvalidOperationException("Already exists."));

        Assert.IsType<BadRequestObjectResult>(
            (await _controller.Register(new RegisterRequest())).Result);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginSucceeds()
    {
        var request = new LoginRequest { Username = "testuser" };
        _serviceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(new AuthResponseDto { Token = "token" });

        Assert.IsType<OkObjectResult>(
            (await _controller.Login(request)).Result);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        _serviceMock.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        Assert.IsType<UnauthorizedObjectResult>(
            (await _controller.Login(new LoginRequest())).Result);
    }
}

