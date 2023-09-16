using System.ComponentModel.DataAnnotations.Schema;

namespace Isolaatti.DTOs;

public class FollowDto
{
    public bool FollowingThisUser { get; set; }
    public bool ThisUserIsFollowingMe { get; set; }
}
