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
    }
}