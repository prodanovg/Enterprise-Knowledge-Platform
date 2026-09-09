using System.Security.Claims;
using Domain.Dto;
using Domain.Dto.ProcessingJobs;
using Domain.Enums;
using DocumentEntity = Domain.Models.Document;
using ProcessingJobEntity = Domain.Models.ProcessingJob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.ProcessingJobs;
using Xunit;

namespace Tests.ProcessingJob;

public class ProcessingJobControllerTests
{
    private readonly Mock<IProcessingJobService> _serviceMock;
    private readonly ProcessingJobController _controller;

    public ProcessingJobControllerTests()
    {
        _serviceMock = new Mock<IProcessingJobService>();
        _controller = new ProcessingJobController(new ProcessingJobMapper(_serviceMock.Object));
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        SetUser("user-id");
        var request = new CreateProcessingJobRequest { DocumentId = Guid.NewGuid() };
        _serviceMock.Setup(x => x.CreateAsync(It.IsAny<CreateProcessingJobDto>(), "user-id"))
            .ReturnsAsync(new ProcessingJobEntity { Id = Guid.NewGuid(), DocumentId = request.DocumentId });

        var result = await _controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorizedOrNotFound()
    {
        SetUnauthenticatedUser();
        Assert.IsType<UnauthorizedResult>(
            (await _controller.Create(new CreateProcessingJobRequest())).Result);

        SetUser("user-id");
        _serviceMock.Setup(x => x.CreateAsync(
                It.IsAny<CreateProcessingJobDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Create(new CreateProcessingJobRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync(new ProcessingJobEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);

        _serviceMock.Setup(x => x.GetByIdAsync(id, "user-id"))
            .ReturnsAsync((ProcessingJobEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllAsync("user-id"))
            .ReturnsAsync(new List<ProcessingJobEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
    }

    [Fact]
    public async Task GetAllPaged_ShouldValidateAndReturnOk()
    {
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestObjectResult>(
            (await _controller.GetAllPaged(1, 0)).Result);

        SetUser("user-id");
        _serviceMock.Setup(x => x.GetAllPagedAsync(1, 10, "user-id"))
            .ReturnsAsync(new PaginatedResult<ProcessingJobEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task Update_ShouldReturnOkOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        var request = new UpdateProcessingJobRequest();
        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateProcessingJobDto>(), "user-id"))
            .ReturnsAsync(new ProcessingJobEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.Update(id, request)).Result);

        _serviceMock.Setup(x => x.UpdateAsync(
                id, It.IsAny<UpdateProcessingJobDto>(), "user-id"))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>((await _controller.Update(id, request)).Result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContentOrNotFound()
    {
        SetUser("user-id");
        var id = Guid.NewGuid();
        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
    }

    private void SetUser(string userId)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                    "TestAuthentication"))
            }
        };
    }

    private void SetUnauthenticatedUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}

