/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DeleteElementFromQueue : Controller
    {
        private readonly DbContextApp _db;
        public DeleteElementFromQueue(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm]int id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            try
            {
                _db.SongsQueue.Remove(_db.SongsQueue.Find(id));
                _db.SaveChanges();
                return Ok();
            }
            catch (NullReferenceException)
            {
                return NotFound("Element with id " + id + " was not found");
            }
        }
    }
}