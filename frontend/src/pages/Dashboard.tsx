import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import '../styles/Dashboard.css';
import { Navigate } from 'react-router-dom';

// ...Household, DashboardSummary, Expense types...
type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number; // This is calculated in your backend DTO/service
    // Optionally, you can add:
    // createdAt?: string;
    // members?: HouseholdMember[];
};

type Expense = {
    id: string;
    householdId: string;
    createdByUserId: string;
    categoryId: string;
    amount: number;
    date: string; // Use string for ISO date, or Date if you parse it
    description?: string;
    // Optionally, you can add:
    // category?: BudgetCategory;
    // household?: Household;
    // createdByUser?: User;
};

type CategorySummary = {
    categoryId: string;
    categoryName: string;
    budgetLimit: number;
    spent: number;
    remaining: number;
    progress: number;
};

type DashboardSummary = {
    householdId: string;
    year: number;
    month: number;
    categories: CategorySummary[];
    totalBudget: number;
    totalSpent: number;
};

function Dashboard({ onLogout }: { onLogout: () => void }) {
    const { session } = useAuthContext();
    const [households, setHouseholds] = useState<Household[]>([]);
    const [selectedHouseholdId, setSelectedHouseholdId] = useState<string | null>(null);
    const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
    const [expenses, setExpenses] = useState<Expense[]>([]);
    const [loading, setLoading] = useState(false);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [newHouseholdName, setNewHouseholdName] = useState('');
    const [creating, setCreating] = useState(false);
    const [error, setError] = useState<string | null>(null);

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    useEffect(() => {
        if (!session?.user?.id) return;
        fetch(`/api/householdmembers/user/${session.user.id}`)
            .then(res => res.json())
            .then((data: Household[]) => {
                setHouseholds(data);
                if (data.length > 0) setSelectedHouseholdId(data[0].id);
            });
    }, [session]);

    useEffect(() => {
        if (!selectedHouseholdId) return;
        const now = new Date();
        const year = now.getFullYear();
        const month = now.getMonth() + 1;
        setLoading(true);
        Promise.all([
            fetch(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`).then(res => res.json()),
            fetch(`/api/expenses/${selectedHouseholdId}/${year}/${month}`).then(res => res.json())
        ]).then(([dashboardData, expensesData]) => {
            setDashboard(dashboardData);
            setExpenses(expensesData);
            setLoading(false);
        });
    }, [selectedHouseholdId]);

    const handleCreateHousehold = async (e: React.FormEvent) => {
        e.preventDefault();
        setCreating(true);
        setError(null);
        try {
            const res = await fetch('/api/households', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: newHouseholdName, parentId: session.user.id })
            });
            if (!res.ok) throw new Error('Failed to create household');
            setShowCreateForm(false);
            setNewHouseholdName('');
            const data: Household[] = await fetch(`/api/householdmembers/user/${session.user.id}`).then(r => r.json());
            setHouseholds(data);
            if (data.length > 0) setSelectedHouseholdId(data[0].id);
        } catch (err: any) {
            setError(err.message || 'Unknown error');
        } finally {
            setCreating(false);
        }
    };

    return (
        <div className="dashboard-container">
            <h1 className="dashboard-title">Dashboard</h1>
            <button className="signout-btn" onClick={onLogout}>Sign Out</button>

            {households.length === 0 ? (
                <div className="no-household-section">
                    <p>You are not part of any household.</p>
                    {showCreateForm ? (
                        <form className="create-household-form" onSubmit={handleCreateHousehold}>
                            <label>
                                Household Name:
                                <input
                                    type="text"
                                    value={newHouseholdName}
                                    onChange={e => setNewHouseholdName(e.target.value)}
                                    required
                                    className="input"
                                />
                            </label>
                            <div className="form-actions">
                                <button type="submit" className="primary-btn" disabled={creating}>
                                    {creating ? 'Creating...' : 'Create'}
                                </button>
                                <button type="button" className="secondary-btn" onClick={() => setShowCreateForm(false)}>
                                    Cancel
                                </button>
                            </div>
                            {error && <div className="error-msg">{error}</div>}
                        </form>
                    ) : (
                        <button className="primary-btn" onClick={() => setShowCreateForm(true)}>Create Household</button>
                    )}
                </div>
            ) : (
                <div className="household-list-section">
                    <h2>Your Households</h2>
                    <div className="household-list">
                        {households.map(h => (
                            <div key={h.id} className={`household-card${selectedHouseholdId === h.id ? ' selected' : ''}`}>
                                <strong>{h.name}</strong>
                                <div>Members: {h.memberCount}</div>
                                <button className="primary-btn" onClick={() => setSelectedHouseholdId(h.id)}>
                                    View Dashboard
                                </button>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {selectedHouseholdId && households.length > 0 && (
                loading ? (
                    <p>Loading...</p>
                ) : (
                    <div className="dashboard-data-section">
                        <h2>Summary</h2>
                        <pre className="dashboard-summary">{dashboard ? JSON.stringify(dashboard, null, 2) : 'No data'}</pre>
                        <h2>Recent Expenses</h2>
                        <ul className="expenses-list">
                            {expenses.map((e, i) => (
                                <li key={i}>{e.description}: ${e.amount}</li>
                            ))}
                        </ul>
                    </div>
                )
            )}
        </div>
    );
}

export default Dashboard;