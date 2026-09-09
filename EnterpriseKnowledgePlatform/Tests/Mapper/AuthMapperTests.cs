using Domain.Dto.Authentication;
using Moq;
using Service.Interface;
using Web.Mapper;
using Web.Request.Authentication;
using Xunit;

namespace Tests.Mapper;

public class AuthMapperTests
{
    [Fact]
    public async Task RegisterAsync_ShouldMapRequestAndAuthResponseDto()
    {
        var serviceMock = new Mock<IAuthService>();
        var mapper = new AuthMapper(serviceMock.Object);
        var request = new RegisterRequest
        {
            Name = "Name",
            Username = "username",
            Email = "email@example.com",
            Password = "Password123!"
        };
        var result = new AuthResponseDto
        {
            Token = "token",
            Username = request.Username,
            Email = request.Email,
            Role = "User"
        };
        serviceMock.Setup(x => x.RegisterAsync(It.Is<Domain.Dto.Authentication.RegisterDto>(
                dto => dto.Name == request.Name &&
                       dto.Username == request.Username &&
                       dto.Email == request.Email &&
                       dto.Password == request.Password)))
            .ReturnsAsync(result);

        var response = await mapper.RegisterAsync(request);

        Assert.Equal(result.Token, response.Token);
        Assert.Equal(result.Username, response.Username);
        serviceMock.Verify(x => x.RegisterAsync(It.IsAny<
            Domain.Dto.Authentication.RegisterDto>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldMapRequestAndAuthResponseDto()
    {
        var serviceMock = new Mock<IAuthService>();
        var mapper = new AuthMapper(serviceMock.Object);
        var request = new LoginRequest { Username = "username", Password = "password" };
        serviceMock.Setup(x => x.LoginAsync(It.IsAny<
                Domain.Dto.Authentication.LoginDto>()))
            .ReturnsAsync(new AuthResponseDto
            {
                Token = "token",
                Username = request.Username
            });

        var response = await mapper.LoginAsync(request);

        Assert.Equal("token", response.Token);
        Assert.Equal(request.Username, response.Username);
        serviceMock.Verify(x => x.LoginAsync(It.Is<
            Domain.Dto.Authentication.LoginDto>(dto =>
                dto.Username == request.Username &&
                dto.Password == request.Password)), Times.Once);
    }
}



