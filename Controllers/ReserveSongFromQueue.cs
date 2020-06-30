using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
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