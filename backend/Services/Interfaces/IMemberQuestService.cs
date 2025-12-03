using GoalboundFamily.Api.DTOs;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IMemberQuestService
{
    Task<IEnumerable<MemberQuestDto>> GetQuestsForMemberAsync(Guid memberId);
    Task<MemberQuestDto?> GetAsync(Guid memberId, Guid questId);

    Task<MemberQuestDto> AssignQuestAsync(Guid memberId, Guid questId);
    Task<bool> UpdateProgressAsync(Guid memberId, Guid questId, int progress);
    Task<bool> CompleteQuestAsync(Guid memberId, Guid questId);
    Task<bool> ClaimQuestAsync(Guid memberId, Guid questId);
}
