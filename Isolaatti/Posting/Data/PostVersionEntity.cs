using System;
using System.Text.Json.Serialization;

namespace Isolaatti.Posting.Data;

public class PostVersionEntity
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string TextContent { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public string? AudioId { get; set; }
    [JsonIgnore]
    public Post Post { get; set; }
}