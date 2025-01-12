using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google;
using Isolaatti.Accounts.Service;
using Isolaatti.Audios.Data;
using Isolaatti.Audios.Dto;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Isolaatti.Audios.Service;

public class AudiosService(DbContextApp _db, GoogleCloudStorageService _storage, ILogger<AudiosService> logger)
{

    public async Task<Audio> CreateAudio(Stream file, int userId, long postId, string contentType, int duration)
    {
        var objectName = $"audios/{Guid.NewGuid()}";
        await _storage.CreateObject(file, contentType, objectName);

        var post = await _db.SimpleTextPosts.FindAsync(postId);

        if (post == null)
        {
            throw new PostNotFoundException(postId);
        }

        if (post.UserId != userId)
        {
            throw new UnauthorizedUserException(userId);
        }

        var audio = new Audio
        {
            UserId = userId,
            FirestoreObjectPath = objectName,
            DurationSeconds = duration
        };

        _db.Audios.Add(audio);

        post.AudioId = audio.Id;
        _db.SimpleTextPosts.Update(post);

        await _db.SaveChangesAsync();

        

        return audio;
    }
    
    public async Task<AudiosOperationResult> DeleteAudio(Guid id, int userId)
    {
        var audio = await _db.Audios.FindAsync(id);
        if (audio == null)
            return AudiosOperationResult.DoesNotExist;
        if (audio.UserId != userId)
            return AudiosOperationResult.NotOwned;

        try
        {
            await _storage.DeleteObject(audio.FirestoreObjectPath);
        }
        catch (GoogleApiException googleApiException)
        {
            logger.LogError("Error deleting object on Google Cloud Bucket. {}", googleApiException.Message);
        }

        _db.Audios.Remove(audio);

        await _db.SaveChangesAsync();

        return AudiosOperationResult.Success;
    }

    public async Task<Audio?> GetAudio(Guid audioId)
    {
        return await _db.Audios.FindAsync(audioId);
    }

    public void DeleteAllUserAudios(int userId)
    {
        _db.Audios.RemoveRange(_db.Audios.Where(a => a.UserId == userId));
    }
}