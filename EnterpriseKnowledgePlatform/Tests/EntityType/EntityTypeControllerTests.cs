using Domain.Dto;
using Domain.Dto.EntityTypes;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.EntityTypes;
using Xunit;
using EntityTypeEntity = Domain.Models.EntityType;

namespace Tests.EntityType;

public class EntityTypeControllerTests
{
    private readonly Mock<IEntityTypeService> _service = new();
    private readonly EntityTypeController _controller;

    public EntityTypeControllerTests() =>
        _controller = new EntityTypeController(new EntityTypeMapper(_service.Object));

    [Fact]
    public async Task CreateAndGetById_ShouldReturnExpectedResults()
    {
        var entityType = NewEntityType();
        _service.Setup(x => x.CreateAsync(It.IsAny<CreateEntityTypeDto>()))
            .ReturnsAsync(entityType);
        Assert.IsType<CreatedAtActionResult>(
            (await _controller.Create(new CreateEntityTypeRequest())).Result);

        _service.Setup(x => x.GetByIdAsync(entityType.Id)).ReturnsAsync(entityType);
        Assert.IsType<OkObjectResult>((await _controller.GetById(entityType.Id)).Result);
        _service.Setup(x => x.GetByIdAsync(entityType.Id))
            .ReturnsAsync((EntityTypeEntity?)null);
        Assert.IsType<NotFoundResult>((await _controller.GetById(entityType.Id)).Result);
    }

    [Fact]
    public async Task GetAllAndPaged_ShouldReturnResultsAndValidatePaging()
    {
        _service.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<EntityTypeEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAll()).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(0, 10)).Result);
        Assert.IsType<BadRequestResult>((await _controller.GetAllPaged(1, 0)).Result);
        _service.Setup(x => x.GetAllPagedAsync(1, 10))
            .ReturnsAsync(new PaginatedResult<EntityTypeEntity>());
        Assert.IsType<OkObjectResult>((await _controller.GetAllPaged()).Result);
    }

    [Fact]
    public async Task UpdateAndDelete_ShouldReturnExpectedResults()
    {
        var id = Guid.NewGuid();
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateEntityTypeDto>()))
            .ReturnsAsync(new EntityTypeEntity { Id = id });
        Assert.IsType<OkObjectResult>(
            (await _controller.Update(id, new UpdateEntityTypeRequest())).Result);
        _service.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateEntityTypeDto>()))
            .ThrowsAsync(new KeyNotFoundException());
        Assert.IsType<NotFoundResult>(
            (await _controller.Update(id, new UpdateEntityTypeRequest())).Result);
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);
        Assert.IsType<NoContentResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);
        Assert.IsType<NotFoundResult>(await _controller.Delete(id));
        _service.Setup(x => x.DeleteAsync(id))
            .ThrowsAsync(new InvalidOperationException());
        Assert.IsType<BadRequestObjectResult>(await _controller.Delete(id));
    }

    private static EntityTypeEntity NewEntityType() =>
        new() { Id = Guid.NewGuid(), Name = "Person" };
}
