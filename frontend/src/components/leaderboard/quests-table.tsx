import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { authenticatedFetch } from '../../services/authService';
import { getApiUrl } from "../../config/api";
import type { MemberQuestDto } from '../../types/MemberQuestDto';

interface QuestTableProps {
  quests: MemberQuestDto[];
  householdMemberId: string;
  onClaim: (questId: string) => void;
}

export function QuestTable({ quests, householdMemberId, onClaim }: QuestTableProps) {
  const handleClaim = async (questId: string) => {
    try {
      const res = await authenticatedFetch(getApiUrl('/api/memberquests/claim'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          memberId: householdMemberId,
          questId: questId
        })
      });

      if (!res.ok) throw new Error('Failed to claim quest');
      onClaim(questId);
    } catch (error) {
      console.error('Error claiming quest:', error);
    }
  };

  if (!quests || quests.length === 0) {
    return <p className="text-gray-500 text-center py-4">No quests available</p>;
  }

  return (
    <div className="space-y-4">
      {quests.map((quest) => {
        const progressPercent = quest.target > 0 ? (quest.progress / quest.target) * 100 : 0;
        const isCompleted = quest.status === 'completed';
        const isClaimed = quest.status === 'claimed';

        return (
          <div
            key={quest.questId}
            className="p-4 bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-shadow"
          >
            <div className="flex justify-between items-start mb-3">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <h4 className="font-semibold text-lg">{quest.title}</h4>
                  <Badge
                    variant={
                      quest.difficulty === 'easy'
                        ? 'default'
                        : quest.difficulty === 'medium'
                        ? 'secondary'
                        : 'destructive'
                    }
                  >
                    {quest.difficulty}
                  </Badge>
                  {isClaimed && (
                    <Badge variant="outline" className="bg-green-50 text-green-700 border-green-300">
                      Claimed
                    </Badge>
                  )}
                </div>
                <p className="text-sm text-gray-600">{quest.description}</p>
              </div>
              <div className="text-right ml-4">
                <div className="text-xl font-bold text-purple-600">{quest.xpReward} XP</div>
              </div>
            </div>

            <div className="space-y-2">
              <div className="flex justify-between text-sm text-gray-600">
                <span>Progress</span>
                <span>
                  {quest.progress} / {quest.target}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="h-2 rounded-full transition-all"
                  style={{ 
                    width: Math.min(progressPercent, 100) + '%',
                    backgroundColor: isCompleted ? 'rgb(34, 197, 94)' : 'rgb(147, 51, 234)'
                  }}
                />
              </div>
            </div>

            {isCompleted && !isClaimed && (
              <div className="mt-3">
                <Button
                  onClick={() => handleClaim(quest.questId)}
                  className="w-full bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700"
                >
                  Claim Reward (+{quest.xpReward} XP)
                </Button>
              </div>
            )}

            {isClaimed && (
              <div className="mt-3 text-center text-sm text-green-600 font-medium">
                Quest completed and claimed!
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
