using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Models.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Isolaatti.Repositories;

public class ImagesRepository
{
    private readonly IMongoCollection<Image> _images;

    public ImagesRepository(MongoDatabase mongoDatabase)
    {
        _images = mongoDatabase.GetImagesCollection();
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

    public async Task<Image?> GetImage(string id)
    {
        return await _images.Find(i => i.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfUser(int userId, string? lastId, int pageSize)
    {
        var userFilter = Builders<Image>.Filter.Eq("UserId", userId);
        var antiSquadFilter = Builders<Image>.Filter.Eq("SquadId", BsonNull.Value);
        if (lastId == null)
        {
            return await _images.Find(userFilter & antiSquadFilter).SortByDescending(i => i.Id).Limit(pageSize).ToListAsync();
        }
        var pagingFilter = Builders<Image>.Filter.Lt(i => i.Id, lastId);

        return await _images.Find(userFilter & pagingFilter & antiSquadFilter).SortByDescending(i => i.Id).Limit(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetImagesOfSquad(Guid squadId, string? lastId, int pageSize)
    {
        var userFilter = Builders<Image>.Filter.Eq("SquadId", squadId);
        if (lastId == null)
        {
            return await _images.Find(userFilter).Limit(20).SortByDescending(i => i.Id).Limit(pageSize).ToListAsync();
        }
        var pagingFilter = Builders<Image>.Filter.Lt(i => i.Id, lastId);
        return await _images.Find(userFilter & pagingFilter).SortByDescending(i => i.Id).Limit(pageSize).ToListAsync();
    }

    public async Task DeleteImage(string id)
    {
        await _images.DeleteOneAsync(i => i.Id.Equals(id));
    }

    public async Task<IEnumerable<string>> DeleteImages(IEnumerable<string> imageIds, int userId)
    {
        var filter = Builders<Image>.Filter.In(i => i.Id, imageIds) & Builders<Image>.Filter.Eq(i => i.UserId, userId);

        var images = _images.Find(filter).ToList();

        await _images.DeleteManyAsync(filter);
        
        return images.Select(i => i.IdOnFirebase);
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

    // TODO take this const value to json config
    private const int OutstandingImageLimit = 10;
    public async Task<bool> SetImageOutstanding(string imageId, int userId, bool outstanding)
    {
        var outstandingImageCount = await _images.CountDocumentsAsync(i => i.UserId == userId && i.Outstanding);

        
        // Intention is to make this image outstanding but count limit has been reached
        if (outstandingImageCount > OutstandingImageLimit && outstanding)
        {
            return false;
        }
        
        var image = await _images.Find(i => i.Id == imageId).FirstOrDefaultAsync();
        
        // verify ownership and existence
        if (image == null || image.UserId != userId)
        {
            return false;
        }

        var updateDefinition = Builders<Image>.Update.Set(i => i.Outstanding, outstanding);

        var updateResult = await _images.UpdateOneAsync(i => i.Id == imageId, updateDefinition);
        return updateResult.IsAcknowledged;
    }

    public async Task<IEnumerable<Image>> GetUserOutstandingImages(int userId)
    {
        return await _images.Find(i => i.UserId == userId && i.Outstanding).ToListAsync();
    }
}