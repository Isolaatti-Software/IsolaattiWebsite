using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;
using System.Net.Http;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeleteSongController : ControllerBase
    {
        private readonly DbContextApp db;
        private static readonly HttpClient httpClient;
        static DeleteSongController()
        {
            httpClient = new HttpClient();
        }
        public DeleteSongController(DbContextApp contextApp)
        {
            db = contextApp;
        }
        // normal delete. includes: deleting song record from db, and deleting the files from server
        [HttpPost]
        public void Index([FromForm] int songId)
        {
            Song songToDelete = db.Songs.Find(songId);

            // splits the url to extract the uid generated in the Python server
            string extractedUid = songToDelete.BassUrl.Split("/")[3];

            // deletes actual files
            DeleteRequestToServer(extractedUid, "http://isolaatti-server1.cloudapp.net/");

            // deletes database record of song
            db.Songs.Remove(songToDelete);
            db.SaveChanges();
        }

        // this method is intended to send a post request to the server that is
        // storing the file that is wanted to delete
        public void DeleteRequestToServer(string songUid, string serverHostname)
        {
            string url = serverHostname + "delete_tracks/";

            var values = new Dictionary<string, string>
            {
                {"uid",songUid }
            };

            var content = new FormUrlEncodedContent(values);
            httpClient.PostAsync(url, content);
        }

        // deletes only the record. It might be because of a failed upload from client.
        // possible reasons: processing server is not available, but the record was 
        // created.
        [HttpPost]
        public void DeleteRecordOnly([FromForm] int songId)
        {
            Song songToDelete = db.Songs.Find(songId);

            // deletes database record of song
            db.Songs.Remove(songToDelete);
            db.SaveChanges();
        }
    }
}