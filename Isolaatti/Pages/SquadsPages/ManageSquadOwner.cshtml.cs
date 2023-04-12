using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Pages.SquadsPages;

[IsolaattiAuth]
public class ManageSquadOwner : IsolaattiPageModel
{
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;

    public ManageSquadOwner(SquadsRepository squadsRepository, DbContextApp db)
    {
        _squadsRepository = squadsRepository;
        _db = db;
    }
    
    public Squad Squad { get; set; }
    public UserFeed Owner { get; set; }
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        Squad = await _squadsRepository.GetSquad(squadId);
        if (Squad == null)
        {
            return NotFound();
        }

        Owner = _db.Users.Where(u => u.Id == Squad.UserId).Select(u => new UserFeed()
        {
            Id = u.Id,
            Name = u.Name,
            ImageId = u.ProfileImageId
        }).FirstOrDefault();
        return Page();
    }
}