using isolaatti_API.Enums;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels;

public class SearchResult
{
    public SearchResultType ResultType { get; set; }
    public string ResourceId { get; set; }
    public string ContentPreview { get; set; }
}