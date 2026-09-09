using Domain.Dto;
using Domain.Dto.GraphRelationships;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.GraphRelationships;
using Xunit;
using RelationshipEntity = Domain.Models.GraphRelationship;

namespace Tests.GraphRelationship;

public class GraphRelationshipControllerTests
{
    private readonly Mock<IGraphRelationshipService> _service = new();
    private readonly GraphRelationshipController _controller;

    public GraphRelationshipControllerTests() =>
        _controller = new GraphRelationshipController(new GraphRelationshipMapper(_service.Object));

    [Fact]
    public async Task CreateAndGetById_ShouldReturnExpectedResults()
    {
        var relationship = NewRelationship();
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateGraphRelationshipDto>())).ReturnsAsync(relationship);
        Assert.IsType<CreatedAtActionResult>((await _controller.Create(new CreateGraphRelationshipRequest())).Result);
        _service.Setup(x => x.GetByIdAsync(relationship.Id)).ReturnsAsync(relationship);
        Assert.IsType<OkObjectResult>((await _controller.GetById(relationship.Id)).Result);
        _service.Setup(x => x.GetByIdAsync(relationship.Id)).ReturnsAsync((RelationshipEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(relationship.Id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldValidatePaging()
    {
        _service.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<RelationshipEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10)).ReturnsAsync(new PaginatedResult<RelationshipEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateGraphRelationshipDto>())).ReturnsAsync(new RelationshipEntity { Id = id });
        Assert.IsType<OkObjectResult>((await _controller.Update(id, new UpdateGraphRelationshipRequest())).Result);
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateGraphRelationshipDto>())).ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>((await _controller.Update(id, new UpdateGraphRelationshipRequest())).Result);
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
    }

    private static RelationshipEntity NewRelationship() => new() { Id = Guid.NewGuid() };
}
