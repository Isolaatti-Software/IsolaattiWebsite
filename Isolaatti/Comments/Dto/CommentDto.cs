using Isolaatti.Comments.Entity;

namespace Isolaatti.Comments.Dto;

public class CommentDto
{
    public Comment Comment { get; set; }
    
    public string Username { get; set; }
}