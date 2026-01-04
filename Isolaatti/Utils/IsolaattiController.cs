using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Accounts.SignIn.Data;
using Isolaatti.Accounts.SignIn.Dto;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Utils;

[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}, {CookieAuthenticationDefaults.AuthenticationScheme}")]
public class IsolaattiController : ControllerBase
{
    public new User User => HttpContext.Items["CurrentUser"] as User;
}