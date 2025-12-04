import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";

import { getApiUrl } from '../../config/api';
import type { MemberQuestDto } from '../../types/MemberQuestDto';
import { categoryIcons } from '../../types/QuestCategory';

interface QuestRequest {
  memberId: string;
  questId: string;
}

interface QuestRowProps {
  quests: MemberQuestDto[];
  householdMemberId: string;
  onClaim: (questId: string) => void;  // <--- this is the type
}

export const QuestTable: React.FC<QuestRowProps> = ({ quests, householdMemberId, onClaim }) => {
    async function handleClaim(questId: string) {
      const reqBody: QuestRequest = { memberId: householdMemberId, questId };
    
      const response = await fetch(getApiUrl("/api/memberquests/claim"), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(reqBody),
      });
    
      if (response.ok) {
        onClaim(questId);
        console.log("Quest claimed successfully!");
      } else if (response.status === 404) {
        console.log("Quest not found or not eligible to claim.");
      } else {
        console.error("Failed to claim quest:", response.statusText);
      }
    }

    return (
      <div className="flex flex-col">
        {quests.map((quest) => (
          <div key={quest.questId}>
            <div className="flex justify-between items-center text-sm">
              <div className="flex items-center gap-3">
                <span className="text-5xl">{categoryIcons[quest.category] ?? categoryIcons["others"]}</span>
                <div className="flex flex-col leading-tight">
                  <p className="font-bold text-lg">{quest.title}</p>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    {quest.description}
                  </p>
                </div>
              </div>
              <Button
                disabled={quest.status !== "completed"} // disabled if not "completed"
                className="justify-end"
                onClick={() => handleClaim(quest.questId)}
              >
                {quest.status === "claimed"
                  ? "Reward Claimed" // show this if already claimed
                  : `Claim ${quest.xpReward} XP`}{" "}
              </Button>
            </div>
  
            <div className="relative w-full my-4">
              <Progress
                value={(quest.progress / quest.target) * 100}
                className="h-6 rounded-md bg-gray-100 *:bg-gray-200 shadow"
              />
              <span className="absolute inset-0 flex items-center justify-center text-xs font-bold">
                {quest.progress} / {quest.target}
              </span>
            </div>
  
            <Separator className="my-8" />
          </div>
        ))}
      </div>
    );
  }