using System;
using System.Threading.Tasks;
using Isolaatti.Favorites.Data;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Favorites.Controller;

[ApiController]
[Route("/api/favorites")]
public class FavoritesController : IsolaattiController
{
    private readonly FavoritesRepository _favoritesRepository;
    
    public FavoritesController(FavoritesRepository favoritesRepository)
    {
        _favoritesRepository = favoritesRepository;
    }
    
    [HttpGet]
    [IsolaattiAuth]
    public async Task<IActionResult> GetFavorites()
    {
        
        return Ok(await _favoritesRepository.GetUserFavorites(User.Id));
    }

    [IsolaattiAuth]
    [HttpPost]
    public async Task<IActionResult> AddToFavorites([FromQuery] int postId)
    {
        return Ok(new {Added = await _favoritesRepository.AddToFavorites(postId, User.Id)});
    }

    [IsolaattiAuth]
    [HttpDelete]
    [Route("{favoriteId:guid}")]
    public async Task<IActionResult> RemoveFromFavorites(Guid favoriteId)
    {
        return Ok(new {Removed = await _favoritesRepository.RemoveFromFavorites(favoriteId, User.Id)});
    }
}