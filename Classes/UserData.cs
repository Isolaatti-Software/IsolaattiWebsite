using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Models;

namespace isolaatti_API.Classes
{
    public class UserData
    {
        private readonly DbContextApp dbContext;
        private User user;
        public string name { get; }
        public string profilePhotoUrl { get; }
        public string emailAddress { get; }
        public Boolean badPassword { get; }
        public Boolean validatedEmail { get; }
        public int userId { get; }
        public UserData(int userId, DbContextApp dbContextApp)
        {
            dbContext = dbContextApp;
            user = dbContext.Users.Find(userId);
            name = user.Name;
            emailAddress = user.Email;
            profilePhotoUrl = ""; //fill this when implemented
            validatedEmail = user.EmailValidated;
            this.userId = userId;
        }
        public UserData(Boolean badPass)
        {
            if (badPass)
            {
                badPassword = true;
            }
            else
            {
                badPassword = false;
            }
            
        }
    }
}
