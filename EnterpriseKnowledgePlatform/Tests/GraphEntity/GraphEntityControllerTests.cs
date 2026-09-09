using Domain.Dto;
using Domain.Dto.GraphEntities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.GraphEntities;
using Xunit;
using GraphEntityEntity = Domain.Models.GraphEntity;

namespace Tests.GraphEntity;

public class GraphEntityControllerTests
{
    private readonly Mock<IGraphEntityService> _service = new();
    private readonly GraphEntityController _controller;

    public GraphEntityControllerTests() =>
        _controller = new GraphEntityController(new GraphEntityMapper(_service.Object));

    [Fact]
    public async Task CreateAndGetById_ShouldReturnExpectedResults()
    {
        var entity = NewEntity();
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateGraphEntityDto>())).ReturnsAsync(entity);
        Assert.IsType<CreatedAtActionResult>(
            (await _controller.Create(new CreateGraphEntityRequest())).Result);
        _service.Setup(x => x.GetByIdAsync(entity.Id)).ReturnsAsync(entity);
        Assert.IsType<OkObjectResult>((await _controller.GetById(entity.Id)).Result);
        _service.Setup(x => x.GetByIdAsync(entity.Id)).ReturnsAsync((GraphEntityEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(entity.Id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldReturnResultsAndValidatePaging()
    {
        _service.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<GraphEntityEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10))
            .ReturnsAsync(new PaginatedResult<GraphEntityEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateGraphEntityDto>()))
            .ReturnsAsync(new GraphEntityEntity { Id = id });
        Assert.IsType<OkObjectResult>(
            (await _controller.Update(id, new UpdateGraphEntityRequest())).Result);
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateGraphEntityDto>()))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Update(id, new UpdateGraphEntityRequest())).Result);
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id)).ThrowsAsync(new InvalidOperationException());
        Assert.IsType<BadRequestObjectResult>(await _controller.Delete(id));
    }

    private static GraphEntityEntity NewEntity() =>
        new() { Id = Guid.NewGuid(), Name = "Alice", EntityTypeId = Guid.NewGuid() };
}
