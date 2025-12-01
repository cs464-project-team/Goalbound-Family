import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthContext } from '../context/AuthProvider';
import '../styles/Dashboard.css';
import { Navigate } from 'react-router-dom';


type BudgetCategory = {
    id: string;
    name: string;
};



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


type HouseholdBudget = {
    id: string;
    householdId: string;
    categoryId: string;
    categoryName: string;
    limit: number;
    year: number;
    month: number;
};

function Dashboard() {
    // Invite link state
    const [inviteLink, setInviteLink] = useState<string | null>(null);
    const [generatingInvite, setGeneratingInvite] = useState(false);
    const [inviteError, setInviteError] = useState<string | null>(null);
    const [linkCopied, setLinkCopied] = useState(false);

    // Generate invite link handler
    const handleGenerateInvite = async () => {
        if (!selectedHouseholdId || !session?.user?.id) return;
        setGeneratingInvite(true);
        setInviteError(null);
        setLinkCopied(false);
        try {
            const res = await fetch('/api/invitations', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    householdId: selectedHouseholdId,
                    invitedByUserId: session.user.id
                })
            });
            if (!res.ok) throw new Error('Failed to generate invite');
            const data = await res.json();
            const link = `${window.location.origin}/accept-invite?token=${data.token}`;
            setInviteLink(link);
        } catch (err: unknown) {
            setInviteError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setGeneratingInvite(false);
        }
    };

    // Copy link handler
    const handleCopyLink = () => {
        if (inviteLink) {
            navigator.clipboard.writeText(inviteLink);
            setLinkCopied(true);
            setTimeout(() => setLinkCopied(false), 2000);
        }
    };
    const navigate = useNavigate();
    const [showAddCategory, setShowAddCategory] = useState(false);
    const [newCategoryName, setNewCategoryName] = useState('');
    const [addingCategory, setAddingCategory] = useState(false);
    const [categoryError, setCategoryError] = useState<string | null>(null);
    const [budgets, setBudgets] = useState<HouseholdBudget[]>([]);
    const [categories, setCategories] = useState<BudgetCategory[]>([]);
    const [budgetInputs, setBudgetInputs] = useState<{ [categoryId: string]: string }>({});
    const [budgetLoading, setBudgetLoading] = useState<{ [categoryId: string]: boolean }>({});
    const [budgetError, setBudgetError] = useState<{ [categoryId: string]: string | null }>({});

    const handleAddCategory = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedHouseholdId) return;
        setAddingCategory(true);
        setCategoryError(null);
        try {
            const res = await fetch('/api/budgetcategories', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ householdId: selectedHouseholdId, name: newCategoryName })
            });
            if (!res.ok) throw new Error('Failed to add category');
            setShowAddCategory(false);
            setNewCategoryName('');
            // Optionally, refresh dashboard data to show new category
            const now = new Date();
            const year = now.getFullYear();
            const month = now.getMonth() + 1;
            const dashboardData = await fetch(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`).then(res => res.json());
            setDashboard(dashboardData);
        } catch (err: unknown) {
            setCategoryError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setAddingCategory(false);
        }
    };
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
            fetch(`/api/expenses/${selectedHouseholdId}/${year}/${month}`).then(res => res.json()),
            fetch(`/api/householdbudgets/${selectedHouseholdId}/${year}/${month}`).then(res => res.json()),
            fetch(`/api/budgetcategories/${selectedHouseholdId}`).then(res => res.json())
        ]).then(([dashboardData, expensesData, budgetsData, categoriesData]) => {
            setDashboard(dashboardData);
            setExpenses(expensesData);
            setBudgets(budgetsData);
            setCategories(categoriesData);
            setLoading(false);
        });
    }, [selectedHouseholdId]);

    if (!session) {
        return <Navigate to="/auth" replace />;
    }
    const handleSetBudget = async (categoryId: string) => {
        if (!selectedHouseholdId) return;
        const now = new Date();
        const year = now.getFullYear();
        const month = now.getMonth() + 1;
        setBudgetLoading(prev => ({ ...prev, [categoryId]: true }));
        setBudgetError(prev => ({ ...prev, [categoryId]: null }));
        try {
            const limit = parseFloat(budgetInputs[categoryId]);
            if (isNaN(limit) || limit < 0) throw new Error('Invalid limit');
            const res = await fetch('/api/householdbudgets', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    householdId: selectedHouseholdId,
                    categoryId,
                    limit,
                    year,
                    month
                })
            });
            if (!res.ok) throw new Error('Failed to set budget');
            // Refresh budgets
            const budgetsData = await fetch(`/api/householdbudgets/${selectedHouseholdId}/${year}/${month}`).then(res => res.json());
            setBudgets(budgetsData);
            setBudgetInputs(prev => ({ ...prev, [categoryId]: '' }));
        } catch (err: unknown) {
            setBudgetError(prev => ({ ...prev, [categoryId]: err instanceof Error ? err.message : 'Unknown error' }));
        } finally {
            setBudgetLoading(prev => ({ ...prev, [categoryId]: false }));
        }
    };

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
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setCreating(false);
        }
    };



    return (
        <div className="dashboard-container">
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <h1 className="dashboard-title">Dashboard</h1>
                <button className="primary-btn" onClick={() => navigate('/expenses')}>
                    Go to Expenses
                </button>
            </div>

            {/* Show create household form as a modal/overlay or main content if active */}
            {showCreateForm && (
                <div className="create-household-section" style={{ marginBottom: '2rem' }}>
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
                </div>
            )}

            {/* Household Selector */}
            {!showCreateForm && households.length > 0 && (
                <div className="household-selector" style={{ marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
                    <div>
                        <label htmlFor="household-select"><strong>Select Family:</strong></label>
                        <select
                            id="household-select"
                            value={selectedHouseholdId ?? ''}
                            onChange={e => setSelectedHouseholdId(e.target.value)}
                            style={{ marginLeft: '0.5rem', padding: '0.3rem', borderRadius: '4px' }}
                        >
                            {households.map(h => (
                                <option key={h.id} value={h.id}>{h.name}</option>
                            ))}
                        </select>
                    </div>
                    <button className="primary-btn" onClick={() => setShowCreateForm(true)}>
                        Create Household
                    </button>
                </div>
            )}

            {!showCreateForm && households.length === 0 ? (
                <div className="no-household-section">
                    <p>You are not part of any household.</p>
                </div>
            ) : null}

            {!showCreateForm && households.length > 0 && (
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
                        {/* Invite Link Section */}
                        <div className="invite-section" style={{ marginBottom: '2rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '8px', background: '#f9f9f9' }}>
                            <h3>Invite Members</h3>
                            <p>Generate an invite link to share with others to join this household.</p>
                            {!inviteLink ? (
                                <button
                                    className="primary-btn"
                                    onClick={handleGenerateInvite}
                                    disabled={generatingInvite}
                                >
                                    {generatingInvite ? 'Generating...' : 'Generate Invite Link'}
                                </button>
                            ) : (
                                <div style={{ marginTop: '1rem' }}>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                                        <input
                                            type="text"
                                            value={inviteLink}
                                            readOnly
                                            className="input"
                                            style={{ flex: 1, padding: '0.5rem', fontSize: '0.9rem' }}
                                        />
                                        <button
                                            className="primary-btn"
                                            onClick={handleCopyLink}
                                            style={{ minWidth: '100px' }}
                                        >
                                            {linkCopied ? '✓ Copied!' : 'Copy Link'}
                                        </button>
                                    </div>
                                    <button
                                        className="secondary-btn"
                                        onClick={() => setInviteLink(null)}
                                        style={{ fontSize: '0.85rem' }}
                                    >
                                        Generate New Link
                                    </button>
                                </div>
                            )}
                            {inviteError && <div className="error-msg" style={{ marginTop: '0.5rem' }}>{inviteError}</div>}
                        </div>
                        <h2>Summary</h2>
                        {dashboard ? (
                            <>
                                <div className="dashboard-summary">
                                    <div><strong>Year:</strong> {dashboard.year}</div>
                                    <div><strong>Month:</strong> {dashboard.month}</div>
                                    <div><strong>Total Budget:</strong> ${dashboard.totalBudget}</div>
                                    <div><strong>Total Spent:</strong> ${dashboard.totalSpent}</div>
                                </div>
                                <div style={{ marginTop: '1rem' }}>
                                    {showAddCategory ? (
                                        <form onSubmit={handleAddCategory} className="add-category-form">
                                            <input
                                                type="text"
                                                value={newCategoryName}
                                                onChange={e => setNewCategoryName(e.target.value)}
                                                placeholder="Category name"
                                                required
                                                className="input"
                                            />
                                            <button type="submit" className="primary-btn" disabled={addingCategory}>
                                                {addingCategory ? 'Adding...' : 'Add Category'}
                                            </button>
                                            <button type="button" className="secondary-btn" onClick={() => setShowAddCategory(false)}>
                                                Cancel
                                            </button>
                                            {categoryError && <div className="error-msg">{categoryError}</div>}
                                        </form>
                                    ) : (
                                        <button className="primary-btn" onClick={() => setShowAddCategory(true)}>
                                            Add Category
                                        </button>
                                    )}
                                </div>
                                <div className="categories-section" style={{ marginTop: '2rem' }}>
                                    <h3>Budget Categories</h3>
                                    <div className="categories-list" style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem' }}>
                                        {categories.length === 0 ? (
                                            <div>No categories yet.</div>
                                        ) : (
                                            categories.map(cat => {
                                                const budget = budgets.find(b => b.categoryId === cat.id);
                                                return (
                                                    <div key={cat.id} className="category-box" style={{ border: '1px solid #ddd', borderRadius: '8px', padding: '1rem', minWidth: '200px', background: '#fafbfc', boxShadow: '0 1px 4px rgba(0,0,0,0.04)' }}>
                                                        <strong>{cat.name}</strong>
                                                        <div>Limit: {budget ? `$${budget.limit}` : <span style={{ color: '#888' }}>Not set</span>}</div>
                                                        <form onSubmit={e => { e.preventDefault(); handleSetBudget(cat.id); }} style={{ marginTop: '0.5rem' }}>
                                                            <input
                                                                type="number"
                                                                min="0"
                                                                step="0.01"
                                                                value={budgetInputs[cat.id] ?? ''}
                                                                onChange={e => setBudgetInputs(prev => ({ ...prev, [cat.id]: e.target.value }))}
                                                                placeholder="Set budget limit"
                                                                className="input"
                                                                style={{ width: '100px', marginRight: '0.5rem' }}
                                                            />
                                                            <button type="submit" className="primary-btn" disabled={budgetLoading[cat.id] || !budgetInputs[cat.id] || isNaN(Number(budgetInputs[cat.id]))}>
                                                                {budgetLoading[cat.id] ? 'Saving...' : (budget ? 'Update' : 'Set')}
                                                            </button>
                                                        </form>
                                                        {budgetError[cat.id] && <div className="error-msg">{budgetError[cat.id]}</div>}
                                                    </div>
                                                );
                                            })
                                        )}
                                    </div>
                                </div>
                                <div style={{ marginTop: '2rem' }}>
                                    <h3>Categories Table</h3>
                                    <table className="dashboard-categories-table">
                                        <thead>
                                            <tr>
                                                <th>Category</th>
                                                <th>Budget Limit</th>
                                                <th>Spent</th>
                                                <th>Remaining</th>
                                                <th>Progress</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {dashboard.categories.map(cat => (
                                                <tr key={cat.categoryId}>
                                                    <td>{cat.categoryName}</td>
                                                    <td>${cat.budgetLimit}</td>
                                                    <td>${cat.spent}</td>
                                                    <td>${cat.remaining}</td>
                                                    <td>
                                                        <progress value={cat.progress} max={1} style={{ width: '80px' }} />
                                                        {(cat.progress * 100).toFixed(0)}%
                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            </>
                        ) : (
                            'No data'
                        )}
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