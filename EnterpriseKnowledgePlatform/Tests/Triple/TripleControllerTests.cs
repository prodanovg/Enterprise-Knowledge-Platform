using Domain.Dto;
using Domain.Dto.Triples;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.Triples;
using Xunit;
using TripleEntity = Domain.Models.Triple;

namespace Tests.Triple;

public class TripleControllerTests
{
    private readonly Mock<ITripleService> _service = new();
    private readonly TripleController _controller;

    public TripleControllerTests()
    {
        _controller = new TripleController(new TripleMapper(_service.Object));
    }

    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateTripleDto>()))
            .ReturnsAsync(new TripleEntity { Id = Guid.NewGuid() });

        Assert.IsType<CreatedAtActionResult>(
            (await _controller.Create(new CreateTripleRequest())).Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkOrNotFound()
    {
        var id = Guid.NewGuid();
        _service.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new TripleEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.GetById(id)).Result);

        _service.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((TripleEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldReturnExpectedResults()
    {
        _service.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TripleEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);

        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10))
            .ReturnsAsync(new PaginatedResult<TripleEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateTripleDto>()))
            .ReturnsAsync(new TripleEntity { Id = id });
        Assert.IsType<OkObjectResult>(
            (await _controller.Update(id, new UpdateTripleRequest())).Result);

        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateTripleDto>()))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Update(id, new UpdateTripleRequest())).Result);

        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
    }
}
