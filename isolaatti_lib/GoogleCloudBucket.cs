using System;
using System.Diagnostics;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace isolaatti_API.isolaatti_lib
{
    public class GoogleCloudBucket
    {
        private readonly StorageClient _storage;
        private static GoogleCloudBucket _instance;
        private GoogleCloudBucket()
        {
            var file = File.Open("isolaatti-firebase-adminsdk.json", FileMode.Open);
            var credential = GoogleCredential.FromStream(file);
            _storage = StorageClient.Create(credential);
        }

        public static GoogleCloudBucket GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GoogleCloudBucket();
            }

            return _instance;
        }

        public void DeleteFile(string fileRef)
        {
            try
            {
                _storage.DeleteObject("isolaatti-b6641.appspot.com", fileRef);
            }
            catch (Google.GoogleApiException)
            {
                Debug.Write("Couldn't delete file, it doesn't exist");
            }
        }
    }
}