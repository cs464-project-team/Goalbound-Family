import type { MemberQuestDto } from '../../types/MemberQuestDto';

interface TimedQuestsProps {
  quest: MemberQuestDto | null;
}

export function TimedQuests({ quest }: TimedQuestsProps) {
  if (!quest) return null;

  return (
    <div className="p-6 mb-6 bg-gradient-to-r from-orange-50 to-red-50 rounded-lg shadow border border-orange-200">
      <div className="flex items-center gap-2 mb-3">
        <span className="text-2xl">⏱️</span>
        <h3 className="text-xl font-bold text-orange-900">Timed Quest</h3>
      </div>
      <div className="bg-white p-4 rounded-lg">
        <h4 className="font-semibold text-lg mb-2">{quest.title}</h4>
        <p className="text-sm text-gray-600 mb-3">{quest.description}</p>
        <div className="flex justify-between items-center">
          <span className="text-sm text-gray-500">
            Progress: {quest.progress} / {quest.target}
          </span>
          <span className="font-bold text-orange-600">{quest.xpReward} XP</span>
        </div>
      </div>
    </div>
  );
}
