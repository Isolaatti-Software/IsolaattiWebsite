using System;

namespace Isolaatti.RealtimeInteraction.Dto;

public enum EventType
{
    /// <summary>
    /// When the following occurs: content of post is updated, post is liked
    /// </summary>
    PostUpdate,
    /// <summary>
    /// When a comment is removed
    /// </summary>
    CommentAdded,
    /// <summary>
    /// When a comment is removed from the discussion
    /// </summary>
    CommentRemoved,
    /// <summary>
    /// Content of comment was modified and should be re rendered
    /// </summary>
    CommentModified,
    /// <summary>
    /// When a notification was inserted. This is not used to sent notifications using Firebase Cloud Messaging
    /// </summary>
    NotificationSent
}

/// <summary>
/// Represents a realtime event that is sent through the web sockets service.
/// T is the type of the related id. Example: a post with a int Id
/// </summary>
public class RealtimeEventDto<T>
{

    /// <summary>
    /// An optional object that can be sent with the event. This can be, for example, the item that will be displayed.
    /// If null, client should resolve the content somehow else
    /// </summary>
    public object Payload { get; set; }
    /// <summary>
    /// An Guid that is generated server side an then sent on each request through a header. This is then sent
    /// with each event and read client side. This is primarily used to conditionally make requests. For example,
    /// a client might want to request updated post after a modification event, but only when that modification
    /// was made in a client different from itself.
    /// </summary>
    public Guid ClientId { get; set; }
    /// <summary>
    /// Id of the related entity. This can be the id of the post that was modified or that the comment was made to.
    /// </summary>
    public T RelatedId { get; set; }
    /// <summary>
    /// The type of the event
    /// </summary>
    public EventType Type { get; set; }
}