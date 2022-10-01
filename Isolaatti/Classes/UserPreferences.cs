/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

//Class to generate JSON response when the android app wants to
//recover the preferences

namespace Isolaatti.Classes
{
    public class UserPreferences
    {
        public bool EmailNotifications { get; set; }
        public string ProfileHtmlColor { get; set; }
    }
}