/* Class to generate JSON response when the android app wants to
   recover the preferences */
namespace isolaatti_API.Classes
{
    public class UserPreferences
    {
        public bool EmailNotifications { get; set; }
        public bool NotifyWhenProcessStarted { get; set; }
        public bool NotifyWhenProcessFinishes { get; set; }
    }
}