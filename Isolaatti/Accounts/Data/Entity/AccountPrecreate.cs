using System;
using System.ComponentModel.DataAnnotations;

namespace Isolaatti.Accounts.Data.Entity;

public class AccountPrecreate
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime CreationDateTime { get; set; } = DateTime.UtcNow;
    public bool Validated { get; set; }
}