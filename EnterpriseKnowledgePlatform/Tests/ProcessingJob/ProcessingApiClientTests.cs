using System.Net;
using Microsoft.Extensions.Configuration;
using Moq;
using Web.Clients;
using Web.Services;
using Domain.Models;
using ProcessingJobEntity = Domain.Models.ProcessingJob;
using DocumentEntity = Domain.Models.Document;

namespace Tests.ProcessingJob;

public class ProcessingApiClientTests
{
    [Fact]
    public async Task SendProcessingJobAsync_ShouldSendMultipartJobIdsAndStoredFile()
    {
        using var fileStream = new MemoryStream("file contents"u8.ToArray());
        string? requestBody = null;
        var handler = new CaptureHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"success\":true}", System.Text.Encoding.UTF8, "application/json")
            };
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://fastapi.test/") };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FastApi:ProcessingPath"] = "processing/jobs"
            })
            .Build();
        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.OpenReadAsync("stored.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);
        var client = new ProcessingApiClient(httpClient, configuration, storage.Object);
        var job = new ProcessingJobEntity { Id = Guid.NewGuid() };
        var document = new DocumentEntity { Id = Guid.NewGuid(), FilePath = "stored.pdf", Name = "client-name.pdf" };

        await client.SendProcessingJobAsync(job, document);

        Assert.NotNull(requestBody);
        Assert.Contains($"name=processingJobId", requestBody);
        Assert.Contains(job.Id.ToString(), requestBody);
        Assert.Contains($"name=documentId", requestBody);
        Assert.Contains(document.Id.ToString(), requestBody);
        Assert.Contains("file contents", requestBody);
        storage.Verify(x => x.OpenReadAsync("stored.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }
}
