using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Isolaatti.Messenger;

[ApiController]
[Route("/api/messenger")]
public class MessengerController(DbContextApp db, IMessagingServer messagingServerApi, IOptions<MessengerConfig> messengerConfig) : IsolaattiController
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
                message = "Account not registered in messenger. Send post to request_registration"
            });
        }

        var messengerAccount = await db.MessengerAccounts.FindAsync(User.MessengerId);
        if (messengerAccount == null)
        {
            // something happened, setting "MessengerId" on user null
            User.MessengerId = null;
            db.Users.Update(User);
            await db.SaveChangesAsync();
            return NotFound("Info not found. Send post to request_registration");
        }
        return Ok(new MessengerInfoDto()
        {
            Account = messengerAccount,
            Domain = messengerConfig.Value.Domain
        });
    }

    [IsolaattiAuth]
    [Route("request_registration")]
    [HttpPost]
    public async Task<IActionResult> RequestRegistration()
    {
        var messengerAccounts = db.MessengerAccounts.Where(ma => ma.Id.Equals(User.MessengerId)).ToList();
        var response = new RequestRegistrationDto();
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
            var userCreatedSuccessfully = await messagingServerApi.CreateUser(newMessengerAccountUsername, User.Name, User.Email, newMessengerAccount.Password);
            if (userCreatedSuccessfully)
            {
                await db.SaveChangesAsync();
            }
            else
            {
                return Problem("An error occurred when creating user on messenger");
            }

            response.Result = MessengerInfoDto.Success;
            response.Account = newMessengerAccount;
        }
        else
        {
            response.Result = MessengerInfoDto.AlreadyExists;
            response.Account = messengerAccounts[0];
        }
        
        
        return Ok(response);
    }
}