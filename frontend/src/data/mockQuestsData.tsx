export interface Quest {
  id: string;
  type: "daily" | "weekly" | "timed";
  title: string;
  description: string;
  xpReward: number;
  progress: number;
  target: number;
  status: "in-progress" | "completed" | "claimed"; // replaced completed + claimed
  difficulty: "easy" | "medium" | "hard";
  category: "finance" | "food" | "health" | "productivity";
  timeLimit?: number;     // seconds
  startTime?: Date;       // when the timed quest started
}

export const quests: Quest[] = [
    // Daily Quests
    {
      id: "1",
      type: "daily",
      title: "Receipt Logger",
      description: "Log one expense or scan a receipt",
      xpReward: 100,
      progress: 1,
      target: 1,
      status: "claimed",
      difficulty: "easy",
      category: "finance",
    },
    {
      id: "2",
      type: "daily",
      title: "Budget Champion",
      description: "Stay under daily discretionary budget",
      xpReward: 150,
      progress: 42,
      target: 50,
      status: "in-progress",
      difficulty: "medium",
      category: "finance",
    },
  
    // Weekly Quests
    {
      id: "3",
      type: "weekly",
      title: "Grocery Master",
      description: "Stay under weekly grocery budget",
      xpReward: 500,
      progress: 145,
      target: 200,
      status: "in-progress",
      difficulty: "hard",
      category: "food",
    },
    {
      id: "4",
      type: "weekly",
      title: "Streak Master",
      description: "Maintain all streaks for 7 consecutive days",
      xpReward: 600,
      progress: 5,
      target: 7,
      status: "in-progress",
      difficulty: "hard",
      category: "productivity",
    },
  
    // Timed Challenges
    // {
    //   id: "tc1",
    //   type: "timed",
    //   title: "Receipt Sprint",
    //   description: "Complete 3 receipts in 10 minutes",
    //   xpReward: 250,
    //   progress: 0,
    //   target: 3,
    //   status: "in-progress",
    //   difficulty: "medium",
    //   category: "finance",
    //   timeLimit: 10 * 60, // 10 minutes
    //   startTime: new Date(), // now
    // },
    {
      id: "tc2",
      type: "timed",
      title: "Home Cook Marathon",
      description: "Log 5 home-cooked meals in 2 hours",
      xpReward: 300,
      progress: 1,
      target: 5,
      status: "in-progress",
      difficulty: "medium",
      category: "food",
      timeLimit: 2 * 60 * 60, // 2 hours
      startTime: new Date(),
    },
  ];
  