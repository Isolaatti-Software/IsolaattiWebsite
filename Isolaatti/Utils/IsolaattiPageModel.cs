using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Isolaatti.Utils;

public class IsolaattiPageModel : PageModel
{

    public new User User { get; set; }
    public Session Session {get;set;}

    
    public bool HideNav { get; set; }

    public const string ViewDataNameKey = "name";
    public const string ViewDataEmailKey = "email";
    public const string ViewDataUserIdKey = "userId";
    public const string ViewDataProfilePicUrlKey = "profilePicUrl";
    public const string ViewDataJwtTokenKey = "token";
}