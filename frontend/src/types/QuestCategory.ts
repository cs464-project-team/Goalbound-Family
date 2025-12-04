export type QuestCategory = "dashboard" | "expense" | "receipt" | "budget" | "household" | "others";

export const categoryIcons: Record<QuestCategory, string> = {
    dashboard: "📊",
    expense: "💸",
    receipt: "🧾",          
    budget: "📉",
    household: "🏠",
    others: "✨",
  };