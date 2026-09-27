using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Dto.ProcessingResults;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Service.Interface;
using Web.Authentication;
using Web.Controllers;
using Web.Mapper;
using Web.Request.ProcessingJobs;
using ProcessingJobEntity = Domain.Models.ProcessingJob;
using SemanticBlockEntity = Domain.Models.SemanticBlock;
using TripleEntity = Domain.Models.Triple;
using TripleProvenanceEntity = Domain.Models.TripleProvenance;
using ApiKeyEntity = Domain.Models.ApiKey;

namespace Tests.ProcessingJob;

public class ProcessingCallbackAuthenticationTests
{
    [Fact]
    public async Task AuthenticatedCallback_ShouldPersistResult_AndRejectInvalidApiKey()
    {
        var user = new User { Id = "user-1", UserName = "processor" };
        var apiKeys = new Mock<IApiKeyService>();
        apiKeys.Setup(x => x.ValidateAsync("callback-key"))
            .ReturnsAsync(new ApiKeyEntity { UserId = user.Id, IsActive = true });
        var userManager = CreateUserManager(user);
        var authenticatedContext = new DefaultHttpContext();
        authenticatedContext.Request.Headers["X-API-Key"] = "callback-key";
        var authenticated = await AuthenticateAsync(
            authenticatedContext, apiKeys.Object, userManager.Object);

        Assert.True(authenticated.Succeeded);
        Assert.Equal(user.Id, authenticated.Principal!.FindFirstValue(ClaimTypes.NameIdentifier));

        var jobId = Guid.NewGuid();
        var store = new CallbackStore(jobId);
        var controller = new ProcessingJobController(
            new ProcessingJobMapper(new Mock<IProcessingJobService>().Object),
            new ProcessingResultMapper(store));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = authenticatedContext
        };

        var request = new ProcessingResultRequest
        {
            SemanticBlocks = new List<ProcessingSemanticBlockDto>
            {
                new() { BlockIndex = 0, Text = "Block", Page = 1 }
            },
            Triples = new List<ProcessingTripleDto>
            {
                new()
                {
                    Subject = "A",
                    Predicate = "knows",
                    Object = "B",
                    Confidence = 0.9m,
                    Status = TripleStatus.Validated,
                    SemanticBlockIndex = 0
                }
            }
        };

        var response = await controller.Result(jobId, request);

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(ProcessingJobStatus.Completed, store.Job.Status);
        Assert.Single(store.SemanticBlocks);
        Assert.Single(store.Triples);
        Assert.Single(store.TripleProvenances);

        var invalidContext = new DefaultHttpContext();
        invalidContext.Request.Headers["X-API-Key"] = "wrong-key";
        var invalid = await AuthenticateAsync(
            invalidContext, apiKeys.Object, userManager.Object);

        Assert.False(invalid.Succeeded);
        Assert.Equal("Invalid API key.", invalid.Failure!.Message);
    }

    private static Mock<UserManager<User>> CreateUserManager(User user)
    {
        var store = new Mock<IUserStore<User>>();
        var manager = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        manager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
        manager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        return manager;
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(
        HttpContext context,
        IApiKeyService apiKeys,
        UserManager<User> userManager)
    {
        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(x => x.Get(ApiKeyAuthenticationHandler.Scheme))
            .Returns(new AuthenticationSchemeOptions());
        var handler = new ApiKeyAuthenticationHandler(
            options.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            new SystemClock(),
            apiKeys,
            userManager);
        await handler.InitializeAsync(new AuthenticationScheme(
            ApiKeyAuthenticationHandler.Scheme,
            ApiKeyAuthenticationHandler.Scheme,
            typeof(ApiKeyAuthenticationHandler)), context);
        return await handler.AuthenticateAsync();
    }

    private sealed class CallbackStore : IProcessingResultService
    {
        public CallbackStore(Guid jobId)
        {
            Job = new ProcessingJobEntity
            {
                Id = jobId,
                Status = ProcessingJobStatus.Processing
            };
        }

        public ProcessingJobEntity Job { get; }
        public List<SemanticBlockEntity> SemanticBlocks { get; } = new();
        public List<TripleEntity> Triples { get; } = new();
        public List<TripleProvenanceEntity> TripleProvenances { get; } = new();

        public Task<ProcessingResultSummary> PersistAsync(
            Guid processingJobId,
            ProcessingResultDto result,
            CancellationToken cancellationToken = default)
        {
            foreach (var block in result.SemanticBlocks)
            {
                SemanticBlocks.Add(new SemanticBlockEntity
                {
                    Id = Guid.NewGuid(),
                    DocumentId = Job.DocumentId,
                    BlockIndex = block.BlockIndex,
                    Text = block.Text,
                    Page = block.Page
                });
            }

            foreach (var triple in result.Triples)
            {
                var persistedTriple = new TripleEntity
                {
                    Id = Guid.NewGuid(),
                    Subject = triple.Subject,
                    Predicate = triple.Predicate,
                    Object = triple.Object,
                    Confidence = triple.Confidence,
                    Status = triple.Status
                };
                Triples.Add(persistedTriple);
                TripleProvenances.Add(new TripleProvenanceEntity
                {
                    TripleId = persistedTriple.Id,
                    DocumentId = Job.DocumentId,
                    SemanticBlockId = SemanticBlocks.Single(x => x.BlockIndex == triple.SemanticBlockIndex).Id
                });
            }

            Job.Status = ProcessingJobStatus.Completed;
            return Task.FromResult(new ProcessingResultSummary
            {
                ProcessingJobId = processingJobId,
                Status = Job.Status.ToString(),
                SemanticBlocks = SemanticBlocks.Count,
                Triples = Triples.Count
            });
        }
    }
}
