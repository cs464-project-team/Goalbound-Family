// Leaderboard.tsx
import { Ranking } from "@/components/leaderboard/ranking";
import { Quests } from "@/components/leaderboard/quests-section";

import { useAuthContext } from "../context/AuthProvider";
import { useState, useEffect } from "react";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  SelectGroup,
  SelectLabel,
} from "@/components/ui/select";
import { getApiUrl } from "../config/api";
import type { HouseholdDto } from "../types/HouseholdDto";
import type { HouseholdMemberDto } from "../types/HouseholdMemberDto";
import type { MemberQuestDto } from "../types/MemberQuestDto";

export default function Leaderboard() {
  const { userId } = useAuthContext();
  const [households, setHouseholds] = useState<HouseholdDto[]>([]);
  const [selectedHousehold, setSelectedHousehold] = useState<string | null>(
    null
  );
  const [loading, setLoading] = useState(true);

  // Shared state
  const [householdMemberId, setHouseholdMemberId] = useState<string | null>(
    null
  );
  const [householdMembers, setHouseholdMembers] = useState<
    HouseholdMemberDto[]
  >([]);
  const [quests, setQuests] = useState<MemberQuestDto[]>([]);

  // Fetch households
  useEffect(() => {
    if (!userId) return;
    const fetchHouseholds = async () => {
      try {
        const res = await fetch(
          getApiUrl(`api/householdmembers/user/${userId}`)
        );
        if (!res.ok) throw new Error("Failed to fetch households");
        const data = await res.json();
        setHouseholds(data);
        if (data.length > 0) setSelectedHousehold(data[0].id);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchHouseholds();
  }, [userId]);

  // Fetch household member ID
  useEffect(() => {
    if (!selectedHousehold || !userId) return;
    const fetchMemberId = async () => {
      try {
        const res = await fetch(
          getApiUrl(`api/householdmembers/${selectedHousehold}/user/${userId}`)
        );
        if (!res.ok) throw new Error("Failed to fetch household member ID");
        const data = await res.json();
        setHouseholdMemberId(data?.id ?? null);
      } catch (error) {
        console.error(error);
      }
    };
    fetchMemberId();
  }, [selectedHousehold, userId]);

  // Fetch household members
  useEffect(() => {
    if (!selectedHousehold) return;
    const fetchMembers = async () => {
      try {
        const res = await fetch(
          getApiUrl(`api/householdmembers/${selectedHousehold}`)
        );
        if (!res.ok) throw new Error("Failed to fetch members");
        const data = await res.json();
        setHouseholdMembers(data || []);
      } catch (error) {
        console.error(error);
      }
    };
    fetchMembers();
  }, [selectedHousehold]);

  // Fetch quests
  useEffect(() => {
    if (!householdMemberId) return;
    const fetchQuests = async () => {
      try {
        const res = await fetch(
          getApiUrl(`api/memberquests/${householdMemberId}`)
        );
        if (!res.ok) throw new Error("Failed to fetch quests");
        const data = await res.json();
        setQuests(data ?? []);
      } catch (error) {
        console.error(error);
      }
    };
    fetchQuests();
  }, [householdMemberId]);

  if (loading) return <p>Loading...</p>;
  if (!households.length) return <p>You are not part of any household yet.</p>;

  return (
    <div className="px-6">
      <div className="flex gap-3 text-center items-center mb-4">
        <h2 className="text-2xl font-bold">Family Leaderboard</h2>
        <Select
          value={selectedHousehold ?? ""}
          onValueChange={setSelectedHousehold}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Select household" />
          </SelectTrigger>
          <SelectContent>
            <SelectGroup>
              <SelectLabel>Households</SelectLabel>
              {households.map((h) => (
                <SelectItem key={h.id} value={h.id}>
                  {h.name}
                </SelectItem>
              ))}
            </SelectGroup>
          </SelectContent>
        </Select>
      </div>

      {selectedHousehold && householdMemberId && (
        <>
          <Ranking householdMembers={householdMembers} />
          <h2 className="text-2xl font-bold mt-4">Quests</h2>
          <p className="text-sm text-gray-600 mb-4">
            Complete quests to earn rewards!
          </p>
          <Quests
            householdMemberId={householdMemberId}
            quests={quests}
            setQuests={setQuests}
            householdMembers={householdMembers}
            setHouseholdMembers={setHouseholdMembers}
          />
        </>
      )}
    </div>
  );
}
