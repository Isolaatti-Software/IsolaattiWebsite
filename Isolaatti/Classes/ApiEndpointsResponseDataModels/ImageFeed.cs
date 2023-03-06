using Isolaatti.Models.MongoDB;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class ImageFeed : Image
{
    public ImageFeed(Image image)
    {
        Id = image.Id;
        Name = image.Name;
        UserId = image.UserId;
        IdOnFirebase = image.IdOnFirebase;
    }
    
    public string UserName { get; set; }
}