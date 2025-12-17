using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Isolaatti.Comments.Entity;

namespace Isolaatti.Comments.Dto;

public class CommentDto
{
    public long Id { get; set; }
    public string TextContent { get; set; }
    
    public int UserId { get; set; }
    public long PostId { get; set; }
    
    public DateTime Date { get; set; }
    public Guid? SquadId { get; set; }
    
    // Response in the same discussion
    public long? ResponseForCommentId { get; set; }
    public bool Modified { get; set; }
    public bool IsFromUser { get; set; }
    public string Username { get; set; }
    public int RepliesCount { get; set; }
}