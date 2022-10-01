using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;

        public SignUpController(DbContextApp context, IAccounts accounts)
        {
            _db = context;
            _accounts = accounts;
        }

        [HttpPost]
        public async Task<IActionResult> Index(SignUpDataModel signUpData)
        {
            var result = await _accounts.MakeAccountAsync(signUpData.Username, signUpData.Email, signUpData.Password);
            return result switch
            {
                AccountMakingResult.Ok => Ok(),
                AccountMakingResult.EmptyFields => BadRequest(new { error = "Empty fields" }),
                AccountMakingResult.EmailNotAvailable => Unauthorized(new { error = "Email already used" }),
                AccountMakingResult.Error => Problem(title: "A database error occurred"),
                _ => NoContent()
            };
        }
    }
}