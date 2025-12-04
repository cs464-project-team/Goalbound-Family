import type { MemberBadgeDto } from "./MemberBadgeDto";

export interface HouseholdMemberDto {
    id: string; // Guid -> string
    userId: string; // Guid -> string
    firstName: string;
    lastName: string;
    email: string;
  
    // Combined name for UI display
    userName: string;
  
    role: string; // default: "Member"
    joinedAt: string; // DateTime -> string (ISO format)
  
    // Optional avatar
    avatar: string;
  
    // Gamification
    xp: number; // default: 0
    streak: number; // default: 0
    questsCompleted: number; // default: 0
  
    // Badges
    badges: MemberBadgeDto[];
  }