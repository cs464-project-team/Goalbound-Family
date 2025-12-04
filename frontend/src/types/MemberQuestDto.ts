import type { QuestCategory } from './QuestCategory';

export interface MemberQuestDto {
    householdMemberId: string; // Guid -> string
    questId: string; // Guid -> string
  
    // MemberQuest fields
    status: string;
    progress: number;
    assignedAt: string; // DateTime -> string (ISO format)
    startTime?: string; // nullable DateTime
    completedAt?: string; // nullable DateTime
    claimedAt?: string; // nullable DateTime
  
    // Quest fields
    title: string;
    description: string;
    xpReward: number;
    category: QuestCategory;
    type: string;
    difficulty: string;
    target: number;

    timeLimitSeconds?: number | null;
  }

