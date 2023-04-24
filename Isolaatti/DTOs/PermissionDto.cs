using System.Collections.Generic;

namespace Isolaatti.DTOs;

public class PermissionDto
{
    public const string ModifySquadName = "modify_name";
    public const string ModifyDescription = "modify_description";
    public const string ModifyAudio = "modify_audio";
    public const string ModifyImage = "modify_image";

    public static readonly List<string> Permissions = new()
    {
        ModifySquadName, ModifyDescription, ModifyAudio, ModifyImage
    };

    public bool Validate()
    {
        return  PermissionsList.TrueForAll(permission => Permissions.Exists(p => permission == p));
    }

    
    public List<string> PermissionsList { get; set; }
}