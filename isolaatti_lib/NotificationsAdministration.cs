using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using isolaatti_API.Classes.NotificationsData;
using isolaatti_API.Models;

/*
 * These methods are only for storing notifications and should be called before message to
 * * SignalR Hub is sent. For now I only have three types of notification.
 * * Notifications for Music Demixer should be put separately (I am not working on this yet)
 */

namespace isolaatti_API.isolaatti_lib
{
    public class NotificationsAdministration
    {
        private DbContextApp _db;
        
        public NotificationsAdministration(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        private const int TypeLikes = 1;
        private const int TypePosts = 2;
        private const int TypeNewFollower = 3;

        public void NewLikesActivityNotification(int userToNotifyId, int userWhoLikedId, long postId, long numberOfLikes)
        {
            if (userToNotifyId == userWhoLikedId) return;
            
            var shouldCreateNewNotification =
                !_db.SocialNotifications.Any(
                    notification => notification.PostRelated.Equals(postId) && 
                                    notification.Type == TypeLikes && 
                                    notification.UserId == userToNotifyId);

            if (shouldCreateNewNotification)
            {
                var data = new LikeData()
                {
                    AuthorsIds = new List<int>() { userWhoLikedId },
                    NumberOfLikes = numberOfLikes,
                    PostId = postId
                };
                var newNotification = new SocialNotification()
                {
                    PostRelated = postId,
                    TimeSpan = DateTime.Now,
                    Read = false,
                    Type = TypeLikes,
                    UserId = userToNotifyId,
                    DataJson = JsonSerializer.Serialize(data)
                };

                _db.SocialNotifications.Add(newNotification);
            }
            else
            {
                var existingNotification = _db.SocialNotifications.Single(
                    notification => notification.Type == TypeLikes && 
                                    notification.PostRelated.Equals(postId) &&
                                    notification.UserId == userToNotifyId);

                var data = JsonSerializer.Deserialize<LikeData>(existingNotification.DataJson);
                
                data.AuthorsIds.Add(userWhoLikedId);
                data.NumberOfLikes = numberOfLikes;

                existingNotification.DataJson = JsonSerializer.Serialize(data);
                
                // this will make the notification new again
                existingNotification.TimeSpan = DateTime.Now; 
                existingNotification.Read = false;

                _db.SocialNotifications.Update(existingNotification);
            }

            _db.SaveChanges();
        }

        public void NewCommentsActivityNotification(int userToNotifyId, int lastUserWhoCommentedId, long postId, int numberOfComments)
        {
            if (userToNotifyId == lastUserWhoCommentedId) return;
            
            var shouldCreateNewNotification =
                !_db.SocialNotifications.Any(
                    notification => notification.PostRelated.Equals(postId) && 
                                    notification.Type == TypePosts &&
                                    notification.UserId == userToNotifyId
                                    );

            if (shouldCreateNewNotification)
            {
                var notificationData = new PostData()
                {
                    PostId = postId,
                    AuthorsIds = new List<int>()
                    {
                        lastUserWhoCommentedId
                    },
                    NumberOfComments = numberOfComments
                };

                var newNotification = new SocialNotification()
                {
                    PostRelated = postId,
                    DataJson = JsonSerializer.Serialize(notificationData),
                    Read = false,
                    TimeSpan = DateTime.Now,
                    Type = TypePosts,
                    UserId = userToNotifyId
                };

                _db.SocialNotifications.Add(newNotification);

            }
            else
            {
                var existingNotification = _db.SocialNotifications.Single(
                    notification => notification.UserId == userToNotifyId &&
                                    notification.PostRelated == postId &&
                                    notification.Type == TypePosts);

                var data = JsonSerializer.Deserialize<PostData>(existingNotification.DataJson);
                data.AuthorsIds.Add(lastUserWhoCommentedId);
                data.NumberOfComments = numberOfComments;
                
                existingNotification.TimeSpan = DateTime.Now;
                existingNotification.Read = false;

                existingNotification.DataJson = JsonSerializer.Serialize(data);
                
                _db.SocialNotifications.Update(existingNotification);

            }

            _db.SaveChanges();
        }

        public void CreateNewFollowerNotification(int userWhoFollows, int userWhoIsFollowed)
        {
            // TODO: is it needed to limit how many times notifications about following are sent?
            // This is to avoid spam, that can be done by following and unfollowing multiple times, which would
            // cause multiple notifications to be sent and stored.
            
            // This code can be used to decide to notify only if last notification about the same user following was
            // not 30 minutes or less ago. But this can cause issues, as json data must be deserialized for every record
            // and that can take much time. 
            
            // var shouldNotify = _db.SocialNotifications.Any(notification =>
            //     notification.UserId == userWhoIsFollowed &&
            //     DateTime.Now.Subtract(notification.TimeSpan).Minutes >= 30);
            
            
            
            // For now, a notification is created no matter how much time has passed
            
            var data = new NewFollowerData()
            {
                NewFollowerId = userWhoFollows
            };
            
            var newNotification = new SocialNotification()
            {
                PostRelated = Convert.ToInt64(userWhoFollows), // in this case it is not a post but a user
                Read = false,
                Type = TypeNewFollower,
                UserId = userWhoIsFollowed,
                DataJson = JsonSerializer.Serialize(data)
            };

            _db.SocialNotifications.Add(newNotification);
        }
    }
}