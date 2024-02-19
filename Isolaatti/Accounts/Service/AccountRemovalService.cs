using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Config;
using Isolaatti.EmailSender;
using Isolaatti.Enums;
using Isolaatti.isolaatti_lib;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Isolaatti.Accounts.Service;

public class AccountRemovalService
{
    private readonly IOptions<HostConfig> _optionsHostConfig;
    private readonly DbContextApp _db;
    private readonly EmailSenderMessaging _emailSender;
    private readonly ImagesService _imagesService;
    private readonly IAccountsService _accountsService;
    private readonly AudiosService _audiosService;
    private readonly SquadInvitationsRepository _squadInvitationsRepository;
    private readonly SquadJoinRequestsRepository _squadJoinRequestsRepository;
    private readonly SquadsRepository _squadsRepository;

    public AccountRemovalService(
        DbContextApp db, 
        IOptions<HostConfig> optionsHostConfig, 
        EmailSenderMessaging emailSender, 
        ImagesService imagesService, 
        IAccountsService accountsService, 
        AudiosService audiosService, 
        SquadInvitationsRepository squadInvitationsRepository, 
        SquadJoinRequestsRepository squadJoinRequestsRepository, 
        SquadsRepository squadsRepository)
    {
        _db = db;
        _optionsHostConfig = optionsHostConfig;
        _emailSender = emailSender;
        _imagesService = imagesService;
        _accountsService = accountsService;
        _audiosService = audiosService;
        _squadInvitationsRepository = squadInvitationsRepository;
        _squadJoinRequestsRepository = squadJoinRequestsRepository;
        _squadsRepository = squadsRepository;
    }

    public async Task SendEmail(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.Equals(email));
        if (user == null)
        {
            return;
        }

        var key = Utils.RandomData.GenerateRandomKey(15);

        var passwordHasher = new PasswordHasher<string>();
        
        var keyHash = passwordHasher.HashPassword(user.Id.ToString(), key);

        var accountRemovalRequestEntity = new AccountRemovalRequestEntity()
        {
            UserId = user.Id,
            KeyHash = keyHash
        };

        _db.AccountRemovalRequests.Add(accountRemovalRequestEntity);
        await _db.SaveChangesAsync();
        
        // enqueue email
        var url = 
            $"{_optionsHostConfig.Value.BaseUrl}/AccountRemoval/ConfirmAccountRemoval?id={accountRemovalRequestEntity.Id}&key={HttpUtility.UrlEncode(key)}";
        var body = string.Format(EmailTemplates.DeleteAccountEmail, url);
        _emailSender.SendEmail(
            "no-reply@isolaatti.com", 
            "Isolaatti", 
            user.Email, 
            user.Name, 
            "Elimina tu cuenta de Isolaatti", 
            body);
    }

    public async Task<bool> ProceedWithAccountRemoval(Guid accountRemovalRequestId, string key)
    {
        var accountRemovalRequest = await _db.AccountRemovalRequests.FindAsync(accountRemovalRequestId);
        if (accountRemovalRequest == null)
        {
            return false;
        }
        
        // check key

        var hasher = new PasswordHasher<string>();

        var hashVerificationResult = hasher.VerifyHashedPassword(accountRemovalRequest.UserId.ToString(), accountRemovalRequest.KeyHash, key);

        if (hashVerificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        await DeleteAudios(accountRemovalRequest.UserId);
        await DeleteImages(accountRemovalRequest.UserId);
        await DeleteSessions(accountRemovalRequest.UserId);
        await HandleUserRemovalFromSquads(accountRemovalRequest.UserId);
        await DeleteAccount(accountRemovalRequest.UserId);
        
        return true;
    }
    
    // remove data that is not in relational database first
    private async Task DeleteImages(int userId)
    {
        await _imagesService.DeleteUserImages(userId);
    }

    private async Task DeleteSessions(int userId)
    {
        await _accountsService.RemoveAllSessions(userId, null);
    }

    private async Task DeleteAudios(int userId)
    {
        await _audiosService.DeleteAllUserAudios(userId);
    }
    
    // special case for squads:
    // check every squad user owns.
    // 1. if squad will have 0 members remaining, squad will be removed entirely
    // 2. if it has some admin, whoever has more "points" will be promoted to owner
    // 3. if no admin, ordinary member with the most "points" will be promoted to owner
    private async Task HandleUserRemovalFromSquads(int userId)
    {
        await _squadInvitationsRepository.RemoveInvitationsFromAndToUser(userId);
        await _squadJoinRequestsRepository.RemoveJoinRequestFromAndToUser(userId);

        var squadsUserOwns = _squadsRepository.GetSquadsUserOwns(userId).ToList();
        
        foreach (var squad in squadsUserOwns)
        {
            var membersCount = _db.SquadUsers
                .Where(su => su.SquadId.Equals(squad))
                .GroupBy(sq => sq.Role)
                .ToDictionary(sqg => sqg.Key);

            var adminCount = membersCount.GetValueOrDefault(SquadUserRole.Admin)?.Count();
            var ordinaryMemberCount = membersCount.GetValueOrDefault(SquadUserRole.User)?.Count();
            
            if (adminCount is > 0)
            {
                var mostParticipativeAdmin = _db.SquadUsers
                    .Where(su => su.SquadId.Equals(squad.Id) && su.Role == SquadUserRole.Admin)
                    .MaxBy(su => su.Ranking);

                if (mostParticipativeAdmin != null)
                {
                    await _squadsRepository.SetSquadOwner(squad.Id, mostParticipativeAdmin.UserId);
                }
                
            } 
            else if (ordinaryMemberCount is > 0)
            {
                var mostParticipativeOrdinaryMember = _db.SquadUsers
                    .Where(su => su.SquadId.Equals(squad.Id) && su.Role == SquadUserRole.User)
                    .MaxBy(su => su.Ranking);

                if (mostParticipativeOrdinaryMember != null)
                {
                    await _squadsRepository.SetSquadOwner(squad.Id, mostParticipativeOrdinaryMember.UserId);
                }
            }
            else
            {
                await _squadsRepository.RemoveSquad(squad.Id);
            }
        }
    }
    
    // deleting user will cause all related data to be removed in cascade
    private async Task DeleteAccount(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}