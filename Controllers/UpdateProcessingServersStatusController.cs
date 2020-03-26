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
    public class UpdateProcessingServersStatusController : ControllerBase
    {
        private readonly DbContextApp db;
        public UpdateProcessingServersStatusController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
    }
}