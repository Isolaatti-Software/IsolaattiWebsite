using System.Linq;
using System.Security.Claims;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Database.EF;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

/// <summary>
/// Implement this class in controllers to get <c>Profile</c> field automatically populated by <c>AuthenticationFilter</c>
///
/// Check
/// <seealso cref="Isolaatti.Utils.ActionFilters.AuthenticationFilter"/>
/// </summary>
public class ProfileRequiredController : ControllerBase
{
    public Profile? Profile { get; set; }
}