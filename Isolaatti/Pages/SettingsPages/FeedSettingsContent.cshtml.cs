using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Pages.SettingsPages;
[IsolaattiAuth]
public class FeedSettingsContent : IsolaattiPageModel
{
    private readonly DbContextApp _db;

    public FeedSettingsContent(DbContextApp dbContextApp)
    {
        _db = dbContextApp;
    }

    [BindProperty] public bool ShowOwnPostsOnFeed { get; set; }
    [BindProperty] public bool PreferencesUpdated { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ShowOwnPostsOnFeed = _db.FollowerRelations.Any(fr => fr.TargetUserId == User.Id && fr.UserId == User.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostShowOwnPostsSet()
    {
        if (_db.FollowerRelations.Any(fr => fr.TargetUserId == User.Id && fr.UserId == User.Id) == ShowOwnPostsOnFeed)
        {
            PreferencesUpdated = true;
            return Page();
        }

        if (ShowOwnPostsOnFeed)
        {
            _db.FollowerRelations.Add(new FollowerRelation
            {
                UserId = User.Id,
                TargetUserId = User.Id
            });
        }
        else
        {
            var frToDelete = _db.FollowerRelations
                .Single(fr => fr.UserId == User.Id && fr.TargetUserId == User.Id);

            _db.FollowerRelations.Remove(frToDelete);
        }

        PreferencesUpdated = true;

        await _db.SaveChangesAsync();
        return Page();
    }
}