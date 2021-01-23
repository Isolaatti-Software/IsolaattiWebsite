/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DeleteElementFromQueue : Controller
    {
        private readonly DbContextApp _dbContextApp;
        public DeleteElementFromQueue(DbContextApp dbContextApp)
        {
            _dbContextApp = dbContextApp;
        }
        [HttpPost]
        public bool Index([FromForm]int id, [FromForm]int userId, [FromForm]string password)
        {
            try
            {
                _dbContextApp.SongsQueue.Remove(
                    _dbContextApp.SongsQueue.Find(id));
                _dbContextApp.SaveChanges();
                return true;
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }
    }
}