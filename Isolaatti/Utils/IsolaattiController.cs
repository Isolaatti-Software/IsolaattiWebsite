using Isolaatti.Models;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

public class IsolaattiController : ControllerBase
{
    public new User User { get; set; }
    public string SessionId { get; set; }
}