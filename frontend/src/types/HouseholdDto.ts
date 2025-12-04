export interface HouseholdDto {
    id: string; // Guid -> string
    name: string;
  
    // Admin user (Parent)
    parentId: string; // Guid -> string
  
    // Optional: number of members
    memberCount: number;
  }
  
