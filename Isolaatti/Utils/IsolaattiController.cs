using Isolaatti.Accounts.Data.Entity;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

public class IsolaattiController : ControllerBase
{
    public new User User { get; set; }
    /// <summary>
    /// Current session dto
    /// </summary>
    public SessionDto CurrentSessionDto { get; set; }
}