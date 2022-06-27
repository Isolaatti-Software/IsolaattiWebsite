using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MakeComment : ControllerBase
    {
        private readonly DbContextApp _db;

        public MakeComment(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
    }
}