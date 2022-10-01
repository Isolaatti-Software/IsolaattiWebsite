using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Models.MongoDB;
using Microsoft.Extensions.Options;
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
    }

    public async Task<Image> GetImage(string id)
    {
        return await _images.Find(i => i.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfUser(int userId, string lastId)
    {
        var userFilter = Builders<Image>.Filter.Eq("UserId", userId);
        var pagingFilter = Builders<Image>.Filter.Gt("id", lastId);

        return await _images.Find(userFilter & pagingFilter).Limit(20).ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfSquad(Guid squadId, string lastId)
    {
        var userFilter = Builders<Image>.Filter.Eq("SquadId", squadId);
        var pagingFilter = Builders<Image>.Filter.Gt("id", lastId);
        
        return await _images.Find(userFilter & pagingFilter).Limit(20).ToListAsync();
    }

    public async Task DeleteImage(string id)
    {
        await _images.DeleteOneAsync(i => i.Id.Equals(id));
    }

    public async Task DeleteAllImagesOfUser()
    {
        
    }
    
    public async Task UnrelateAllImagesOfSquad(Guid squadId)
    {
        
    }

    public async Task SetSquadOfImage(string id, Guid squadId)
    {
        
    }

    public async Task SetNullSquadOfImage(string id)
    {
        
    }

}