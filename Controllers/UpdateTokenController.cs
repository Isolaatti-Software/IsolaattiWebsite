using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UpdateTokenController : ControllerBase
    {
        private readonly DbContextApp db;
        public UpdateTokenController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        public void Index(int userId, string token)
        {
            User user = db.Users.Find(userId);
            user.googleToken = token;
        }
    }
}