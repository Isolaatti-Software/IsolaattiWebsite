using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

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
        public IActionResult Index(int id, string code)
        {
            try
            {
                User userToValidate = dbContext.Users.Find(id);
                if(userToValidate.Uid == code)
                {
                    userToValidate.EmailValidated = true;
                    dbContext.Users.Update(userToValidate);
                    dbContext.SaveChanges();
                }
                else
                {
                    return StatusCode(404);
                }

                return RedirectToPage("/WebApp/LogIn", new
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