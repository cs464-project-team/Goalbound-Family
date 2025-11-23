// Leaderboard.tsx
import { familyUsers } from "@/data/mockFamilyData";
import { quests } from "@/data/mockQuestsData";

import { Ranking } from "@/components/leaderboard/ranking";
import { Quests } from "@/components/leaderboard/quests-section";

export default function Leaderboard() {
  return (
    <div className="px-6">
      <h2 className="text-2xl font-bold mb-4">Family Leaderboard</h2>
      <Ranking familyUsers={familyUsers} />
      <h2 className="text-2xl font-bold mt-4">Quests</h2>
      <h2 className="text-sm text-gray-600 mb-4">
        Complete quests to earn rewards!
      </h2>
      <Quests quests={quests} />
    </div>
  );
}
