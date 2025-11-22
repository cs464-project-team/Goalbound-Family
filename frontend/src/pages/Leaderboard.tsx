// Leaderboard.tsx
import { familyUsers } from "@/data/mockFamilyData";

import { Ranking } from "@/components/leaderboard/ranking";

export default function Leaderboard() {
  return (
    <div className="px-6">
      <h2 className="text-2xl font-bold mb-4">Family Leaderboard</h2>
      <Ranking familyUsers={familyUsers} />
    </div>
  );
}
