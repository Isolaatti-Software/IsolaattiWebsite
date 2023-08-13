namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class UserFeed
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ImageId { get; set; }
    public bool Following { get; set; }
}