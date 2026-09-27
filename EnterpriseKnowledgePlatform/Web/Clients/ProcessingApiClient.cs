using Domain.Models;
using Web.Services;
using Web.Response;

namespace Web.Clients;

public class ProcessingApiClient : IProcessingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _fileStorageService;

    public ProcessingApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        IFileStorageService fileStorageService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _fileStorageService = fileStorageService;
    }

    public async Task<ProcessingApiResponse> SendProcessingJobAsync(
        ProcessingJob processingJob,
        Document document,
        CancellationToken cancellationToken = default)
    {
        var path = _configuration["FastApi:ProcessingPath"] ?? "processing/jobs";
        await using var stream = await _fileStorageService.OpenReadAsync(
            document.FilePath, cancellationToken);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(processingJob.Id.ToString()), "processingJobId");
        content.Add(new StringContent(document.Id.ToString()), "documentId");
        content.Add(new StreamContent(stream), "file", document.Name);

        using var response = await _httpClient.PostAsync(path, content, cancellationToken);

        response.EnsureSuccessStatusCode();

        return new ProcessingApiResponse
        {
            Success = true
        };
    }
}