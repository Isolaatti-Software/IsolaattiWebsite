using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Isolaatti.Notifications.Entity
{
    public abstract class NotificationPayload
    {
        public string Type { get; set; }
    }

    public class LikeNotificationPayload : NotificationPayload
    {
        public LikeNotificationPayload()
        {
            Type = GetType().Name;
        }
        public long PostId { get; set; }
        public int MakerUserId { get; set; }
    }

    public class FollowerNotificationPayload : NotificationPayload
    {
        public FollowerNotificationPayload()
        {
            Type = GetType().Name;
        }

        public int NewFollowerUserId { get; set; }
    }

    public class InformativeMessageNotificationPayload : NotificationPayload
    {

        public InformativeMessageNotificationPayload()
        {
            Type = GetType().Name;
        }

        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Text { get; set; }
    }

    public class NewActivityOnPost : NotificationPayload
    {
        public NewActivityOnPost()
        {
            Type = GetType().Name;
        }
    }

    public class NewActivityOfUser : NotificationPayload
    {
        public NewActivityOfUser()
        {
            Type = GetType().Name;
        }

        public long PostId { get; set; }

    }

    public class NewSquadJoinRequest : NotificationPayload
    {
        public NewSquadJoinRequest()
        {
            Type = GetType().Name;
        }

        public string JoinRequestId { get; set; }

        [NotMapped]
        public string Who { get; set; }
        [NotMapped]
        public string SquadName { get; set; }

    }

    public class NewSquadInvitation : NotificationPayload
    {
        public NewSquadInvitation()
        {
            Type = GetType().Name;
        }

        public string InvitationId { get; set; }

        [NotMapped]
        public string Who { get; set; }
        [NotMapped]
        public string SquadName { get; set; }

    }

    public class NewActivityOnSquad : NotificationPayload
    {
        public NewActivityOnSquad()
        {
            Type = GetType().Name;
        }

        public Guid SquadId { get; set; }
        public long PostId { get; set; }

        [NotMapped]
        public string Username { get; set; }

    }
}
