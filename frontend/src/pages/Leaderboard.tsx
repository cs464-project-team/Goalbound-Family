// Leaderboard.tsx
import { Ranking } from "@/components/leaderboard/ranking";
import { Quests } from "@/components/leaderboard/quests-section";

import { useAuth } from '../hooks/useAuth'
import { useState, useEffect } from "react";
import { Link } from 'react-router-dom';

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
import { authenticatedFetch } from '../services/authService';
import type { HouseholdDto } from "../types/HouseholdDto";
import type { HouseholdMemberDto } from "../types/HouseholdMemberDto";
import type { MemberQuestDto } from "../types/MemberQuestDto";

export default function Leaderboard() {
  const { session } = useAuth()
  const userId = session?.user.id || null;
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
        const res = await authenticatedFetch(
          getApiUrl(`api/households/user/${userId}`)
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
        const res = await authenticatedFetch(
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
        const res = await authenticatedFetch(
          getApiUrl(`api/households/${selectedHousehold}/members`)
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
        const res = await authenticatedFetch(
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

  if (!households.length) {
    return (
      <div className="px-6 py-8">
        <div style={{
          maxWidth: '600px',
          margin: '0 auto',
          padding: '3rem 2rem',
          background: 'white',
          borderRadius: '16px',
          boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
          border: '1px solid rgba(0,0,0,0.05)',
          textAlign: 'center'
        }}>
          <h2 style={{
            fontSize: '1.5rem',
            fontWeight: '700',
            marginBottom: '1rem',
            color: '#1e293b'
          }}>
            No Household Found
          </h2>
          <p style={{
            fontSize: '1.1rem',
            marginBottom: '1.5rem',
            color: '#64748b',
            lineHeight: '1.6'
          }}>
            You need to be part of a household to view the leaderboard and quests.
          </p>
          <Link
            to="/dashboard"
            style={{
              display: 'inline-block',
              padding: '0.75rem 2rem',
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              color: 'white',
              borderRadius: '10px',
              textDecoration: 'none',
              fontWeight: '600',
              transition: 'all 0.2s ease',
              boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)',
              fontSize: '1rem'
            }}
          >
            Go to Dashboard to Create a Household
          </Link>
        </div>
      </div>
    );
  }

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
            setHouseholdMembers={setHouseholdMembers}
          />
        </>
      )}
    </div>
  );
}
