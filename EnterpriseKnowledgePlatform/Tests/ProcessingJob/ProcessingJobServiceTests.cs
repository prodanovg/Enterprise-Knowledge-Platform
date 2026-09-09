using System.Linq.Expressions;
using Domain.Dto;
using Domain.Dto.ProcessingJobs;
using Domain.Enums;
using Domain.Models;
using Moq;
using Repository.Interface;
using Service.Implementation;
using Xunit;
using DocumentEntity = Domain.Models.Document;
using ProcessingJobEntity = Domain.Models.ProcessingJob;

namespace Tests.ProcessingJob;

public class ProcessingJobServiceTests
{
    private readonly Mock<IRepository<ProcessingJobEntity>> _processingJobRepositoryMock;
    private readonly Mock<IRepository<DocumentEntity>> _documentRepositoryMock;
    private readonly ProcessingJobService _processingJobService;

    public ProcessingJobServiceTests()
    {
        _processingJobRepositoryMock = new Mock<IRepository<ProcessingJobEntity>>();
        _documentRepositoryMock = new Mock<IRepository<DocumentEntity>>();
        _processingJobService = new ProcessingJobService(
            _processingJobRepositoryMock.Object,
            _documentRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePendingJob_WhenDocumentBelongsToUser()
    {
        var userId = "user-id";
        var document = new DocumentEntity { Id = Guid.NewGuid(), OwnerId = userId };
        var dto = new CreateProcessingJobDto { DocumentId = document.Id };
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(document);
        _processingJobRepositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<ProcessingJobEntity>()))
            .ReturnsAsync((ProcessingJobEntity job) => job);
        _processingJobRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _processingJobService.CreateAsync(dto, userId);

        Assert.Equal(document.Id, result.DocumentId);
        Assert.Equal(ProcessingJobStatus.Pending, result.Status);
        Assert.Equal(userId, result.CreatedBy);
        _processingJobRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDocumentDoesNotExist()
    {
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _processingJobService.CreateAsync(
                new CreateProcessingJobDto { DocumentId = Guid.NewGuid() },
                "user-id"));
        _processingJobRepositoryMock.Verify(
            x => x.InsertAsync(It.IsAny<ProcessingJobEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDocumentBelongsToAnotherUser()
    {
        var document = new DocumentEntity { Id = Guid.NewGuid(), OwnerId = "another-user" };
        _documentRepositoryMock
            .Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.Is<Expression<Func<DocumentEntity, bool>>>(predicate =>
                    !predicate.Compile()(document)), null, null, false))
            .ReturnsAsync((DocumentEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _processingJobService.CreateAsync(
                new CreateProcessingJobDto { DocumentId = document.Id },
                "user-id"));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenDocumentBelongsToUser()
    {
        var job = CreateJob("user-id");
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                null, null, true))
            .ReturnsAsync(job);

        var result = await _processingJobService.GetByIdAsync(job.Id, "user-id");

        Assert.Same(job, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentBelongsToAnotherUser()
    {
        var job = CreateJob("another-user");
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.Is<Expression<Func<ProcessingJobEntity, bool>>>(predicate =>
                    !predicate.Compile()(job)), null, null, true))
            .ReturnsAsync((ProcessingJobEntity?)null);

        var result = await _processingJobService.GetByIdAsync(job.Id, "user-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        var jobs = new List<ProcessingJobEntity> { CreateJob("user-id") };
        _processingJobRepositoryMock
            .Setup(x => x.GetAllAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(),
                null, null))
            .ReturnsAsync(jobs);

        var result = await _processingJobService.GetAllAsync("user-id");

        Assert.Same(jobs, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldReturnEntitiesAndUsePagination()
    {
        var resultPage = new PaginatedResult<ProcessingJobEntity>
        {
            Items = new List<ProcessingJobEntity> { CreateJob("user-id") },
            TotalCount = 1,
            PageNumber = 2,
            PageSize = 5,
            TotalPages = 1
        };
        _processingJobRepositoryMock
            .Setup(x => x.GetAllPagedAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                2, 5,
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(),
                null, true))
            .ReturnsAsync(resultPage);

        var result = await _processingJobService.GetAllPagedAsync(2, 5, "user-id");

        Assert.Same(resultPage, result);
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageNumberIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _processingJobService.GetAllPagedAsync(0, 10, "user-id"));
    }

    [Fact]
    public async Task GetAllPagedAsync_ShouldThrow_WhenPageSizeIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _processingJobService.GetAllPagedAsync(1, 0, "user-id"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedEntity()
    {
        var userId = "user-id";
        var job = CreateJob(userId);
        var dto = new UpdateProcessingJobDto
        {
            Status = ProcessingJobStatus.Completed,
            StartedAt = DateTime.UtcNow.AddMinutes(-1),
            FinishedAt = DateTime.UtcNow
        };
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(job);
        _processingJobRepositoryMock.Setup(x => x.UpdateAsync(job)).ReturnsAsync(job);
        _processingJobRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _processingJobService.UpdateAsync(job.Id, dto, userId);

        Assert.Same(job, result);
        Assert.Equal(dto.Status, result.Status);
        Assert.Equal(userId, result.ModifiedBy);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenJobIsNotOwned()
    {
        var job = CreateJob("another-user");
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.Is<Expression<Func<ProcessingJobEntity, bool>>>(predicate =>
                    !predicate.Compile()(job)), null, null, false))
            .ReturnsAsync((ProcessingJobEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _processingJobService.UpdateAsync(
                job.Id, new UpdateProcessingJobDto(), "user-id"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenJobIsOwned()
    {
        var job = CreateJob("user-id");
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                null, null, false))
            .ReturnsAsync(job);
        _processingJobRepositoryMock.Setup(x => x.DeleteAsync(job)).ReturnsAsync(job);
        _processingJobRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _processingJobService.DeleteAsync(job.Id, "user-id");

        Assert.True(result);
        _processingJobRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenJobIsNotOwned()
    {
        var job = CreateJob("another-user");
        _processingJobRepositoryMock
            .Setup(x => x.GetAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.Is<Expression<Func<ProcessingJobEntity, bool>>>(predicate =>
                    !predicate.Compile()(job)), null, null, false))
            .ReturnsAsync((ProcessingJobEntity?)null);

        var result = await _processingJobService.DeleteAsync(job.Id, "user-id");

        Assert.False(result);
        _processingJobRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<ProcessingJobEntity>()), Times.Never);
    }

    private static ProcessingJobEntity CreateJob(string ownerId)
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId
        };
        return new ProcessingJobEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Document = document,
            Status = ProcessingJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}

