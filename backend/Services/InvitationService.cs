using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _inviteRepo;
    private readonly IHouseholdMemberRepository _memberRepo;

    public InvitationService(
        IInvitationRepository inviteRepo,
        IHouseholdMemberRepository memberRepo)
    {
        _inviteRepo = inviteRepo;
        _memberRepo = memberRepo;
    }

    public async Task<InvitationDto> CreateAsync(CreateInvitationRequest request)
    {
        var token = Guid.NewGuid().ToString();

        var invitation = new Invitation
        {
            // Email = request.Email,
            HouseholdId = request.HouseholdId,
            InvitedByUserId = request.InvitedByUserId,
            Token = token
        };

        await _inviteRepo.AddAsync(invitation);
        await _inviteRepo.SaveChangesAsync();

        return new InvitationDto
        {
            Id = invitation.Id,
            // Email = invitation.Email,
            HouseholdId = invitation.HouseholdId,
            InvitedByUserId = invitation.InvitedByUserId,
            ExpiresAt = invitation.ExpiresAt,
            IsAccepted = false,
            Token = invitation.Token
        };
    }

    public async Task<InvitationDto?> GetAsync(Guid id)
    {
        var invite = await _inviteRepo.GetByIdAsync(id);
        if (invite == null) return null;

        return new InvitationDto
        {
            Id = invite.Id,
            // Email = invite.Email,
            HouseholdId = invite.HouseholdId,
            InvitedByUserId = invite.InvitedByUserId,
            ExpiresAt = invite.ExpiresAt,
            IsAccepted = invite.IsAccepted
        };
    }

    public async Task<bool> AcceptAsync(AcceptInvitationRequest request)
    {
        Console.WriteLine($"[AcceptInvite] userId={request.UserId}, token={request.Token}");

        var invite = await _inviteRepo.GetByTokenAsync(request.Token);
        if (invite == null) { Console.WriteLine("[AcceptInvite] Invite not found"); return false; }

        if (invite.ExpiresAt < DateTime.UtcNow) { Console.WriteLine("[AcceptInvite] Invite expired"); return false; }

        if (invite.IsAccepted) { Console.WriteLine("[AcceptInvite] Invite already accepted"); return false; }

        // Check if user is already a member of the household
        var existingMember = await _memberRepo.GetByUserAndHouseholdAsync(request.UserId, invite.HouseholdId);
        if (existingMember != null)
        {
            Console.WriteLine("[AcceptInvite] User already a member");
            invite.IsAccepted = true;
            await _inviteRepo.UpdateAsync(invite);
            await _inviteRepo.SaveChangesAsync();
            return true;
        }

        // Add new member to household
        try
        {
            Console.WriteLine("[AcceptInvite] Adding new member");
            var member = new HouseholdMember
            {
                HouseholdId = invite.HouseholdId,
                UserId = request.UserId,
                Role = "Member"
            };

            await _memberRepo.AddAsync(member);

            invite.IsAccepted = true;
            await _inviteRepo.UpdateAsync(invite);

            await _inviteRepo.SaveChangesAsync();
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Duplicate key, treat as success
                Console.WriteLine("[AcceptInvite] Duplicate key error, treating as success");
                invite.IsAccepted = true;
                await _inviteRepo.UpdateAsync(invite);
                await _inviteRepo.SaveChangesAsync();
                return true;
            }
            throw;
        }
    }
}