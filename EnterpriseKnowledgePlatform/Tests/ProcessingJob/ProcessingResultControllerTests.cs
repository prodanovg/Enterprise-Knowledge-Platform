using Domain.Dto.ProcessingResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Service.Interface;
using Web.Controllers;
using Web.Mapper;
using Web.Request.ProcessingJobs;

namespace Tests.ProcessingJob;

public class ProcessingResultControllerTests
{
    private readonly Mock<IProcessingResultService> _resultService = new();
    private readonly ProcessingJobController _controller;

    public ProcessingResultControllerTests()
    {
        _controller = new ProcessingJobController(
            new ProcessingJobMapper(new Mock<IProcessingJobService>().Object),
            new ProcessingResultMapper(_resultService.Object));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Result_ShouldReturnPersistedSummary()
    {
        var id = Guid.NewGuid();
        _resultService.Setup(x => x.PersistAsync(id, It.IsAny<ProcessingResultDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResultSummary { ProcessingJobId = id, Status = "Completed", SemanticBlocks = 1, Triples = 1 });

        var result = await _controller.Result(id, new ProcessingResultRequest());

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(id, Assert.IsType<ProcessingResultSummary>(response.Value).ProcessingJobId);
    }

    [Fact]
    public async Task Result_ShouldReturnNotFoundForMissingJob()
    {
        _resultService.Setup(x => x.PersistAsync(It.IsAny<Guid>(), It.IsAny<ProcessingResultDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        Assert.IsType<NotFoundResult>((await _controller.Result(Guid.NewGuid(), new ProcessingResultRequest())).Result);
    }

    [Fact]
    public async Task Result_ShouldReturnConflictForInvalidJobState()
    {
        _resultService.Setup(x => x.PersistAsync(It.IsAny<Guid>(), It.IsAny<ProcessingResultDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        Assert.IsType<ConflictResult>((await _controller.Result(Guid.NewGuid(), new ProcessingResultRequest())).Result);
    }

    [Fact]
    public async Task Result_ShouldReturnBadRequestForInvalidSemanticBlockReference()
    {
        _resultService.Setup(x => x.PersistAsync(It.IsAny<Guid>(), It.IsAny<ProcessingResultDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException());

        Assert.IsType<BadRequestResult>((await _controller.Result(Guid.NewGuid(), new ProcessingResultRequest())).Result);
    }
}
