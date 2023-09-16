using Isolaatti.Comments.Entity;

namespace Isolaatti.DTOs;

public class CommentDto
{
    public Comment Comment { get; set; }
    
    public string Username { get; set; }
}