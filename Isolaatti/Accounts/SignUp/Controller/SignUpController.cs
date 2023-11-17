using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Service;
using Isolaatti.Accounts.SignUp.Data;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Config;
using Isolaatti.Enums;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Isolaatti.Accounts.SignUp.Controller;

[ApiController]
[Route("/api/signUp")]
public class SignUpController : ControllerBase
{
    private readonly IAccountsService _accounts;
    private readonly List<Client> _clients;

    public SignUpController(IAccountsService accounts, IOptions<List<Client>> clients)
    {
        _accounts = accounts;
        _clients = clients.Value;
    }


    [HttpPost]
    [Route(("get_code"))]
    public async Task<IActionResult> Index([FromHeader(Name = "clientId")] string clientId, [FromHeader(Name = "clientSecret")] string clientSecret, SimpleStringData email)
    {
        var client = _clients.Find(c => c.Id == clientId);
        if (client == null)
        {
            return Unauthorized("Invalid client id");
        }

        if (client.Secret != clientSecret)
        {
            return Unauthorized("Invalid secret");
        }

        if (!client.SpecialPermissions.Contains(Client.SpecialPermissionSignUp))
        {
            return Unauthorized("Api client unauthorized to sign up");
        }

        var result = await _accounts.PreCreateAccount(email.Data);
        return Ok(new { result = result.ToString()});
    }

    [HttpPost]
    [Route("validate_code")]
    public async Task<IActionResult> ValidateCode(
        [FromHeader(Name = "clientId")] string clientId, 
        [FromHeader(Name = "clientSecret")] string clientSecret, 
        SimpleStringData code)
    {
        var client = _clients.Find(c => c.Id == clientId);
        if (client == null)
        {
            return Unauthorized("Invalid client id");
        }

        if (client.Secret != clientSecret)
        {
            return Unauthorized("Invalid secret");
        }

        if (!client.SpecialPermissions.Contains(Client.SpecialPermissionSignUp))
        {
            return Unauthorized("Api client unauthorized to sign up");
        }

        var accountPrecreate = await _accounts.ValidatePreCreateCode(code.Data);
        
        return Ok(new {valid = accountPrecreate != null});
    }

    [HttpPost]
    [Route("sign_up_with_code")]
    public async Task<IActionResult> SignUpWithCode(
        [FromHeader(Name = "clientId")] string clientId, 
        [FromHeader(Name = "clientSecret")] string clientSecret, 
        SignUpDto signUpDto)
    {
        var client = _clients.Find(c => c.Id == clientId);
        if (client == null)
        {
            return Unauthorized("Invalid client id");
        }

        if (client.Secret != clientSecret)
        {
            return Unauthorized("Invalid secret");
        }

        if (!client.SpecialPermissions.Contains(Client.SpecialPermissionSignUp))
        {
            return Unauthorized("Api client unauthorized to sign up");
        }

        var accountPrecreate = await _accounts.ValidatePreCreateCode(signUpDto.Code);

        if (accountPrecreate is null)
        {
            return NotFound();
        }

        var result = 
            await _accounts.MakeAccountAsync(signUpDto.Username, signUpDto.DisplayName, accountPrecreate.Email, signUpDto.Password);

        if (result is { AccountMakingResult: AccountMakingResult.Ok, UserId: not null })
        {
            var session = await _accounts.CreateNewSession(result.UserId.Value, signUpDto.Password);
            return Ok(new SignUpWithCodeDto
            {
                AccountMakingResult = result.AccountMakingResult.ToString(),
                Session = session
            });
        }
        
        return Ok(new SignUpWithCodeDto
        {
            AccountMakingResult = result.AccountMakingResult.ToString(),
            Session = null
        });
    }
}