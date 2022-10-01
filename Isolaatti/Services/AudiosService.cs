using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Isolaatti.Services;

public class AudiosService
{
    private readonly GoogleCloudStorageService _storage;
    private readonly AudiosRepository _audiosRepository;
    private readonly IAccounts _accounts;
    
    public AudiosService(GoogleCloudStorageService googleFirebaseStorageService, AudiosRepository audiosRepository, IAccounts accounts)
    {
        _storage = googleFirebaseStorageService;
        _audiosRepository = audiosRepository;
        _accounts = accounts;
    }

    public async Task<Audio> CreateAudio(Stream file, int userId, string name, string contentType)
    {
        var objectName = $"audios/{Guid.NewGuid()}";
        await _storage.CreateObject(file, contentType, objectName);
        return await _audiosRepository.InsertAudio(userId, name, objectName);
    }

    public async Task<FeedAudio> GetAudio(string id)
    {
        var audio = await _audiosRepository.GetAudio(id);
        if (audio == null) return null;

        var feedAudio = new FeedAudio(audio);

        feedAudio.UserName = _accounts.GetUsernameFromId(feedAudio.UserId);

        return feedAudio;
    }


    public async Task<AudiosOperationResult> RenameAudio(string id, int userId, string name)
    {
        var audio = await _audiosRepository.GetAudio(id);
        if (audio == null)
            return AudiosOperationResult.DoesNotExist;
        if (audio.UserId != userId)
            return AudiosOperationResult.NotOwned;

        if (name.Length > 30)
        {
            name = name[..30];
        }
        
        await _audiosRepository.RenameAudio(id, name);
        return AudiosOperationResult.Success;
    }

    public async Task<AudiosOperationResult> DeleteAudio(string id, int userId)
    {
        var audio = await _audiosRepository.GetAudio(id);
        if (audio == null)
            return AudiosOperationResult.DoesNotExist;
        if (audio.UserId != userId)
            return AudiosOperationResult.NotOwned;

        await _storage.DeleteObject(audio.FirestoreObjectPath);
        await _audiosRepository.RemoveAudio(audio.Id);

        return AudiosOperationResult.Success;
    }
}