// src/data/mockFamilyData.ts

export interface User {
  id: number;
  name: string;
  role: "parent" | "child";
  avatar: string;
  allowance?: number; // only for children
  balance?: number; // current allowance balance
  xp: number; // gamification XP
  streak: number; // consecutive activity days
  rank: number; // leaderboard rank
  badges: string[];
  savings: number;
  spendingScore: number;
  goalsCompleted: number;
}

export const familyUsers: User[] = [
  {
    id: 1,
    name: "Alice",
    role: "parent",
    avatar: "/avatars/alice.jpg",
    xp: 1800,
    streak: 22,
    rank: 1,
    badges: ["Budget Master", "Receipt Hero"],
    savings: 700,
    spendingScore: 98,
    goalsCompleted: 6,
  },
  {
    id: 2,
    name: "Bob",
    role: "parent",
    avatar: "/avatars/bob.jpg",
    xp: 1500,
    streak: 18,
    rank: 2,
    badges: ["Allowance Expert", "Smart Saver"],
    savings: 550,
    spendingScore: 90,
    goalsCompleted: 5,
  },
  {
    id: 3,
    name: "Charlie",
    role: "child",
    avatar: "/avatars/charlie.jpg",
    allowance: 50,
    balance: 50,
    xp: 1200,
    streak: 12,
    rank: 3,
    badges: ["Receipt Collector"],
    savings: 400,
    spendingScore: 85,
    goalsCompleted: 4,
  },
  {
    id: 4,
    name: "Diana",
    role: "child",
    avatar: "/avatars/diana.jpg",
    allowance: 40,
    balance: 40,
    xp: 900,
    streak: 9,
    rank: 4,
    badges: ["Budget Beginner"],
    savings: 300,
    spendingScore: 75,
    goalsCompleted: 2,
  },
  {
    id: 5,
    name: "Ethan",
    role: "child",
    avatar: "/avatars/ethan.jpg",
    allowance: 30,
    balance: 30,
    xp: 700,
    streak: 5,
    rank: 5,
    badges: ["First Saver"],
    savings: 150,
    spendingScore: 65,
    goalsCompleted: 1,
  },
];
