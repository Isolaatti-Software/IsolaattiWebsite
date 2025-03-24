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

    /// <summary>
    /// Creates a description audio for the user. Removes previously set audio if any.
    /// </summary>
    /// <param name="file">audio file</param>
    /// <param name="userId">user id that requests this</param>
    /// <param name="contentType">mime type of audio file</param>
    /// <param name="duration">Duration in seconds</param>
    /// <returns>Created audio</returns>
    public async Task<Audio> CreateAudioForDescription(Stream file, int userId, string contentType, int duration)
    {
        
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        var oldAudioId = user.DescriptionAudioId;
        
        var objectName = $"audios/{Guid.NewGuid()}";
        await _storage.CreateObject(file, contentType, objectName);
        
        var audio = new Audio
        {
            UserId = userId,
            FirestoreObjectPath = objectName,
            DurationSeconds = duration
        };
        
        _db.Audios.Add(audio);
        

        user.DescriptionAudioId = audio.Id;
        await _db.SaveChangesAsync();

        // remove old audio from storage and db

        if (oldAudioId != null)
        {

            var oldAudio = _db.Audios.FirstOrDefault(a => a.Id == oldAudioId);
            
            if (oldAudio != null)
            {
                _db.Audios.Remove(oldAudio);
                await _db.SaveChangesAsync();
                try
                {
                    await _storage.DeleteObject(oldAudio.FirestoreObjectPath);
                }
                catch (Exception e)
                {
                    logger.LogError("Failed to remove audio");
                }
            }
            
            
        }
        

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