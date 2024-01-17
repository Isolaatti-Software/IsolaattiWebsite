using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Service;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Accounts.Controller;

[ApiController]
[Route("/api/account")]
public class AccountsController : IsolaattiController
{
    private readonly IAccountsService _accounts;
    public AccountsController(IAccountsService accountsService)
    {
        _accounts = accountsService;
    }
    
    [HttpGet]
    [Route("get_sessions")]
    [IsolaattiAuth]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _accounts.GetSessionsOfUser(User.Id);
        return Ok(new
        {
            data = sessions.Select(s => new
            {
                Id = s.Id,
                Date = s.CreationDate,
                Ip = s.IpAddress,
                UserAgent = s.UserAgent,
                Current = s.Id == SessionId
            })
        });
    }

    [HttpPost]
    [Route("password/change")]
    [IsolaattiAuth]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, [FromQuery] bool signOut, [FromQuery] bool signOutCurrent = false)
    {
        var result = await _accounts.ChangeAPassword(User.Id, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
        if (!result.Success)
        {
            return Ok(new
            {
                result
            });
        }
        if (signOut)
        {
            var list = new List<string>();
            if (!signOutCurrent)
            {
                list.Add((await _accounts.CurrentSession()).GetDto().SessionId);
            }
            await _accounts.RemoveAllSessions(User.Id, list);
        }
        return Ok(new
        {
            result
        });
    }

    [HttpPost]
    [Route("sign_out")]
    [IsolaattiAuth]
    public async Task<IActionResult> Logout()
    {
        var result = await _accounts.RemoveSession((await _accounts.CurrentSession()).GetDto());
        return Ok(new
        {
            SessionRemoved = result
        });
    }

    [HttpPost]
    [Route("sign_out_sessions")]
    [IsolaattiAuth]
    public async Task<IActionResult> LogoutByIds([FromBody] LogoutSessionsDto sessionsIdsDto)
    {
        var result = await _accounts.RemoveSessions(User.Id, sessionsIdsDto.Ids);

        return Ok(new
        {
            result
        });
    }
}