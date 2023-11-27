using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google;
using Isolaatti.Accounts;
using Isolaatti.Accounts.Service;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Enums;
using Isolaatti.Repositories;
using Isolaatti.Users.Repository;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = Isolaatti.Models.MongoDB.Image;

namespace Isolaatti.Services;

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

    public async Task<Image> CreateImage(Stream file, int userId, string name, Guid? squadId)
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
        return await _imagesRepository.InsertImage(userId, name, imageGuid.ToString(), squadId);
    }

    public async Task<ImageFeed> GetImage(string imageId)
    {
        var image = await _imagesRepository.GetImage(imageId);
        if (image == null) return null;

        var imageFeed = new ImageFeed(image);
        imageFeed.UserName = _accounts.GetUsernameFromId(imageFeed.UserId);

        return imageFeed;
    }

    public async Task<string> GetImageDownloadUrl(string imageId, string mode = "original")
    {
        var image = await _imagesRepository.GetImage(imageId);
        if (image == null) return null;

        if (mode.Equals("original") || mode.Equals("reduced") || mode.Equals("small"))
            return await _storage.GetDownloadUrl($"images/{image.IdOnFirebase}/{mode}");
        return null;
    }

    public async Task<ImageModificationResult> DeleteImage(string id, int userId)
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

    public async Task<IEnumerable<Image>> GetImagesOfUser(int userId, string? lastId, int pageSize)
    {
        var images =  await _imagesRepository.GetImagesOfUser(userId, lastId, pageSize);
        return images.Select(i =>
        {
            i.Username = _usersRepository.GetUsernameById(i.UserId);
            return i;
        });
    }

    public async Task<ImageModificationResult> RenameImage(string imageId, int userId, string newName)
    {
        var image = await _imagesRepository.GetImage(imageId);
        if (image == null)
        {
            return ImageModificationResult.NotFound;
        }
        if (image.UserId != userId)
        {
            return ImageModificationResult.NotOwned;
        }

        await _imagesRepository.RenameImage(imageId, newName);

        return ImageModificationResult.Success;
    }
}