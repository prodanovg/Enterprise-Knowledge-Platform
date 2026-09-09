using Domain.Dto;
using Domain.Models;
using Domain.Dto.SemanticBlocks;
using Moq;
using Service.Interface;
using Web.Mapper;
using Web.Request.SemanticBlocks;
using Xunit;
using SemanticBlockEntity = Domain.Models.SemanticBlock;

namespace Tests.Mapper;

public class SemanticBlockMapperTests
{
    [Fact]
    public async Task CreateAsync_ShouldMapRequestAndResponse()
    {
        var serviceMock = new Mock<ISemanticBlockService>();
        var mapper = new SemanticBlockMapper(serviceMock.Object);
        var request = new CreateSemanticBlockRequest
        {
            DocumentId = Guid.NewGuid(),
            Text = "Text",
            BlockIndex = 2,
            Page = 3
        };
        var block = new SemanticBlockEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            Text = request.Text,
            BlockIndex = request.BlockIndex,
            Page = request.Page
        };
        serviceMock.Setup(x => x.CreateAsync(It.Is<CreateSemanticBlockDto>(
                dto => dto.DocumentId == request.DocumentId &&
                       dto.Text == request.Text &&
                       dto.BlockIndex == request.BlockIndex &&
                       dto.Page == request.Page), "user-id"))
            .ReturnsAsync(block);

        var response = await mapper.CreateAsync(request, "user-id");

        Assert.Equal(block.Id, response.Id);
        Assert.Equal(block.Text, response.Text);
        serviceMock.Verify(x => x.CreateAsync(It.IsAny<CreateSemanticBlockDto>(),
            "user-id"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapEntityToResponse()
    {
        var serviceMock = new Mock<ISemanticBlockService>();
        var mapper = new SemanticBlockMapper(serviceMock.Object);
        var block = new SemanticBlockEntity { Id = Guid.NewGuid(), Text = "Text" };
        serviceMock.Setup(x => x.GetByIdAsync(block.Id, "user-id")).ReturnsAsync(block);

        var response = await mapper.GetByIdAsync(block.Id, "user-id");

        Assert.Equal(block.Id, response!.Id);
        Assert.Equal(block.Text, response.Text);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldPreservePagination()
    {
        var serviceMock = new Mock<ISemanticBlockService>();
        var mapper = new SemanticBlockMapper(serviceMock.Object);
        var page = new PaginatedResult<SemanticBlockEntity>
        {
            Items = new List<SemanticBlockEntity> { new() { Id = Guid.NewGuid() } },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10,
            TotalPages = 1
        };
        serviceMock.Setup(x => x.GetAllPagedAsync(1, 10, "user-id")).ReturnsAsync(page);

        var response = await mapper.GetAllPagedAsync(1, 10, "user-id");

        Assert.Equal(page.TotalCount, response.TotalCount);
        Assert.Equal(page.Items[0].Id, response.Items[0].Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldMapRequest()
    {
        var serviceMock = new Mock<ISemanticBlockService>();
        var mapper = new SemanticBlockMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        var request = new UpdateSemanticBlockRequest
        {
            Text = "Updated",
            BlockIndex = 4,
            Page = 5
        };
        serviceMock.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateSemanticBlockDto>(),
                "user-id"))
            .ReturnsAsync(new SemanticBlockEntity { Id = id });

        await mapper.UpdateAsync(id, request, "user-id");

        serviceMock.Verify(x => x.UpdateAsync(id, It.Is<UpdateSemanticBlockDto>(
            dto => dto.Text == request.Text &&
                   dto.BlockIndex == request.BlockIndex &&
                   dto.Page == request.Page), "user-id"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldForwardUser()
    {
        var serviceMock = new Mock<ISemanticBlockService>();
        var mapper = new SemanticBlockMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);

        Assert.True(await mapper.DeleteAsync(id, "user-id"));
        serviceMock.Verify(x => x.DeleteAsync(id, "user-id"), Times.Once);
    }
}





