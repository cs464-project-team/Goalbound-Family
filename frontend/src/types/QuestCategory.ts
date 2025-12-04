export type QuestCategory = "dashboard" | "expense" | "reciept" | "budget" | "household" | "others";

export const categoryIcons: Record<QuestCategory, string> = {
    dashboard: "📊",
    expense: "💸",
    reciept: "🧾",          // spelling kept as you wrote it
    budget: "📉",
    household: "🏠",
    others: "✨",
  };