using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Messenger;

[ApiController]
[Route("/api/messenger")]
public class MessengerController(DbContextApp db, IMessagingServer messagingServerApi) : IsolaattiController
{
    [IsolaattiAuth]
    [Route("information")]
    [HttpGet]
    public async Task<IActionResult> GetMessengerInformation()
    {
        if (User.MessengerId == null)
        {
            return NotFound(new
            {
                message = "Account not registered in messenger"
            });
        }

        var messengerAccount = await db.MessengerAccounts.FindAsync(User.MessengerId);
        if (messengerAccount == null)
        {
            return NotFound();
        }
        return Ok(messengerAccount);
    }

    [IsolaattiAuth]
    [Route("request_registration")]
    [HttpPost]
    public async Task<IActionResult> RequestRegistration()
    {
        var messengerAccounts = db.MessengerAccounts.Where(ma => ma.Id.Equals(User.MessengerId)).ToList();
        if (messengerAccounts.Count == 0)
        {
            var newMessengerAccountUsername = $"{Utils.RandomData.GenerateRandomString(4)}-{User.UniqueUsername}";
            var newMessengerAccount = new OpenfireAccount()
            {
                Id = newMessengerAccountUsername,
                Password = Utils.RandomData.GenerateRandomPassword()
            };
            User.MessengerId = newMessengerAccountUsername;
            db.Users.Update(User);
            db.MessengerAccounts.Add(newMessengerAccount);
            var result = await messagingServerApi.CreateUser(newMessengerAccountUsername, User.Name, User.Email, newMessengerAccount.Password);
            if (result)
            {
                await db.SaveChangesAsync();
            }
            else
            {
                return Problem("Could not create user on openfire");
            }

            
        }
        return Ok(new
        {
            
        });
    }
}