using System;
using System.Collections.Generic;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class EditSingleTrackProperties : ControllerBase
    {
        private readonly DbContextApp Db;
        
        public EditSingleTrackProperties(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        [HttpPost]
        public IActionResult Index()
        {
            return Ok();
        }
        
        [Route("SetColor")]
        [HttpPost]
        public IActionResult SetColor([FromForm] int songId, [FromForm] string trackName, [FromForm] string htmlHexColor)
        {
            var song = Db.Songs.Find(songId);
            if (song == null) return NotFound("Track not found");


            try
            {
                var tracksProperties = JsonSerializer.Deserialize<List<TrackPreferences>>(song.TracksSettings);
                var indexOfElementToEdit = tracksProperties
                    .FindIndex(element => element.Name.Equals(trackName));

                var elementToEdit = tracksProperties[indexOfElementToEdit];
                elementToEdit.HtmlColorHex = htmlHexColor;

                tracksProperties[indexOfElementToEdit] = elementToEdit;


                song.TracksSettings = JsonSerializer.Serialize(tracksProperties);
                Db.Songs.Update(song);
                Db.SaveChanges();
                return Ok("Color changed!");
            }
            catch (NullReferenceException)
            {
                return Problem($"Track with name {trackName} was not found", statusCode: 500);
            }


        }
    }
}