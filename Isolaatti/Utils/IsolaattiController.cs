using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Accounts.SignIn.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

public class IsolaattiController : ControllerBase
{
    public new User User { get; set; }
    /// <summary>
    /// Current session dto
    /// </summary>
    public Session CurrentSession { get; set; }
}