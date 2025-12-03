"use client";

import { Separator } from "@/components/ui/separator";

import { QuestTable } from "./quests-table";
import { TimedQuests } from "./timed-quests-card";

import { useState, useEffect } from "react";
import { useAuthContext } from "../../context/AuthProvider";
import { getApiUrl } from "../../config/api";

interface MemberQuestDto {
  householdMemberId: string; // Guid -> string
  questId: string;           // Guid -> string

  // MemberQuest fields
  status: string;
  progress: number;
  assignedAt: string;        // DateTime -> string (ISO format)
  startTime?: string;        // nullable DateTime
  completedAt?: string;      // nullable DateTime
  claimedAt?: string;        // nullable DateTime

  // Quest fields
  title: string;
  description: string;
  xpReward: number;
  category: string;
  type: string;
  difficulty: string;
  target: number;
}

interface QuestsProps {
  householdId: string;
}

export function Quests({ householdId }: QuestsProps) {
  const { userId } = useAuthContext();
  const [householdMemberId, setHouseholdMemberId] = useState<string | null>(
    null
  );
  const [quests, setQuests] = useState<MemberQuestDto[]>([]);

  const fetchHouseholdMemberId = async () => {
    try {
      const res = await fetch(
        getApiUrl(`api/householdmembers/${householdId}/user/${userId}`)
      );
      if (!res.ok) throw new Error("Failed to fetch households");
      const data = await res.json();
      setHouseholdMemberId(data?.id ?? null);
    } catch (error) {
      console.error(error);
      setHouseholdMemberId(null); // fallback on error
    }
  };

  const fetchQuests = async () => {
    if (!householdMemberId) return;
    try {
      const res = await fetch(
        `http://localhost:5073/api/memberquests/${householdMemberId}`
      );
      if (!res.ok) throw new Error("Failed to fetch quests");
      const data = await res.json();
      console.log("Fetched quests data:", data);
      setQuests(data ?? []);
    } catch (error) {
      console.error(error);
      setQuests([]); // fallback on error
    }
  };

  // Fetch household member ID when householdId or userId changes
  useEffect(() => {
    if (!householdId || !userId) return;
    fetchHouseholdMemberId();
  }, [householdId, userId]);

  // Fetch quests when householdMemberId is available
  useEffect(() => {
    if (!householdMemberId) return;
    fetchQuests();
  }, [householdMemberId]);

  console.log("HouseholdMemberId:", householdMemberId);
  console.log("Quests:", quests);

  const dailyQuests = quests.filter((q) => q.type === "daily");
  const weeklyQuests = quests.filter((q) => q.type === "weekly");
  const timedQuest: Quest | null =
    quests.find((q) => q.type === "timed") || null;

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
            <QuestTable quests={dailyQuests} />

            {/* Weekly Quests */}
            <h3 className="text-xl font-bold mb-4">📅 Weekly Quests</h3>
            <Separator className="mb-4" />
            <QuestTable quests={weeklyQuests} />
          </div>
        </>
      )}
    </>
  );
}
