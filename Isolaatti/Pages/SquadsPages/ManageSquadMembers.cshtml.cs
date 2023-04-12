using System;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SquadsPages;

[IsolaattiAuth]
public class ManageSquadMembers : IsolaattiPageModel
{
    private readonly SquadsRepository _squadsRepository;

    public ManageSquadMembers(SquadsRepository squadsRepository)
    {
        _squadsRepository = squadsRepository;
    }
    
    public Squad Squad { get; set; }
    public async Task<IActionResult> OnGet(Guid squadId)
    {
        Squad = await _squadsRepository.GetSquad(squadId);
        if (Squad == null)
        {
            return NotFound();
        }

        return Page();
    }
}