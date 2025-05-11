using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.Images.Data;

namespace Isolaatti.Images.Repository;

public class ImagesRepository(DbContextApp db)
{

    public async Task<Image> InsertImage(int userId, string idOnFirebase, Guid? squadId, long? postId)
    {
        var image = new Image()
        {
            IdOnFirebase = idOnFirebase,
            UserId = userId,
            SquadId = squadId,
            PostId = postId
        };
        db.Images.Add(image);
        await db.SaveChangesAsync();
        return image;
    }

    public async Task<Image?> GetImage(Guid id)
    {
        return await db.Images.FindAsync(id);
    }
    

    public async Task DeleteImage(Guid id)
    {
        var image = await db.Images.FindAsync(id);
        if(image == null) return;
        db.Images.Remove(image);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> DeleteImages(IEnumerable<Guid> imageIds, int userId)
    {


        var images = db.Images.Where(i => i.UserId == userId && imageIds.Contains(i.Id)).ToList();
        
        db.Images.RemoveRange(images);
        await db.SaveChangesAsync();
        
        return images.Select(i => i.IdOnFirebase);
    }

    public async Task<IEnumerable<string>> DeleteAllImages(int userId)
    {
        var images = db.Images.Where(i => i.UserId == userId).ToList();
        
        db.Images.RemoveRange(images);

        return images.Select(i => i.IdOnFirebase);
    }
}