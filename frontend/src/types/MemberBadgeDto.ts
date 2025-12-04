export interface MemberBadgeDto {
    badgeId: string; // Guid -> string
    name: string;
    description: string;
    icon: string;
    earnedAt: string; // DateTime -> string (ISO format)
  }