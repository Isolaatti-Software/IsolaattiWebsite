using System;
using Isolaatti.DTOs;

namespace Isolaatti.Favorites.Data;

public class FavoriteDto
{
    public Guid Id { get; set; }
    public PostDto Post { get; set; }
}