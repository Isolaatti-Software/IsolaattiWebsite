using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Isolaatti.IdentityService;

public class UserRepresentation
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Enabled { get; set; }
    public bool EmailVerified { get; set; }
    public List<Credential> Credentials { get; set; } = [];

}