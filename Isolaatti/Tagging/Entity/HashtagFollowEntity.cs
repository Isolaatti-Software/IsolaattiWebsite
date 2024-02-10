using System;

namespace Isolaatti.Tagging.Entity;

public class HashtagFollowEntity
{
    public Guid Id { get; set; }
    public string Hashtag { get; set; }
    public int UserId { get; set; }
}