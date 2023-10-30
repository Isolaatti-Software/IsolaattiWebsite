using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Isolaatti.Config;

public class Client
{
    public const string SpecialPermissionSignUp = "sign-up";
    
    [Required(AllowEmptyStrings = false)]
    public string Id { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string Secret { get; set; }
    public List<string> SpecialPermissions { get; set; }
}