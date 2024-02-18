using System;

namespace Isolaatti.Accounts.Data.Entity;

public class AccountRemovalRequestEntity
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string KeyHash { get; set; }
}