using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class AudiosRepository
{
    private readonly IMongoCollection<Audio> _audios;
    private readonly MongoDatabaseConfiguration _settings;
    private readonly DbContextApp _db;

    public AudiosRepository(IOptions<MongoDatabaseConfiguration> settings, DbContextApp db)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _audios = database.GetCollection<Audio>(_settings.AudiosCollectionName);
        _audios.Indexes.CreateOne(new CreateIndexModel<Audio>(Builders<Audio>.IndexKeys.Text(x => x.Name)));

        _db = db;
    }

#nullable enable

    public async Task<Audio?> GetAudio(string audioId)
    {
        return _audios.Find(a => a.Id == audioId).Limit(1).FirstOrDefault();
    }

#nullable disable

    public async Task<IEnumerable<FeedAudio>> GetAudiosOfUser(int userId, string lastAudioId = null)
    {
        List<Audio> audiosData;
        if (lastAudioId == null)
        {
            audiosData = await _audios.Find(a => a.UserId == userId).SortByDescending(a => a.Id).Limit(10).ToListAsync();
        }
        else
        {
            var filterLt = Builders<Audio>.Filter.Lt(doc => doc.Id, lastAudioId);
            var userFilter = Builders<Audio>.Filter.Eq(doc => doc.UserId, userId);
            audiosData = await _audios.Find(userFilter & filterLt).SortByDescending(a => a.Id).Limit(10).ToListAsync();
        }

        
        
        // This might not be optimal, but it is better than making one http request per audio to get its user name
        return audiosData.Select(audioData => new FeedAudio(audioData)
            { UserName = _db.Users.FirstOrDefault(u => u.Id == audioData.UserId)?.Name });

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

    public async Task<Audio> InsertAudio(int userId, string name, string firestorePath, int duration)
    {
        var audio = new Audio()
        {
            FirestoreObjectPath = firestorePath,
            UserId = userId,
            Name = name,
            CreationTime = DateTime.Now.ToUniversalTime(),
            DurationSeconds = duration
        };
        await _audios.InsertOneAsync(audio);
        return audio;
    }

    public async Task RemoveAudio(string audioId)
    {
        await _audios.DeleteOneAsync(a => a.Id == audioId);
    }

    public async Task RenameAudio(string audioId, string name)
    {
        var update = Builders<Audio>.Update.Set(audio => audio.Name, name);
        await _audios.UpdateOneAsync(a => audioId == a.Id,update);
    }

    public async Task<long> NumberOfAudios()
    {
        return await _audios.EstimatedDocumentCountAsync();
    }

    public async Task<List<Audio>> SearchByName(string query)
    {
        return await (await _audios.FindAsync(Builders<Audio>.Filter.Text(query))).ToListAsync();
    }
}