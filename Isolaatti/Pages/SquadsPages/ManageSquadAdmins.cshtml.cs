using System;
using System.Collections.Generic;
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
public class ManageSquadAdmins : IsolaattiPageModel
{
    private readonly SquadsRepository _squadsRepository;
    private readonly SquadUsersRepository _squadUsersRepository;

    public ManageSquadAdmins(SquadsRepository squadsRepository, SquadUsersRepository squadUsersRepository)
    {
        _squadsRepository = squadsRepository;
        _squadUsersRepository = squadUsersRepository;
    }
    
    public Squad Squad { get; set; }
    public List<UserFeed> Admins { get; set; }
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        Squad = await _squadsRepository.GetSquad(squadId);
        if (Squad == null)
        {
            return NotFound();
        }
        
        if (!await _squadsRepository.UserBelongsToSquad(User.Id, squadId))
        {
            return NotFound();
        }

        Admins = await _squadUsersRepository.GetAdminsOfSquad(squadId);

        return Page();
    }
}