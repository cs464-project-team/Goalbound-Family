using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestsController : ControllerBase
{
    private readonly IQuestRepository _questRepo;
    private readonly ApplicationDbContext _context;

    public QuestsController(IQuestRepository questRepo, ApplicationDbContext context)
    {
        _questRepo = questRepo;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quest>>> GetAll()
    {
        var quests = await _questRepo.GetAllAsync();
        return Ok(quests);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Quest>> GetById(Guid id)
    {
        var quest = await _questRepo.GetByIdAsync(id);
        if (quest == null) return NotFound();
        return Ok(quest);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedQuests()
    {
        await QuestSeeder.SeedQuests(_context);
        return Ok(new { message = "Quests seeded successfully" });
    }

    [HttpPost("assign-to-member/{memberId:guid}")]
    public async Task<IActionResult> AssignQuestsToMember(Guid memberId)
    {
        var quests = await _questRepo.GetAllAsync();
        var memberQuestRepo = HttpContext.RequestServices.GetRequiredService<Repositories.Interfaces.IMemberQuestRepository>();

        int assigned = 0;
        foreach (var quest in quests)
        {
            // Check if already assigned
            var existing = await memberQuestRepo.GetAsync(memberId, quest.Id);
            if (existing == null)
            {
                var memberQuest = new MemberQuest
                {
                    HouseholdMemberId = memberId,
                    QuestId = quest.Id,
                    Status = "in-progress",
                    Progress = 0,
                    AssignedAt = DateTime.UtcNow
                };
                await memberQuestRepo.AddAsync(memberQuest);
                assigned++;
            }
        }

        await memberQuestRepo.SaveChangesAsync();
        return Ok(new { message = $"Assigned {assigned} quests to member" });
    }
}
