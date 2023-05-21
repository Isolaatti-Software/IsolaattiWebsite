using System.Collections.Generic;

namespace Isolaatti.DTOs;

public class PermissionDto
{
    public const string ModifySquadName = "modify_name";
    public const string ModifyDescription = "modify_description";
    public const string ModifyAudio = "modify_audio";
    public const string ModifyImage = "modify_image";
    public const string RemoveMembers = "remove_members";
    public const string PromoteToAdmin = "promote_to_admin";
    public const string RemoveContent = "remove_posts";
    public const string ChangeAdminsPermissions = "change_admins_permissions";

    public static readonly List<string> Permissions = new()
    {
        ModifySquadName, ModifyDescription, ModifyAudio, ModifyImage,RemoveMembers, PromoteToAdmin, RemoveContent, ChangeAdminsPermissions
    };

    public bool Validate()
    {
        return  PermissionsList.TrueForAll(permission => Permissions.Exists(p => permission == p));
    }

    
    public List<string> PermissionsList { get; set; }
}