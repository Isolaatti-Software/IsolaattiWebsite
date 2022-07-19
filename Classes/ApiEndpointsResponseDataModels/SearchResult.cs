using Isolaatti.Enums;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class SearchResult
{
    public SearchResultType ResultType { get; set; }
    public string ResourceId { get; set; }
    public string ContentPreview { get; set; }
}