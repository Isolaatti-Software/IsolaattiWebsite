using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google;
using Isolaatti.Accounts.Service;
using Isolaatti.Enums;
using Isolaatti.Images.Data;
using Isolaatti.Images.Repository;
using Isolaatti.Services;
using Isolaatti.Users.Repository;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = Isolaatti.Images.Data.Image;

namespace Isolaatti.Images.Service;

public class ImagesService
{
    private readonly GoogleCloudStorageService _storage;
    private readonly ImagesRepository _imagesRepository;
    private readonly IAccountsService _accounts;
    private readonly UsersRepository _usersRepository;

    public ImagesService(GoogleCloudStorageService googleCloudStorageService, ImagesRepository imagesRepository,
        IAccountsService accounts, UsersRepository usersRepository)
    {
        _storage = googleCloudStorageService;
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

        // Create original image. No resize is performed, but only converted to webp format
        var originalImageConvertedToWebpStream = new MemoryStream();
        using var originalImage = await SixLabors.ImageSharp.Image.LoadAsync(file);
        await file.DisposeAsync();
        using (var img = originalImage.Clone(context => context.Resize(context.GetCurrentSize())))
        {
            await img.SaveAsWebpAsync(originalImageConvertedToWebpStream);
        }


        // Create small image
        var smallImageStream = new MemoryStream();
        using (var img = originalImage.Clone(context => 
                   context.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(120, 120) })))
        {
            await img.SaveAsWebpAsync(smallImageStream);
        }

        // Create reduced image
        var reducedImageStream = new MemoryStream();
        using (var img = originalImage.Clone(context =>
                   context.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(512, 512) })))
        {
            await img.SaveAsWebpAsync(reducedImageStream);
        }

        // Upload original image object
        await _storage.CreateObject(originalImageConvertedToWebpStream, "image/webp", originalImageObjectName);
        await originalImageConvertedToWebpStream.DisposeAsync();
        // Upload small image object
        await _storage.CreateObject(smallImageStream, "image/webp", smallImageObjectName);
        await smallImageStream.DisposeAsync();
        // Upload reduced image object
        await _storage.CreateObject(reducedImageStream, "image/webp", reducedImageObjectName);
        await reducedImageStream.DisposeAsync();
        return await _imagesRepository.InsertImage(userId, imageGuid.ToString(), squadId, postId);
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
        var image = await _imagesRepository.GetImage(imageId);
        if (image == null) return null;

        if (mode.Equals("original") || mode.Equals("reduced") || mode.Equals("small"))
            return await _storage.GetDownloadUrl($"images/{image.IdOnFirebase}/{mode}");
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

        try
        {
            await _storage.DeleteObject(image.IdOnFirebase);
        }
        catch (GoogleApiException)
        {
            
        }

        await _imagesRepository.DeleteImage(id);

        return ImageModificationResult.Success;

    }

    public async Task DeleteImages(IEnumerable<Guid> imageIds, int userId)
    {
        var filesToRemove = await _imagesRepository.DeleteImages(imageIds, userId);
        foreach (var id in filesToRemove)
        {
            await _storage.DeleteObject($"images/{id}/original");
            await _storage.DeleteObject($"images/{id}/reduced");
            await _storage.DeleteObject($"images/{id}/small");
        }
        
    }

    public async Task DeleteUserImages(int userId)
    {
        var fileToRemove = await _imagesRepository.DeleteAllImages(userId);

        foreach (var id in fileToRemove)
        {
            await _storage.DeleteObject($"images/{id}/original");
            await _storage.DeleteObject($"images/{id}/reduced");
            await _storage.DeleteObject($"images/{id}/small");
        }
    }
}