using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Accounts.SignUp.Data;
using Isolaatti.Config;
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
    public async Task<IActionResult> Index([FromHeader] string clientId, [FromHeader] string clientSecret, SignUpDto signUpDto)
    {
        if (clientId == null || clientSecret == null)
        {
            return Unauthorized("Invalid api client id or secret");
        }

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

        await _accounts.MakeAccountAsync(signUpDto.Username, signUpDto.DisplayName, signUpDto.Email, signUpDto.Password);
        return Ok();
    }
}