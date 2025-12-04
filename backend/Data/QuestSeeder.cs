using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Data;

public static class QuestSeeder
{
    public static async Task SeedQuests(ApplicationDbContext context)
    {
        // Check if quests already exist
        if (context.Quests.Any())
        {
            Console.WriteLine("Quests already seeded.");
            return;
        }

        var quests = new List<Quest>
        {
            // Daily Quests - Easy
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "daily",
                Title = "Log Your First Expense",
                Description = "Track a purchase by logging an expense today",
                XpReward = 10,
                Target = 1,
                Difficulty = "easy",
                Category = "expense",
                IsRepeatable = true
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "daily",
                Title = "Scan a Receipt",
                Description = "Use the receipt scanner to log an expense",
                XpReward = 15,
                Target = 1,
                Difficulty = "easy",
                Category = "receipt",
                IsRepeatable = true
            },

            // Daily Quests - Medium
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "daily",
                Title = "Track 3 Expenses",
                Description = "Log at least 3 expenses today",
                XpReward = 25,
                Target = 3,
                Difficulty = "medium",
                Category = "expense",
                IsRepeatable = true
            },

            // Weekly Quests - Medium
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "weekly",
                Title = "Weekly Expense Tracker",
                Description = "Log 10 expenses this week",
                XpReward = 50,
                Target = 10,
                Difficulty = "medium",
                Category = "expense",
                IsRepeatable = true
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "weekly",
                Title = "Receipt Master",
                Description = "Scan 5 receipts this week",
                XpReward = 75,
                Target = 5,
                Difficulty = "medium",
                Category = "receipt",
                IsRepeatable = true
            },

            // Weekly Quests - Hard
            new Quest
            {
                Id = Guid.NewGuid(),
                Type = "weekly",
                Title = "Budget Champion",
                Description = "Log 20 expenses this week",
                XpReward = 100,
                Target = 20,
                Difficulty = "hard",
                Category = "expense",
                IsRepeatable = true
            }
        };

        await context.Quests.AddRangeAsync(quests);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {quests.Count} quests successfully.");
    }
}
