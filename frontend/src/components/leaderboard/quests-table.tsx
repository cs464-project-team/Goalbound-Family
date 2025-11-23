import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";

import type { Quest } from "@/data/mockQuestsData";

export function QuestTable({ quests }: { quests: Quest[] }) {
    const categoryIcons = {
      finance: "💰",
      food: "🍎",
      health: "💪",
      productivity: "🎯",
    };
  
    return (
      <div className="flex flex-col">
        {quests.map((quest) => (
          <div key={quest.id}>
            <div className="flex justify-between items-center text-sm">
              <div className="flex items-center gap-3">
                <span className="text-5xl">{categoryIcons[quest.category]}</span>
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