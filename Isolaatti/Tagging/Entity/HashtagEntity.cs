using System.Collections.ObjectModel;
using Isolaatti.Models;

namespace Isolaatti.Tagging.Entity;

public class HashtagEntity
{
    public long Id { get; set; }
    public string Text { get; set; }
    public long PostId { get; set; }
    
    public Collection<Post> Posts { get; set; }
}