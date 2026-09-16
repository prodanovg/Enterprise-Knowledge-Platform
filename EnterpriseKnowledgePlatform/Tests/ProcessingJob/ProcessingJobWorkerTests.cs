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
using Web.Response;
using Service.Interface;

namespace Tests.ProcessingJob;

public class ProcessingJobWorkerTests
{
    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldMarkSuccessfulJobsCompleted()
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
            .ReturnsAsync(new ProcessingApiResponse { Success = true });
        var email = new Mock<IEmailService>();
        var documentRepository = new Mock<IRepository<DocumentEntity>>();
        documentRepository.Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(), null, null, false))
            .ReturnsAsync(new DocumentEntity { OwnerId = "user-1" });
        var users = new Mock<IRepository<Domain.Models.User>>();
        users.Setup(x => x.GetAsync<Domain.Models.User>(It.IsAny<Expression<Func<Domain.Models.User, Domain.Models.User>>>(), It.IsAny<Expression<Func<Domain.Models.User, bool>>>(), null, null, false))
            .ReturnsAsync(new Domain.Models.User { Id = "user-1", Email = "user@example.com" });
        var worker = CreateWorker(repository.Object, client.Object, documentRepository.Object, email.Object, users.Object);

        var count = await worker.ProcessPendingJobsAsync();

        Assert.Equal(2, count);
        Assert.All(jobs, job =>
        {
            Assert.Equal(ProcessingJobStatus.Completed, job.Status);
            Assert.NotNull(job.StartedAt);
            Assert.NotNull(job.FinishedAt);
            Assert.Null(job.ErrorMessage);
        });
        repository.Verify(x => x.SaveChangesAsync(), Times.Exactly(3));
        email.Verify(x => x.SendAsync("user@example.com", "Document processing completed", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        worker.Dispose();
    }

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldMarkFailedWhenApiResponseFails()
    {
        var job = new ProcessingJobEntity { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending };
        var repository = CreateRepository(job);
        var client = new Mock<IProcessingApiClient>();
        client.Setup(x => x.SendProcessingJobAsync(
                It.IsAny<ProcessingJobEntity>(), It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingApiResponse { Success = false, ErrorMessage = "internal details" });
        var documents = CreateDocumentRepository(job);
        var email = new Mock<IEmailService>();
        var users = CreateUserRepository();
        var worker = CreateWorker(repository.Object, client.Object, documents.Object, email.Object, users.Object);

        await worker.ProcessPendingJobsAsync();

        Assert.Equal(ProcessingJobStatus.Failed, job.Status);
        Assert.NotNull(job.FinishedAt);
        Assert.Equal("FastAPI processing failed.", job.ErrorMessage);
        email.Verify(x => x.SendAsync(It.IsAny<string>(), "Document processing failed", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        worker.Dispose();
    }

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldKeepCompletedWhenEmailFails()
    {
        var job = new ProcessingJobEntity { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending };
        var repository = CreateRepository(job);
        var client = new Mock<IProcessingApiClient>();
        client.Setup(x => x.SendProcessingJobAsync(It.IsAny<ProcessingJobEntity>(), It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingApiResponse { Success = true });
        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
        var worker = CreateWorker(repository.Object, client.Object, CreateDocumentRepository(job).Object,
            email.Object, CreateUserRepository().Object);

        await worker.ProcessPendingJobsAsync();

        Assert.Equal(ProcessingJobStatus.Completed, job.Status);
        Assert.NotNull(job.FinishedAt);
        worker.Dispose();
    }

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldKeepFailedWhenEmailFails()
    {
        var job = new ProcessingJobEntity { Id = Guid.NewGuid(), Status = ProcessingJobStatus.Pending };
        var repository = CreateRepository(job);
        var client = new Mock<IProcessingApiClient>();
        client.Setup(x => x.SendProcessingJobAsync(It.IsAny<ProcessingJobEntity>(), It.IsAny<DocumentEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("unavailable"));
        var email = new Mock<IEmailService>();
        email.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
        var worker = CreateWorker(repository.Object, client.Object, CreateDocumentRepository(job).Object,
            email.Object, CreateUserRepository().Object);

        await worker.ProcessPendingJobsAsync();

        Assert.Equal(ProcessingJobStatus.Failed, job.Status);
        Assert.NotNull(job.FinishedAt);
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

    [Fact]
    public async Task ProcessPendingJobsAsync_ShouldMarkJobFailedWhenDocumentIsMissing()
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
        var documentRepository = new Mock<IRepository<DocumentEntity>>();
        documentRepository.Setup(x => x.GetAsync<DocumentEntity>(
                It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(),
                It.IsAny<Expression<Func<DocumentEntity, bool>>>(), null, null, false))
            .ReturnsAsync((DocumentEntity?)null);
        var worker = CreateWorker(repository.Object, new Mock<IProcessingApiClient>().Object,
            documentRepository.Object);

        await worker.ProcessPendingJobsAsync();

        Assert.Equal(ProcessingJobStatus.Failed, job.Status);
        Assert.NotNull(job.FinishedAt);
        Assert.Equal("Processing API communication failed.", job.ErrorMessage);
        worker.Dispose();
    }

    private static ProcessingJobWorker CreateWorker(
        IRepository<ProcessingJobEntity> repository,
        IProcessingApiClient client,
        IRepository<DocumentEntity> documentRepository,
        IEmailService? email = null,
        IRepository<Domain.Models.User>? users = null)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IRepository<ProcessingJobEntity>)))
            .Returns(repository);
        serviceProvider.Setup(x => x.GetService(typeof(IProcessingApiClient)))
            .Returns(client);
        serviceProvider.Setup(x => x.GetService(typeof(IRepository<DocumentEntity>)))
            .Returns(documentRepository);
        var notification = new Mock<INotificationService>();
        notification.Setup(x => x.CreateAsync(It.IsAny<Domain.Dto.Notifications.CreateNotificationDto>(), It.IsAny<string>()))
            .ReturnsAsync(new Domain.Models.Notification());
        serviceProvider.Setup(x => x.GetService(typeof(INotificationService)))
            .Returns(notification.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IEmailService)))
            .Returns(email ?? new Mock<IEmailService>().Object);
        serviceProvider.Setup(x => x.GetService(typeof(IRepository<Domain.Models.User>)))
            .Returns(users ?? CreateUserRepository().Object);
        var scope = new Mock<IServiceScope>();
        scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);
        return new ProcessingJobWorker(scopeFactory.Object);
    }

    private static Mock<IRepository<Domain.Models.User>> CreateUserRepository()
    {
        var repository = new Mock<IRepository<Domain.Models.User>>();
        repository.Setup(x => x.GetAsync<Domain.Models.User>(It.IsAny<Expression<Func<Domain.Models.User, Domain.Models.User>>>(), It.IsAny<Expression<Func<Domain.Models.User, bool>>>(), null, null, false))
            .ReturnsAsync(new Domain.Models.User { Id = "user-1", Email = "user@example.com" });
        return repository;
    }

    private static Mock<IRepository<ProcessingJobEntity>> CreateRepository(ProcessingJobEntity job)
    {
        var repository = new Mock<IRepository<ProcessingJobEntity>>();
        repository.Setup(x => x.GetAllAsync<ProcessingJobEntity>(It.IsAny<Expression<Func<ProcessingJobEntity, ProcessingJobEntity>>>(), It.IsAny<Expression<Func<ProcessingJobEntity, bool>>>(), It.IsAny<Func<IQueryable<ProcessingJobEntity>, IOrderedQueryable<ProcessingJobEntity>>>(), null, 10)).ReturnsAsync(new List<ProcessingJobEntity> { job });
        repository.Setup(x => x.UpdateAsync(It.IsAny<ProcessingJobEntity>())).ReturnsAsync((ProcessingJobEntity value) => value);
        return repository;
    }

    private static Mock<IRepository<DocumentEntity>> CreateDocumentRepository(ProcessingJobEntity job)
    {
        var repository = new Mock<IRepository<DocumentEntity>>();
        repository.Setup(x => x.GetAsync<DocumentEntity>(It.IsAny<Expression<Func<DocumentEntity, DocumentEntity>>>(), It.IsAny<Expression<Func<DocumentEntity, bool>>>(), null, null, false)).ReturnsAsync(new DocumentEntity { Id = job.DocumentId });
        return repository;
    }
}
