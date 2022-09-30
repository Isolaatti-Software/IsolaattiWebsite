namespace Isolaatti.Models.MongoDB;

public class MongoDatabaseConfiguration
{
    public string AudiosCollectionName { get; set; }
    public string NotificationsCollectionName { get; set; }
    public string SquadsInvitationsCollectionName { get; set; }
    public string SquadsJoinRequestsCollectionName { get; set; }
    public string RealtimeServiceKeysCollectionName { get; set; }
    public string AuthTokensCollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}