using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class ImagesRepository
{
    private readonly IMongoCollection<Image> _images;
    private readonly MongoDatabaseConfiguration _settings;

    public ImagesRepository(IOptions<MongoDatabaseConfiguration> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        var database = client.GetDatabase(_settings.DatabaseName);
        _images = database.GetCollection<Image>(_settings.ImagesCollectionName);
        _images.Indexes.CreateOne(new CreateIndexModel<Image>(Builders<Image>.IndexKeys.Text(i => i.Name)));
    }

    public async Task<Image> InsertImage(int userId, string name, string idOnFirebase, Guid? squadId)
    {
        var image = new Image()
        {
            IdOnFirebase = idOnFirebase,
            UserId = userId,
            Name = name,
            SquadId = squadId
        };
        await _images.InsertOneAsync(image);
        return image;
    }

    public async Task<Image> GetImage(string id)
    {
        return await _images.Find(i => i.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfUser(int userId, string? lastId)
    {
        var userFilter = Builders<Image>.Filter.Eq("UserId", userId);
        var antiSquadFilter = Builders<Image>.Filter.Eq("SquadId", BsonNull.Value);
        if (lastId == null)
        {
            return await _images.Find(userFilter & antiSquadFilter).Limit(20).ToListAsync();
        }
        var pagingFilter = Builders<Image>.Filter.Gt("id", lastId);

        return await _images.Find(userFilter & pagingFilter & antiSquadFilter).Limit(20).ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfSquad(Guid squadId, string lastId)
    {
        var userFilter = Builders<Image>.Filter.Eq("SquadId", squadId);
        if (lastId == null)
        {
            return await _images.Find(userFilter).Limit(20).ToListAsync();
        }
        var pagingFilter = Builders<Image>.Filter.Gt("id", lastId);
        return await _images.Find(userFilter & pagingFilter).Limit(20).ToListAsync();
    }

    public async Task DeleteImage(string id)
    {
        await _images.DeleteOneAsync(i => i.Id.Equals(id));
    }

    public async Task RenameImage(string id, string name)
    {
        var filter = Builders<Image>.Filter.Eq(image => image.Id, id);
        var update = Builders<Image>.Update.Set(image => image.Name, name);
        await _images.UpdateOneAsync(filter, update);
    }
    

    public async Task<List<Image>> SearchOnName(string query)
    {
        return await (await _images
            .FindAsync(Builders<Image>.Filter.Text(query, new TextSearchOptions { CaseSensitive = false })))
            .ToListAsync();
    }

}