/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
namespace isolaatti_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Uid { get; set; }
        public bool EmailValidated { get; set; }
        public string GoogleToken { get; set; }
        
        
        // fields for user preferences
        public bool NotifyByEmail { get; set; }
        public bool NotifyWhenProcessStarted { get; set; }
        public bool NotifyWhenProcessFinishes { get; set; }
        
        // language field is used to decide what language to use in notifications by E-mail
        public string AppLanguage { get; set; }
        
        // here store people (followers and following)
        public string FollowersIdsJson { get; set; }
        public string FollowingIdsJson { get; set; }
        public int NumberOfFollowers { get; set; }
        public int NumberOfFollowing { get; set; }
        
        public byte[] ProfileImageData { get; set; }
        public string DescriptionText { get; set; }
        public string DescriptionAudioUrl { get; set; }
    }
}