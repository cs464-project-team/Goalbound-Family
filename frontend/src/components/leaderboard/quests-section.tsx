"use client";

import { Separator } from "@/components/ui/separator";

import type { Quest } from "@/data/mockQuestsData";
import { QuestTable } from "./quests-table";
import { TimedQuests } from "./timed-quests-card";

interface QuestsProps {
  quests: Quest[];
}

export function Quests({ quests }: QuestsProps) {
  const dailyQuests = quests.filter((q) => q.type === "daily");
  const weeklyQuests = quests.filter((q) => q.type === "weekly");
  const timedQuest: Quest | null = quests.find(q => q.type === "timed") || null;

  return (
    <>
      {timedQuest && <TimedQuests quest={timedQuest} />}
      <div className="p-6 rounded-md shadow">
        {/* Daily Quests */}
        <h3 className="text-xl font-bold mb-4">📅 Daily Quests</h3>
        <Separator className="mb-4" />
        <QuestTable quests={dailyQuests} />

        {/* Weekly Quests */}
        <h3 className="text-xl font-bold mb-4">📅 Weekly Quests</h3>
        <Separator className="mb-4" />
        <QuestTable quests={weeklyQuests} />
      </div>
    </>
  );
}


