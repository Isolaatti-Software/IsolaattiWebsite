using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Enums;
using isolaatti_API.Models;
using isolaatti_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
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