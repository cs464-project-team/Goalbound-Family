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
            Email = request.Email,
            HouseholdId = request.HouseholdId,
            InvitedByUserId = request.InvitedByUserId,
            Token = token
        };

        await _inviteRepo.AddAsync(invitation);
        await _inviteRepo.SaveChangesAsync();

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            HouseholdId = invitation.HouseholdId,
            InvitedByUserId = invitation.InvitedByUserId,
            ExpiresAt = invitation.ExpiresAt,
            IsAccepted = false
        };
    }

    public async Task<InvitationDto?> GetAsync(Guid id)
    {
        var invite = await _inviteRepo.GetByIdAsync(id);
        if (invite == null) return null;

        return new InvitationDto 
        {
            Id = invite.Id,
            Email = invite.Email,
            HouseholdId = invite.HouseholdId,
            InvitedByUserId = invite.InvitedByUserId,
            ExpiresAt = invite.ExpiresAt,
            IsAccepted = invite.IsAccepted
        };
    }

    public async Task<bool> AcceptAsync(AcceptInvitationRequest request)
    {
        var invite = await _inviteRepo.GetByTokenAsync(request.Token);
        if (invite == null) return false;

        if (invite.ExpiresAt < DateTime.UtcNow) return false;

        // Add new member to household
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
}