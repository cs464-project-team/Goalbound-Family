using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IInvitationService
{
    Task<InvitationDto> CreateAsync(CreateInvitationRequest request, Guid invitedByUserId);
    Task<InvitationDto?> GetAsync(Guid id);
    Task<bool> AcceptAsync(AcceptInvitationRequest request);
}