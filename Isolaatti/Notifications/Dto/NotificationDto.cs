using Isolaatti.Notifications.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using static Isolaatti.Notifications.Entity.Notification;

namespace Isolaatti.Notifications.Dto
{
    public class NotificationDto
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string ImageId { get; set; }
        public bool Read { get; set; }

        public object Payload { get; set; }

        public class PayloadDtoBase
        {
            public string Type { get; set; }
        }

        public class LikePayloadDto : PayloadDtoBase
        {
            public long PostId { get; set; }
            public int MakerUserId { get; set; }
            public string MakerUserName { get; set; }
        }

        public class FollowerPayloadDto : PayloadDtoBase
        {
            public int NewFollowerUserId { get; set; }

            public string NewFollowerName { get; set; }
        }

        public class InformativePayloadDto : PayloadDtoBase
        {
            public string Url { get; set; }
            public string ImageUrl { get; set; }
            public string Text { get; set; }
        }

        public class NewActivityOnPostDto : PayloadDtoBase
        {
            // TODO
        }

        public class NewActivityOfUserDto : PayloadDtoBase
        {
            public long PostId { get; set; }
            public string UserName { get; set; }
            public string ImageId { get; set; }
        }

        public class NewSquadJoinRequestDto : PayloadDtoBase
        {
            public string JoinRequestId { get; set; }
            public string Who { get; set; }
            public string UserImageId { get; set; }
            public string SquadName { get; set; }
            public string SquadPhotoId { get; set; }
        }

        public class NewSquadInvitationDto : PayloadDtoBase
        {
            public string InvitationId { get; set; }

            public string Who { get; set; }
            public string UserImageId { get; set; }
            public string SquadName { get; set; }
            public string SquadPhotoId { get; set; }
        }

        public class NewActivityOnSquadDto : PayloadDtoBase
        {
            public Guid SquadId { get; set; }
            public long PostId { get; set; }

            public string Username { get; set; }
            public string UserPhotoId { get; set; }
        }

    }
}
