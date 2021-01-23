/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetJobFromQueue : ControllerBase
    {
        private readonly DbContextApp db;

        public GetJobFromQueue(DbContextApp _dbContext)
        {
            db = _dbContext;
        }
        [HttpGet]
        public SongQueue Index()
        {
            try
            {
                // returns the first element that is not reserved, as it's the oldest element on the queue
                return db.SongsQueue.First(element => element.Reserved.Equals(false));
            }
            // this means there's not any element on queue
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}