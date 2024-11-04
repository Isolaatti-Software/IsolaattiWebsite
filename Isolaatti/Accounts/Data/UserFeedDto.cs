using System;

namespace Isolaatti.Accounts.Data;

public class UserFeedDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Guid? ImageId { get; set; }
    public bool Following { get; set; }
}