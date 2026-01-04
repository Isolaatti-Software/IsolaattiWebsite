namespace Isolaatti.Config;

public class Keycloak
{
    public string Realm { get; set; }
    public string AdminApi { get; set; }
    public string Authority { get; set; }
    public string MetadataAddress { get; set; }
    public string ClientId { get; set; }
    public string Audience { get; set; }
    public string AdminClientId { get; set; }
    public string AdminClientSecret { get; set; }
}