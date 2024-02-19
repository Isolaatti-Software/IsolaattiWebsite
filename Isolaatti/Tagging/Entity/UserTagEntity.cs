using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Models;

namespace Isolaatti.Tagging.Entity;

public class UserTagEntity
{
    public long Id { get; set; }
    public int TaggedUserId { get; set; }
    public string Username { get; set; }
    public long PostId { get; set; }
    
    public User TaggedUser { get; set; }
    public Post Post { get; set; }
}