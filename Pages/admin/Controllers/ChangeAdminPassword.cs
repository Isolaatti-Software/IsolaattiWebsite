using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Pages.admin.Controllers
{
    [Route("/adminControllers/[controller]")]
    public class ChangeAdminPassword : Controller
    {
        private readonly DbContextApp _db;

        public ChangeAdminPassword(DbContextApp dbContext)
        {
            _db = dbContext;
        }
        
        [HttpPost]
        public IActionResult Index([FromForm] string currentPassword, 
            [FromForm] string newPassword, [FromForm] string newPasswordConfirm)
        {
            var sessionTokenFromRequest = Request.Cookies["isolaatti_admin_session"];
            if (sessionTokenFromRequest == null) return Unauthorized("Token is not present");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(sessionTokenFromRequest);
            if (user == null) return Unauthorized("Token is invalid");
            
            // verify if current password is correct
            var passwordHasher = new PasswordHasher<string>();
            var verificationResult = passwordHasher
                .VerifyHashedPassword(user.name, user.password, currentPassword);

            if (verificationResult == PasswordVerificationResult.Failed)
                return RedirectToPage("/admin/AccountSettings", new
                {
                    status = AdminAccounts.StatusCurrentPasswordIsIncorrect
                });
            
            // verify if both provided password are equal (confirmation)
            if (newPassword != newPasswordConfirm)
                return RedirectToPage("/admin/AccountSettings",new
                {
                    status = AdminAccounts.StatusNewPasswordVerificationFailed
                });
            
            // now I can change the password
            var newHashedPassword = passwordHasher.HashPassword(user.name, newPassword);
            user.password = newHashedPassword;

            _db.AdminAccounts.Update(user);
            _db.SaveChanges();
            
            return RedirectToPage("/admin/AccountSettings", new
            {
                status = AdminAccounts.StatusPasswordChangedSuccess
            });
        }
    }
}