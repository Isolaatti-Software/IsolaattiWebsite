using System;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Models;

namespace Isolaatti.Favorites.Data;

public class FavoriteEntity
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public long PostId { get; set; }
    public virtual Post Post { get; set; }
    public virtual User User { get; set; }
}