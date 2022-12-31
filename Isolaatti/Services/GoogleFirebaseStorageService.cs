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
        var file = File.Open(GoogleCloudCredentialPath, FileMode.Open, FileAccess.Read,
            FileShare.Read);
        var googleCloudCredential = GoogleCredential.FromStream(file);
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