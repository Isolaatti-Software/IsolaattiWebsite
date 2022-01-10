/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

//Handles the data to create a new account for users

using isolaatti_API.Classes.ApiEndpointsRequestDataModels;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class SignIn : ControllerBase
    {
        private readonly DbContextApp dbContextApp;

        public SignIn(DbContextApp context)
        {
            dbContextApp = context;
        }

        /*
         Return codes:
         1 => An existing user has used the same email address
         2 => An existing user has the same username
         3 => By an unknown reason, one or more of the fields are empty
         0 => Everything is OK, the user is now recorded
         Exception => Returns the exception string if something didn't go well on the server
        */
        [HttpPost]
        public string Index(SignUpDataModel signUpData)
        {
            Accounts accounts = new Accounts(dbContextApp);
            return accounts.MakeAccount(signUpData.Username, signUpData.Email, signUpData.Password);
        }
    }
}