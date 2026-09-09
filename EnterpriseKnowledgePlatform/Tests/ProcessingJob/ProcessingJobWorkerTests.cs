using System.Linq.Expressions;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Repository.Interface;
using Web.Workers;
using Web.Clients;
using Xunit;
using ProcessingJobEntity = Domain.Models.ProcessingJob;
using DocumentEntity = Domain.Models.Document;

namespace Tests.ProcessingJob;

public class ProcessingJobWorkerTests
{
    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldMarkPendingJobsProcessing()
    {
        var jobs = new List<ProcessingJobEntity>
        {
            new() { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending },
            new() { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending }
        };
        var repository = new Mock<IRepository<ProcessingJobEntity>>();
        repository.Setup(x => x.GetAllAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(),
                null, 10))
            .ReturnsAsync(jobs);
        repository.Setup(x => x.UpdateAsync(It.IsAny<ProcessingJobEntity>()))
            .ReturnsAsync((ProcessingJobEntity job) => job);
        repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(2);
        var client = new Mock<IProcessingApiClient>();
        client.Setup(x => x.SendProcessingJobAsync(
            It.IsAny<ProcessingJobEntity>(), It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var worker = CreateWorker(repository.Object, client.Object, new Mock<IRepository<DocumentEntity>>().Object);

        var count = await worker.ProcessPendingJobsAsync();

        Assert.Equal(2, count);
        Assert.All(jobs, job =>
        {
            Assert.Equal(ProcessingJobStatus.Processing, job.Status);
            Assert.NotNull(job.StartedAt);
        });
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
        worker.Dispose();
    }

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldDoNothingWhenNoPendingJobs()
    {
        var repository = new Mock<IRepository<ProcessingJobEntity>>();
        repository.Setup(x => x.GetAllAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(),
                null, 10))
            .ReturnsAsync(new List<ProcessingJobEntity>());
        var worker = CreateWorker(repository.Object, new Mock<IProcessingApiClient>().Object,
            new Mock<IRepository<DocumentEntity>>().Object);

        Assert.Equal(0, await worker.ProcessPendingJobsAsync());
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
        worker.Dispose();
    }

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldMarkJobFailedWhenClientFails()
    {
        var job = new ProcessingJobEntity
        { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending };
        var repository = new Mock<IRepository<ProcessingJobEntity>>();
        repository.Setup(x => x.GetAllAsync<ProcessingJobEntity>(
                It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(),
                It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(),
                null, 10)).ReturnsAsync(new List<ProcessingJobEntity> { job });
        repository.Setup(x => x.UpdateAsync(It.IsAny<ProcessingJobEntity>()))
            .ReturnsAsync((ProcessingJobEntity value) => value);
        var client = new Mock<IProcessingApiClient>();
        client.Setup(x => x.SendProcessingJobAsync(
                It.IsAny<ProcessingJobEntity>(), It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("unavailable"));
        var documentRepository = new Mock<IRepository<DocumentEntity>>();
        documentRepository.Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(), null, null, false))
            .ReturnsAsync(new DocumentEntity { Id = job.DocumentId, FilePath = "document.pdf" });
        var worker = CreateWorker(repository.Object, client.Object, documentRepository.Object);

        await worker.ProcessPendingJobsAsync();

        Assert.Equal(ProcessingJobStatus.Failed, job.Status);
        Assert.NotNull(job.FinishedAt);
        Assert.Equal("Processing API communication failed.", job.ErrorMessage);
        repository.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        worker.Dispose();
    }

    private static ProcessingJobWorker CreateWorker(
        IRepository<ProcessingJobEntity> repository,
        IProcessingApiClient client,
        IRepository<DocumentEntity> documentRepository)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IRepository<ProcessingJobEntity>)))
            .Returns(repository);
        serviceProvider.Setup(x => x.GetService(typeof(IProcessingApiClient)))
            .Returns(client);
        serviceProvider.Setup(x => x.GetService(typeof(IRepository<DocumentEntity>)))
            .Returns(documentRepository);
        var scope = new Mock<IServiceScope>();
        scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        return new ProcessingJobWorker(scopeFactory.Object);
    }
}
