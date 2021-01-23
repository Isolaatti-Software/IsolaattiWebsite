/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReserveSongFromQueue : Controller
    {
        private readonly DbContextApp db;

        public ReserveSongFromQueue(DbContextApp dbContext)
        {
            db = dbContext;
        }
        [HttpPost]
        public void Index([FromForm]int elementId)
        {
            var elementToReserve = db.SongsQueue.Find(elementId);
            elementToReserve.Reserved = true;
            db.SongsQueue.Update(elementToReserve);
            db.SaveChanges();
        }
    }
}