namespace Isolaatti.Images.Data;

public class ImageFeed : Image
{
    public ImageFeed(Image image)
    {
        Id = image.Id;
        UserId = image.UserId;
        IdOnFirebase = image.IdOnFirebase;
    }
}