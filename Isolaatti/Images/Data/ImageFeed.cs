namespace Isolaatti.Images.Data;

public class ImageFeed : Image
{
    public ImageFeed(Image image)
    {
        Id = image.Id;
        Name = image.Name;
        UserId = image.UserId;
        IdOnFirebase = image.IdOnFirebase;
    }
}