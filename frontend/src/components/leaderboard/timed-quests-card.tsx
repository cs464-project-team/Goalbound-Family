import { useEffect, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";

import type { Quest } from "@/data/mockQuestsData";
import { Timer } from "lucide-react";

export function TimedQuests({ quest }: { quest: Quest }) {
  const [timeLeft, setTimeLeft] = useState<number>(() => {
    if (!quest.timeLimit || !quest.startTime) return 0;
    const now = new Date().getTime();
    const endTime =
      new Date(quest.startTime).getTime() + quest.timeLimit * 1000;
    return Math.max(Math.floor((endTime - now) / 1000), 0);
  });

  useEffect(() => {
    if (!quest.timeLimit || timeLeft <= 0) return;

    const interval = setInterval(() => {
      setTimeLeft((prev) => Math.max(prev - 1, 0));
    }, 1000);

    return () => clearInterval(interval);
  }, [quest.timeLimit, timeLeft]);

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  return (
    <>
      {/* Available Challenges */}
      <Card className="bg-linear-to-r from-cyan-50 to-blue-50 dark:from-cyan-950 dark:to-blue-950 border-cyan-200 dark:border-cyan-800 p-4 rounded-md mb-2 gap-2">
        <div className="flex justify-between items-center">
          <h3 className="text-lg font-bold"> Timed Challenge</h3>
          <Button variant="ghost" className="text-lg text-orange-600 dark:text-orange-400 hover:bg-transparent hover:text-orange-600 dark:hover:text-orange-400">
            <Timer className="text-orange-600 dark:text-orange-400" /> {formatTime(timeLeft)}
          </Button>
        </div>
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-3">
            <span className="text-5xl">⚡</span>
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

        <div className="relative w-full my-2">
          <Progress
            value={(quest.progress / quest.target) * 100}
            className="h-6 rounded-md bg-gray-100 *:bg-gray-200 shadow"
          />
          <span className="absolute inset-0 flex items-center justify-center text-xs font-bold">
            {quest.progress} / {quest.target}
          </span>
        </div>
      </Card>
    </>
  );
}
