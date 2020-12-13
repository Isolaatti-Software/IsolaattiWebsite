using System;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeleteElementFromQueue : Controller
    {
        private readonly DbContextApp _dbContextApp;
        public DeleteElementFromQueue(DbContextApp dbContextApp)
        {
            _dbContextApp = dbContextApp;
        }
        [HttpPost]
        public bool Index([FromForm]int id)
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