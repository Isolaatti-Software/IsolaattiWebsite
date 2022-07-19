using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Models.AudiosMongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class AudiosRepository
{
    private readonly IMongoCollection<Audio> _audios;
    private readonly MongoDatabaseConfiguration _settings;

    public AudiosRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _audios = database.GetCollection<Audio>(_settings.AudiosCollectionName);
    }

#nullable enable

    public async Task<Audio?> GetAudio(string audioId)
    {
        return _audios.Find(a => a.Id == audioId).Limit(1).FirstOrDefault();
    }

#nullable disable

    public async Task<List<Audio>> GetAudiosOfUser(int userId, string lastAudioId = null)
    {
        if (lastAudioId == null)
        {
            return await _audios.Find(a => a.UserId == userId).Limit(10).ToListAsync();
        }

        return await _audios.Find(a =>
            a.UserId == userId && new ObjectId(a.Id) > new ObjectId(lastAudioId)).Limit(10).ToListAsync();
    }

    public async Task<List<Audio>> GetGlobalFeed(string lastAudioId = null)
    {
        if (lastAudioId == null)
        {
            return await _audios.Find(a => true).Limit(50).ToListAsync();
        }

        var filter = Builders<Audio>.Filter.Gt("id", ObjectId.Parse(lastAudioId));
        return await _audios.Find(filter).Limit(50).ToListAsync();
    }

    public async Task<Audio> InsertAudio(int userId, string name, string firestorePath)
    {
        var audio = new Audio()
        {
            FirestoreObjectPath = firestorePath,
            UserId = userId,
            Name = name,
            CreationTime = DateTime.Now.ToUniversalTime()
        };
        await _audios.InsertOneAsync(audio);
        return audio;
    }

    public async Task RemoveAudio(string audioId)
    {
        await _audios.DeleteOneAsync(a => new ObjectId(a.Id) == new ObjectId(audioId));
    }

    public async Task RenameAudio(string audioId, string name)
    {
        await _audios.UpdateOneAsync(a => new ObjectId(audioId) == new ObjectId(a.Id),
            new ObjectUpdateDefinition<Audio>(new { name }));
    }

    public async Task<long> NumberOfAudios()
    {
        return await _audios.EstimatedDocumentCountAsync();
    }
}