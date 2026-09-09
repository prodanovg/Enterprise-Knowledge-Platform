using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Domain.Dto.ProcessingJobs;
using Moq;
using Service.Interface;
using Web.Mapper;
using Web.Request.ProcessingJobs;
using Xunit;
using ProcessingJobEntity = Domain.Models.ProcessingJob;

namespace Tests.Mapper;

public class ProcessingJobMapperTests
{
    [Fact]
    public async Task CreateAsync_ShouldMapRequestAndResponse()
    {
        var serviceMock = new Mock<IProcessingJobService>();
        var mapper = new ProcessingJobMapper(serviceMock.Object);
        var request = new CreateProcessingJobRequest { DocumentId = Guid.NewGuid() };
        var job = new ProcessingJobEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            Status = ProcessingJobStatus.Pending
        };
        serviceMock.Setup(x => x.CreateAsync(It.Is<CreateProcessingJobDto>(
                dto => dto.DocumentId == request.DocumentId), "user-id"))
            .ReturnsAsync(job);

        var response = await mapper.CreateAsync(request, "user-id");

        Assert.Equal(job.Id, response.Id);
        Assert.Equal(job.DocumentId, response.DocumentId);
        serviceMock.Verify(x => x.CreateAsync(It.IsAny<CreateProcessingJobDto>(),
            "user-id"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapEntityToResponse()
    {
        var serviceMock = new Mock<IProcessingJobService>();
        var mapper = new ProcessingJobMapper(serviceMock.Object);
        var job = new ProcessingJobEntity { Id = Guid.NewGuid(), DocumentId = Guid.NewGuid() };
        serviceMock.Setup(x => x.GetByIdAsync(job.Id, "user-id")).ReturnsAsync(job);

        var response = await mapper.GetByIdAsync(job.Id, "user-id");

        Assert.Equal(job.Id, response!.Id);
        Assert.Equal(job.DocumentId, response.DocumentId);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldPreservePagination()
    {
        var serviceMock = new Mock<IProcessingJobService>();
        var mapper = new ProcessingJobMapper(serviceMock.Object);
        var page = new PaginatedResult<ProcessingJobEntity>
        {
            Items = new List<ProcessingJobEntity> { new() { Id = Guid.NewGuid() } },
            TotalCount = 3,
            PageNumber = 2,
            PageSize = 5,
            TotalPages = 1
        };
        serviceMock.Setup(x => x.GetAllPagedAsync(2, 5, "user-id")).ReturnsAsync(page);

        var response = await mapper.GetAllPagedAsync(2, 5, "user-id");

        Assert.Equal(page.TotalCount, response.TotalCount);
        Assert.Equal(page.PageNumber, response.PageNumber);
        Assert.Equal(page.Items[0].Id, response.Items[0].Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldMapRequest()
    {
        var serviceMock = new Mock<IProcessingJobService>();
        var mapper = new ProcessingJobMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        var request = new UpdateProcessingJobRequest
        {
            Status = ProcessingJobStatus.Completed,
            ErrorMessage = "none"
        };
        serviceMock.Setup(x => x.UpdateAsync(id, It.IsAny<UpdateProcessingJobDto>(),
                "user-id"))
            .ReturnsAsync(new ProcessingJobEntity { Id = id });

        await mapper.UpdateAsync(id, request, "user-id");

        serviceMock.Verify(x => x.UpdateAsync(id, It.Is<UpdateProcessingJobDto>(
            dto => dto.Status == request.Status &&
                   dto.ErrorMessage == request.ErrorMessage), "user-id"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldForwardUser()
    {
        var serviceMock = new Mock<IProcessingJobService>();
        var mapper = new ProcessingJobMapper(serviceMock.Object);
        var id = Guid.NewGuid();
        serviceMock.Setup(x => x.DeleteAsync(id, "user-id")).ReturnsAsync(true);

        Assert.True(await mapper.DeleteAsync(id, "user-id"));
        serviceMock.Verify(x => x.DeleteAsync(id, "user-id"), Times.Once);
    }
}





