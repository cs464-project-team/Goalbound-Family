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

interface HouseholdDto {
  id: string;          // Guid -> string
  name: string;

  // Admin user (Parent)
  parentId: string;    // Guid -> string

  // Optional: number of members
  memberCount: number;
}

export default function Leaderboard() {
  const { userId } = useAuthContext();
  const [households, setHouseholds] = useState<HouseholdDto[]>([]);
  const [selectedHousehold, setSelectedHousehold] = useState<string | null>(
    null
  );
  const [loading, setLoading] = useState(true);
  console.log("Current User ID:", userId);

  useEffect(() => {
    if (!userId) return; // wait until userId is available

    const fetchHouseholds = async () => {
      try {
        const res = await fetch(
          getApiUrl(`api/householdmembers/user/${userId}`)
        );
        if (!res.ok) throw new Error("Failed to fetch households");
        const data = await res.json();
        setHouseholds(data);
        // Default to first household if available
        if (data.length > 0) setSelectedHousehold(data[0].id);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    fetchHouseholds();
  }, [userId]);

  if (loading) return <p>Loading...</p>;

  // If no households, don't show leaderboard
  if (households.length === 0)
    return <p>You are not part of any household yet.</p>;

  console.log("Households for user:", households);

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
              {households.map((household) => (
                <SelectItem key={household.id} value={household.id}>
                  {household.name}
                </SelectItem>
              ))}
            </SelectGroup>
          </SelectContent>
        </Select>
      </div>
      {selectedHousehold && (
        <Ranking householdId={selectedHousehold} />
      )}
      <h2 className="text-2xl font-bold mt-4">Quests</h2>
      <h2 className="text-sm text-gray-600 mb-4">
        Complete quests to earn rewards!
      </h2>
      <Quests householdId={selectedHousehold} />
    </div>
  );
}
