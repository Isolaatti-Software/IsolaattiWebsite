using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Isolaatti.Accounts.Service;
using Isolaatti.Enums;
using Isolaatti.Images.Data;
using Isolaatti.Images.Repository;
using Isolaatti.Services;
using Isolaatti.Users.Repository;
using Image = Isolaatti.Images.Data.Image;

namespace Isolaatti.Images.Service;

public class ImagesService
{
    private readonly ImagesRepository _imagesRepository;
    private readonly IAccountsService _accounts;
    private readonly UsersRepository _usersRepository;

    public ImagesService(ImagesRepository imagesRepository,
        IAccountsService accounts, UsersRepository usersRepository)
    {
        _imagesRepository = imagesRepository;
        _accounts = accounts;
        _usersRepository = usersRepository;
    }

    public async Task<Image> CreateImage(Stream file, int userId, Guid? squadId, long? postId)
    {
        var imageGuid = Guid.NewGuid();
        var originalImageObjectName = $"images/{imageGuid}/original";
        var reducedImageObjectName = $"images/{imageGuid}/reduced";
        var smallImageObjectName = $"images/{imageGuid}/small";
        
        // TODO: subir imagen original a bucket y mandar procesar la imagen al worker.

        throw new NotImplementedException();
    }

    public async Task<ImageFeed?> GetImage(Guid imageId)
    {
        var image = await _imagesRepository.GetImage(imageId);
        if (image == null) return null;

        var imageFeed = new ImageFeed(image);
        imageFeed.Username = _accounts.GetUsernameFromId(imageFeed.UserId);

        return imageFeed;
    }

    public async Task<string?> GetImageDownloadUrl(Guid imageId, string mode = "original")
    {
        return null;
    }

    public async Task<ImageModificationResult> DeleteImage(Guid id, int userId)
    {
        var image = await _imagesRepository.GetImage(id);
        if (image == null)
        {
            return ImageModificationResult.NotFound;
        }
        if (image.UserId != userId)
        {
            return ImageModificationResult.NotOwned;
        }
        

        await _imagesRepository.DeleteImage(id);

        return ImageModificationResult.Success;

    }

    public async Task DeleteImages(IEnumerable<Guid> imageIds, int userId)
    {

        
    }

    public async Task DeleteUserImages(int userId)
    {
        
    }
}