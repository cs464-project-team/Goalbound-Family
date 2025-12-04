using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberQuestsController : ControllerBase
{
    private readonly IMemberQuestService _service;

    public MemberQuestsController(IMemberQuestService service)
    {
        _service = service;
    }

    [HttpGet("{memberId:guid}")]
    public async Task<ActionResult<IEnumerable<MemberQuestDto>>> GetForMember(Guid memberId)
    {
        var quests = await _service.GetQuestsForMemberAsync(memberId);
        // Print each quest to the console
        foreach (var q in quests)
        {
            Console.WriteLine($"Quest: {q.Title}, Status: {q.Status}, Progress: {q.Progress}");
        }
        return Ok(quests);
    }

    [HttpGet("{memberId:guid}/{questId:guid}")]
    public async Task<ActionResult<MemberQuestDto>> Get(Guid memberId, Guid questId)
    {
        var quest = await _service.GetAsync(memberId, questId);
        if (quest == null) return NotFound();
        return Ok(quest);
    }

    [HttpPost("assign")]
    public async Task<ActionResult<MemberQuestDto>> Assign([FromBody] AssignRequest req)
    {
        var result = await _service.AssignQuestAsync(req.MemberId, req.QuestId);
        return Ok(result);
    }

    [HttpPost("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] ProgressRequest req)
    {
        var success = await _service.UpdateProgressAsync(req.MemberId, req.QuestId, req.Progress);
        return success ? Ok() : NotFound();
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] QuestRequest req)
    {
        var success = await _service.CompleteQuestAsync(req.MemberId, req.QuestId);
        return success ? Ok() : NotFound();
    }

    [HttpPost("claim")]
    public async Task<IActionResult> Claim([FromBody] QuestRequest req)
    {
        var success = await _service.ClaimQuestAsync(req.MemberId, req.QuestId);
        return success ? Ok() : NotFound();
    }

    // Request classes
    public record AssignRequest(Guid MemberId, Guid QuestId);
    public record ProgressRequest(Guid MemberId, Guid QuestId, int Progress);
    public record QuestRequest(Guid MemberId, Guid QuestId);
}
