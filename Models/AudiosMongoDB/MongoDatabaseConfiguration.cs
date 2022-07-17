namespace isolaatti_API.Models.AudiosMongoDB;

public class MongoDatabaseConfiguration
{
    public string AudiosCollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}