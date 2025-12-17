using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Accounts.SignIn.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

public class IsolaattiController : ControllerBase
{
    public new User User => HttpContext.Items["CurrentUser"] as User;

    /// <summary>
    /// Current session dto
    /// </summary>
    public Session CurrentSession => HttpContext.Items["CurrentSession"] as Session;
}