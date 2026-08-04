using System;
using System.IO;
using System.Threading.Tasks;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Enums;
using Isolaatti.Services;
using Microsoft.Extensions.Logging;

namespace Isolaatti.Audios.Service;

public class AudiosService(DbContextApp _db, ILogger<AudiosService> logger)
{

    public async Task CreateAudio(Stream file, int userId, long postId, string contentType, int duration)
    {
        var objectName = $"audios/{Guid.NewGuid()}";
        // await _storage.CreateObject(file, contentType, objectName);

        var post = await _db.SimpleTextPosts.FindAsync(postId);

        if (post == null)
        {
            throw new PostNotFoundException(postId);
        }

        if (post.UserId != userId)
        {
            throw new UnauthorizedUserException(userId);
        }

        // audio to delete
        if (post.AudioStoragePath != null)
        {
            try
            {
                //await _storage.DeleteObject(post.AudioStoragePath);
            }
            catch (Exception)
            {
                logger.LogError("Failed to delete audio {}", post.AudioStoragePath);
            }
        }

        post.AudioStoragePath = objectName;
        post.AudioDurationMs = duration;

        _db.SimpleTextPosts.Update(post);

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a description audio for the user. Removes previously set audio if any.
    /// </summary>
    /// <param name="file">audio file</param>
    /// <param name="userId">user id that requests this</param>
    /// <param name="contentType">mime type of audio file</param>
    /// <param name="duration">Duration in seconds</param>
    /// <returns>Created audio</returns>
    public async Task<AudiosOperationResult> CreateAudioForDescription(Stream file, int userId, string contentType, int duration)
    {
        
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        var oldAudioPath = user.AudioStoragePath;
        
        var objectName = $"audios/{Guid.NewGuid()}";
        //await _storage.CreateObject(file, contentType, objectName);

        user.AudioStoragePath = objectName;
        
        await _db.SaveChangesAsync();

        // remove old audio from storage

        if (oldAudioPath != null)
        {
            try
            {
                //await _storage.DeleteObject(oldAudioPath);
                return AudiosOperationResult.Success;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to remove audio");
                return AudiosOperationResult.RemovalFailed;
            }
        }

        return AudiosOperationResult.Success;
    }
    
    public async Task<AudiosOperationResult> DeleteAudio(string storagePath)
    {
        

        return AudiosOperationResult.Success;
    }
}