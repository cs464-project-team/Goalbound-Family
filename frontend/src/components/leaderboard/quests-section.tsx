"use client";

import { Separator } from "@/components/ui/separator";

import { QuestTable } from "./quests-table";
import { TimedQuests } from "./timed-quests-card";
import type { MemberQuestDto } from '../../types/MemberQuestDto';
import type { HouseholdMemberDto } from '../../types/HouseholdMemberDto';

interface QuestsProps {
  householdMemberId: string | null;
  quests: MemberQuestDto[];
  setQuests: React.Dispatch<React.SetStateAction<MemberQuestDto[]>>;
  setHouseholdMembers: React.Dispatch<React.SetStateAction<HouseholdMemberDto[]>>;
}

export function Quests({ householdMemberId, quests, setQuests, setHouseholdMembers }: QuestsProps) {
  if (!householdMemberId) return <p>Loading...</p>;
  if (!quests || quests.length === 0) return <p>No quests available</p>;

  const dailyQuests = quests.filter(q => q.type === "daily");
  const weeklyQuests = quests.filter(q => q.type === "weekly");
  const timedQuest = quests.find(q => q.type === "timed") || null;

  // Update quest status + XP optimistically
  const markQuestClaimed = (questId: string) => {
    const quest = quests.find(q => q.questId === questId);
    if (!quest) return;

    // Update quest status
    setQuests(prev => prev.map(q => q.questId === questId ? { ...q, status: "claimed" } : q));

    // Update household member XP
    setHouseholdMembers(prev =>
      prev.map(member =>
        member.id === householdMemberId
          ? { ...member, xp: member.xp + quest.xpReward, questsCompleted: member.questsCompleted + 1 }
          : member
      )
    );
  };

  return (
    <>
      {quests.length === 0 ? (
        <p>No quests available</p>
      ) : (
        <>
          {timedQuest && <TimedQuests quest={timedQuest} />}
          <div className="p-6 rounded-md shadow">
            {/* Daily Quests */}
            <h3 className="text-xl font-bold mb-4">📅 Daily Quests</h3>
            <Separator className="mb-4" />
            <QuestTable quests={dailyQuests} householdMemberId={householdMemberId} onClaim={markQuestClaimed} />

            {/* Weekly Quests */}
            <h3 className="text-xl font-bold mb-4">📅 Weekly Quests</h3>
            <Separator className="mb-4" />
            <QuestTable quests={weeklyQuests} householdMemberId={householdMemberId} onClaim={markQuestClaimed} />
          </div>
        </>
      )}
    </>
  );
}
