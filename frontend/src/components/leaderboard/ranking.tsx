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

interface User {
  id: number;
  name: string;
  avatar?: string;
  rank: number;
  xp: number;
  streak: number;
  goalsCompleted: number;
  badges: string[];
}

interface RankingProps {
  familyUsers: User[];
}

export function Ranking({ familyUsers }: RankingProps) {
  const top3 = familyUsers.slice(0, 3);
  const rest = familyUsers.slice(3);
  const currentUserId = 4; // Replace with your actual current user ID

  return (
    <>
      {/* Top 3 Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
        {top3.map((user) => (
          <div
            key={user.id}
            className={`p-4 sm:p-6 rounded-lg shadow flex flex-col items-center ${
              user.id === currentUserId
                ? "border-2 border-blue-500 bg-blue-50"
                : "bg-white"
            }`}
          >
            <div className="flex justify-between items-center mb-2 w-full">
              {/* Left: Avatar + badge */}
              <div className="relative inline-block">
                <Avatar className="h-10 w-10 rounded-full border">
                  <AvatarImage src={user.avatar} alt={user.name} />
                  <AvatarFallback name={user.name} className="rounded-full" />
                </Avatar>

                <Badge
                  className={`absolute -bottom-1 -right-2 h-5 min-w-5 rounded-full px-1 font-mono tabular-nums ${
                    user.rank === 1
                      ? "bg-yellow-400 text-white" // Gold
                      : user.rank === 2
                      ? "bg-gray-400 text-white" // Silver
                      : user.rank === 3
                      ? "bg-amber-700 text-white" // Bronze
                      : "bg-blue-500 text-white" // Other ranks
                  }`}
                >
                  {user.rank}
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
                {user.name}
              </span>

              <div className="flex items-center gap-1 text-sm text-gray-600">
                <span>🎯 {user.goalsCompleted} quests</span>
                <span>🔥 {user.streak} days</span>
              </div>
              <div className="flex space-x-1 mt-3">
                {user.badges.map((badge, i) => (
                  <span
                    key={i}
                    className="text-xs bg-gray-100 px-2 py-1 rounded border"
                  >
                    {badge}
                  </span>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Remaining Users */}
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
          {rest.map((user) => (
            <TableRow
              key={user.id}
              className={user.id === currentUserId ? "bg-blue-50" : ""}
            >
              <TableCell className="font-medium text-center">
                {user.rank}
              </TableCell>
              <TableCell>
                <div className="flex items-center justify-center gap-2">
                  <Avatar className="h-8 w-8 rounded-full border">
                    <AvatarImage src={user.avatar} alt={user.name} />
                    <AvatarFallback name={user.name} className="rounded-lg" />
                  </Avatar>
                  <span>{user.name}</span>
                </div>
              </TableCell>
              <TableCell className="text-center">{user.xp}</TableCell>
              <TableCell className="text-center">{user.streak} days</TableCell>
              <TableCell className="text-center">
                {user.goalsCompleted} completed
              </TableCell>
              <TableCell>
                <div className="flex gap-1">
                  {user.badges.map((badge, i) => (
                    <span
                      key={i}
                      className="text-xs bg-gray-100 px-2 py-1 rounded border"
                    >
                      {badge}
                    </span>
                  ))}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </>
  );
}
