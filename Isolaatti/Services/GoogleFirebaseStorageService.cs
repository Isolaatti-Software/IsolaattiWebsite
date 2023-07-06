using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace Isolaatti.Services;

public class GoogleCloudStorageService
{
    private const string FirebaseProjectName = "isolaatti-b6641.appspot.com";
    private const string GoogleCloudCredentialPath = "isolaatti-firebase-adminsdk.json";
    private readonly StorageClient _storage;

    public GoogleCloudStorageService()
    {
        const string filePath = "isolaatti-firebase-adminsdk.json";
        GoogleCredential credential;
        if (File.Exists(filePath))
        {
            var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            credential = GoogleCredential.FromStream(file);
            file.Close();
        }
        else
        {
            var json = Environment.GetEnvironmentVariable("google_admin_sdk");
            if(json != null)
                credential = GoogleCredential.FromJson(json);
            else
            {
                throw new FileNotFoundException(
                    "Google credential file was not found. Tried to use env vars but did not have success.");
            }
        }
        var googleCloudCredential = credential;
        _storage = StorageClient.Create(googleCloudCredential);
    }

    public async Task CreateObject(Stream fileContent, string contentType, string objectName)
    {
        await _storage.UploadObjectAsync(FirebaseProjectName, objectName, contentType, fileContent);
    }

    public async Task DeleteObject(string objectName)
    {
        await _storage.DeleteObjectAsync(FirebaseProjectName, objectName);
    }

    public async Task<string> GetSignedUrl(string objectName)
    {
        var urlSigner = UrlSigner.FromServiceAccountPath(GoogleCloudCredentialPath);
        return await urlSigner.SignAsync(FirebaseProjectName, objectName, TimeSpan.FromHours(1), HttpMethod.Get);
    }

    public async Task<string> GetDownloadUrl(string objectName)
    {
        var firestoreObject = await _storage.GetObjectAsync(FirebaseProjectName, objectName);
        return firestoreObject.MediaLink;
    }
}