/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Collections.Generic;
using System.Text.Json;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CreateSongRecord : ControllerBase
    {
        private readonly DbContextApp _contextApp;

        public CreateSongRecord(DbContextApp contextApp)
        {
            _contextApp = contextApp;
        }
        [HttpPost]
        public int Index([FromForm]int userId, [FromForm]string fileName, [FromForm]string songArtist="Unknown")
        {
            var listOfPreferences = new List<TrackPreferences>();
            listOfPreferences.Add(new TrackPreferences()
            {
                Name = "Bass",
                GainValue = 1.0f,
                HtmlColorHex = "#000000",
                PanningValue = 0
            });
            listOfPreferences.Add(new TrackPreferences()
            {
                Name = "Drums",
                GainValue = 1.0f,
                HtmlColorHex = "#000000",
                PanningValue = 0
            });
            listOfPreferences.Add(new TrackPreferences()
            {
                Name = "Vocals",
                GainValue = 1.0f,
                HtmlColorHex = "#000000",
                PanningValue = 0
            });
            listOfPreferences.Add(new TrackPreferences()
            {
                Name = "Other",
                GainValue = 1.0f,
                HtmlColorHex = "#000000",
                PanningValue = 0
            });
            
            Song songToAdd = new Song()
            {
                OwnerId = userId,
                OriginalFileName = fileName,
                Artist = songArtist,
                TracksSettings = JsonSerializer.Serialize(listOfPreferences)
            };
            _contextApp.Songs.Add(songToAdd);
            _contextApp.SaveChanges();
            
            return songToAdd.Id;
        }
    }
}