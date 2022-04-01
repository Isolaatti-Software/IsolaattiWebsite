/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Threading.Tasks;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class validateAccount : ControllerBase
    {
        private readonly DbContextApp dbContext;

        public validateAccount(DbContextApp dbContextApp)
        {
            dbContext = dbContextApp;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id, string code)
        {
            try
            {
                var userToValidate = await dbContext.Users.FindAsync(id);
                if (userToValidate.Uid == code)
                {
                    userToValidate.EmailValidated = true;
                    dbContext.Users.Update(userToValidate);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    return StatusCode(404);
                }

                return RedirectToPage("/LogIn", new
                {
                    justVerified = true,
                    username = userToValidate.Email
                });
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}