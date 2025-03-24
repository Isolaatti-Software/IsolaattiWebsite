using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Isolaatti.Services;

public class GoogleCloudStorageService
{
    private const string FirebaseProjectName = "isolaatti-b6641.appspot.com";
    private const string GoogleCloudCredentialPath = "isolaatti-firebase-adminsdk.json";
    private readonly StorageClient _storage;

    private readonly ILogger<GoogleCloudStorageService> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public GoogleCloudStorageService(ILogger<GoogleCloudStorageService> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
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
        var path = _webHostEnvironment.IsDevelopment() ? $"dev/{objectName}" : objectName;
        await _storage.UploadObjectAsync(FirebaseProjectName, path, contentType, fileContent);
    }

    public async Task DeleteObject(string objectName)
    {
        var path = _webHostEnvironment.IsDevelopment() ? $"dev/{objectName}" : objectName;
        await _storage.DeleteObjectAsync(FirebaseProjectName, path);
    }

    public async Task<string> GetSignedUrl(string objectName)
    {
        var urlSigner = UrlSigner.FromServiceAccountPath(GoogleCloudCredentialPath);
        var path = _webHostEnvironment.IsDevelopment() ? $"dev/{objectName}" : objectName;
        return await urlSigner.SignAsync(FirebaseProjectName, path, TimeSpan.FromHours(1), HttpMethod.Get);
    }

    public async Task<string> GetDownloadUrl(string objectName)
    {
        var path = _webHostEnvironment.IsDevelopment() ? $"dev/{objectName}" : objectName;
        var firestoreObject = await _storage.GetObjectAsync(FirebaseProjectName, path);
        return firestoreObject.MediaLink;
    }

    public async Task DeleteObjects(IEnumerable<string> objects)
    {
        foreach (var file in objects)
        {
            var path = _webHostEnvironment.IsDevelopment() ? $"dev/{file}" : file;
            try
            {
                await _storage.DeleteObjectAsync(FirebaseProjectName, path);
            }
            catch (GoogleApiException e)
            {
                _logger.LogError("Error deleting file {} from Google Cloud Storage Bucket with exception message {}", path, e.Message);
            }
        }
    }
}