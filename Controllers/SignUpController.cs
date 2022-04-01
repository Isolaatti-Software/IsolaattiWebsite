/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

//Handles the data to create a new account for users

using System.Threading.Tasks;
using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.Enums;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly DbContextApp dbContextApp;

        public SignUpController(DbContextApp context)
        {
            dbContextApp = context;
        }

        [HttpPost]
        public async Task<IActionResult> Index(SignUpDataModel signUpData)
        {
            var accounts = new Accounts(dbContextApp);
            var result = await accounts.MakeAccountAsync(signUpData.Username, signUpData.Email, signUpData.Password);
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