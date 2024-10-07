using Isolaatti.Accounts.Data.Entity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Utils;

public class IsolaattiPageModel : PageModel
{

    public new Profile Profile { get; set; }

    
    public bool HideNav { get; set; }

    public const string ViewDataNameKey = "name";
    public const string ViewDataEmailKey = "email";
    public const string ViewDataUserIdKey = "userId";
    public const string ViewDataProfilePicUrlKey = "profilePicUrl";
    public const string ViewDataJwtTokenKey = "token";
}