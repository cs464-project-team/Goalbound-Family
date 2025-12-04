// Leaderboard.tsx
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { useAuth } from '../../hooks/useAuth'
import type { HouseholdMemberDto } from "../../types/HouseholdMemberDto";

export function Ranking({ householdMembers }: { householdMembers: HouseholdMemberDto[] }) {
  const { session } = useAuth();
  const userId = session?.user.id || null;

  // Sort members by XP descending
  const sortedMembers = [...householdMembers].sort((a, b) => b.xp - a.xp);

  // Split top 3 and the rest
  const top3 = sortedMembers.slice(0, 3);
  const rest = sortedMembers.slice(3);

  // Guard rendering
  if (!householdMembers || householdMembers.length === 0) {
    return <p>No members found for this household.</p>;
  }

  return (
    <>
      {/* Top 3 Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        {top3 &&
          top3.map((user, index) => (
            <div
              key={user.id}
              className={`p-6 rounded-lg shadow flex flex-col items-center ${
                user.userId === userId
                  ? "border-2 border-blue-500 bg-blue-50"
                  : "bg-white"
              }`}
            >
              <div className="flex justify-between items-center mb-2 w-full">
                {/* Left: Avatar + badge */}
                <div className="relative inline-block">
                  <Avatar className="h-10 w-10 rounded-full border">
                    <AvatarImage src={user.avatar} alt={user.firstName} />
                    <AvatarFallback
                      name={user.firstName}
                      className="rounded-full"
                    />
                  </Avatar>

                  <Badge
                    className={`absolute -bottom-1 -right-2 h-5 min-w-5 rounded-full px-1 font-mono tabular-nums ${
                      index === 0
                        ? "bg-yellow-400 text-white" // Gold
                        : index === 1
                        ? "bg-gray-400 text-white" // Silver
                        : index === 2
                        ? "bg-amber-700 text-white" // Bronze
                        : "bg-blue-500 text-white" // Other ranks
                    }`}
                  >
                    {index + 1}
                  </Badge>
                </div>

                {/* Right: XP */}
                <div className="flex items-center gap-1 text-sm font-semibold text-gray-700">
                  <span>⚡</span>
                  <span>{user.xp} XP</span>
                </div>
              </div>

              <div className="flex flex-col items-start w-full">
                <span className="text-lg font-semibold justify-start">
                  {user.firstName} {user.lastName}
                </span>

                <div className="flex items-center gap-1 text-sm text-gray-600">
                  <span>🎯 {user.questsCompleted} quests</span>
                  <span>🔥 {user.streak} days</span>
                </div>
                <div className="flex space-x-1 mt-3">
                {user.badges.map((badge, i) => (
                  <span
                    key={i}
                    className="text-xs bg-gray-100 px-2 py-1 rounded border"
                  >
                    {badge.name}
                  </span>
                ))}
              </div>
              </div>
            </div>
          ))}
      </div>

      {/* Remaining Users */}
      {rest.length > 0 && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[100px] text-center">Rank</TableHead>
              <TableHead className="text-center">User</TableHead>
              <TableHead className="text-center">XP</TableHead>
              <TableHead className="text-center">Streaks</TableHead>
              <TableHead className="text-center">Quests</TableHead>
              <TableHead>Badge</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {rest.map((user, index) => (
              <TableRow
                key={user.id}
                className={user.userId === userId ? "bg-blue-50" : ""}
              >
                <TableCell className="font-medium text-center">
                  {index + 4}
                </TableCell>
                <TableCell>
                  <div className="flex items-center justify-center gap-2">
                    <Avatar className="h-8 w-8 rounded-full border">
                      <AvatarImage src={user.avatar} alt={user.firstName} />
                      <AvatarFallback
                        name={user.firstName}
                        className="rounded-lg"
                      />
                    </Avatar>
                    <span>
                      {user.firstName} {user.lastName}
                    </span>
                  </div>
                </TableCell>
                <TableCell className="text-center">{user.xp}</TableCell>
                <TableCell className="text-center">
                  {user.streak} days
                </TableCell>
                <TableCell className="text-center">
                  {user.questsCompleted} completed
                </TableCell>
                <TableCell>
                <div className="flex gap-1">
                  {user.badges.map((badge, i) => (
                    <span
                      key={i}
                      className="text-xs bg-gray-100 px-2 py-1 rounded border"
                    >
                      {badge.name}
                    </span>
                  ))}
                </div>
              </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </>
  );
}
