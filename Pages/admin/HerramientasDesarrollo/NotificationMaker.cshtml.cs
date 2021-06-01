using isolaatti_API.Hubs;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Pages.admin.HerramientasDesarrollo
{
    public class NotificationMaker : PageModel
    {
        private readonly DbContextApp _db;
        private readonly IHubContext<NotificationsHub> _hubContext;
        public NotificationMaker(DbContextApp dbContextApp, IHubContext<NotificationsHub> hubContext)
        {
            _db = dbContextApp;
            _hubContext = hubContext;
        }
        
        public IActionResult OnGet()
        {
            var tokenOnCookie = Request.Cookies["isolaatti_admin_session"];
            if(tokenOnCookie == null) return RedirectToPage("LogIn");
            
            var adminAccounts = new AdminAccounts(_db);
            var user = adminAccounts.ValidateSessionToken(tokenOnCookie);
            if(user == null) return RedirectToPage("LogIn");
            
            // data binding here
            ViewData["username"] = user.name;

            return Page();
        }

        public IActionResult OnPost(int userId, string msg)
        {
            var sessionsId = NotificationsHub.Sessions[userId];
            foreach (var id in sessionsId)
            {
                _hubContext.Clients.Client(id)
                    .SendAsync("hola", msg);
            }
            return Page();
        }
    }
}