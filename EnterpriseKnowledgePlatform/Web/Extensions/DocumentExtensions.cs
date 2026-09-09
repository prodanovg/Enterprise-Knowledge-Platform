using Domain.Dto;
using Domain.Models;
using Web.Response;
using Web.Response.Documents;

namespace Web.Extensions;

public static class DocumentExtensions
{
    public static DocumentResponse ToResponse(this Document document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            Name = document.Name,
            FilePath = document.FilePath,
            FileType = document.FileType,
            Status = document.Status,
            CreatedAt = document.CreatedAt
        };
    }

    public static List<DocumentResponse> ToResponse(
        this IEnumerable<Document> documents)
    {
        return documents.Select(document => document.ToResponse()).ToList();
    }

    public static PaginatedResponse<DocumentResponse> ToResponse(
        this PaginatedResult<Document> result)
    {
        return new PaginatedResponse<DocumentResponse>
        {
            Items = result.Items.ToResponse(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
    }
}
