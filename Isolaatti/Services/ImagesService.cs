using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Repositories;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Image = Isolaatti.Models.MongoDB.Image;

namespace Isolaatti.Services;

public class ImagesService
{
    private readonly GoogleCloudStorageService _storage;
    private readonly ImagesRepository _imagesRepository;
    private readonly IAccounts _accounts;

    public ImagesService(GoogleCloudStorageService googleCloudStorageService, ImagesRepository imagesRepository,
        IAccounts accounts)
    {
        _storage = googleCloudStorageService;
        _imagesRepository = imagesRepository;
        _accounts = accounts;
    }

    public async Task<Image> CreateImage(Stream file, int userId, string name, Guid? squadId)
    {
        var imageGuid = Guid.NewGuid();
        var originalImageObjectName = $"images/{imageGuid}/original";
        var reducedImageObjectName = $"images/{imageGuid}/reduced";
        var smallImageObjectName = $"images/{imageGuid}/small";

        // Create original image. No resize is performed, but only converted to webp format
        var originalImageConvertedToWebpStream = new MemoryStream();
        var originalImage = await SixLabors.ImageSharp.Image.LoadAsync(file);
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
        await _storage.CreateObject(file, "image/webp", originalImageObjectName);
        // Upload small image object
        await _storage.CreateObject(smallImageStream, "image/webp", smallImageObjectName);
        // Upload reduced image object
        await _storage.CreateObject(reducedImageStream, "image/webp", reducedImageObjectName);
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
            return await _storage.GetSignedUrl($"images/{image.IdOnFirebase}/{mode}");
        return null;
    }

    public async Task<IEnumerable<Image>> GetImagesOfUser(int userId, string lastId)
    {
        return await _imagesRepository.GetImagesOfUser(userId, lastId);
    }
}