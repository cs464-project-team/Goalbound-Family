import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate } from 'react-router-dom';
import { getApiUrl } from '../config/api';
import '../styles/Dashboard.css';

type BudgetCategory = {
    id: string;
    name: string;
};

type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number;
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

function Budgets() {
    const { session } = useAuthContext();
    const [households, setHouseholds] = useState<Household[]>([]);
    const [selectedHouseholdId, setSelectedHouseholdId] = useState<string | null>(null);
    const [categories, setCategories] = useState<BudgetCategory[]>([]);
    const [budgets, setBudgets] = useState<HouseholdBudget[]>([]);
    const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
    const [loading, setLoading] = useState(false);

    // Add category state
    const [showAddCategory, setShowAddCategory] = useState(false);
    const [newCategoryName, setNewCategoryName] = useState('');
    const [addingCategory, setAddingCategory] = useState(false);
    const [categoryError, setCategoryError] = useState<string | null>(null);

    // Budget inputs state
    const [budgetInputs, setBudgetInputs] = useState<{ [categoryId: string]: string }>({});
    const [budgetLoading, setBudgetLoading] = useState<{ [categoryId: string]: boolean }>({});
    const [budgetError, setBudgetError] = useState<{ [categoryId: string]: string | null }>({});

    useEffect(() => {
        if (!session?.user?.id) return;
        fetch(getApiUrl(`/api/householdmembers/user/${session.user.id}`))
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
            fetch(getApiUrl(`/api/budgets/categories/${selectedHouseholdId}`)).then(res => res.json()),
            fetch(getApiUrl(`/api/householdbudgets/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json()),
            fetch(getApiUrl(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json())
        ]).then(([categoriesData, budgetsData, dashboardData]) => {
            setCategories(categoriesData);
            setBudgets(budgetsData);
            setDashboard(dashboardData);
            setLoading(false);
        });
    }, [selectedHouseholdId]);

    const handleAddCategory = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedHouseholdId) return;
        setAddingCategory(true);
        setCategoryError(null);
        try {
            const res = await fetch(getApiUrl('/api/budgets/categories'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ householdId: selectedHouseholdId, name: newCategoryName })
            });
            if (!res.ok) throw new Error('Failed to add category');
            setShowAddCategory(false);
            setNewCategoryName('');
            // Refresh data
            const now = new Date();
            const year = now.getFullYear();
            const month = now.getMonth() + 1;
            const [categoriesData, dashboardData] = await Promise.all([
                fetch(getApiUrl(`/api/budgets/categories/${selectedHouseholdId}`)).then(res => res.json()),
                fetch(getApiUrl(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json())
            ]);
            setCategories(categoriesData);
            setDashboard(dashboardData);
        } catch (err: unknown) {
            setCategoryError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setAddingCategory(false);
        }
    };

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
            const res = await fetch(getApiUrl('/api/householdbudgets'), {
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
            // Refresh budgets and dashboard
            const [budgetsData, dashboardData] = await Promise.all([
                fetch(getApiUrl(`/api/householdbudgets/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json()),
                fetch(getApiUrl(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json())
            ]);
            setBudgets(budgetsData);
            setDashboard(dashboardData);
            setBudgetInputs(prev => ({ ...prev, [categoryId]: '' }));
        } catch (err: unknown) {
            setBudgetError(prev => ({ ...prev, [categoryId]: err instanceof Error ? err.message : 'Unknown error' }));
        } finally {
            setBudgetLoading(prev => ({ ...prev, [categoryId]: false }));
        }
    };

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    return (
        <div className="dashboard-container">
            <h1 className="dashboard-title">Budget Management</h1>

            {/* Household Selector */}
            {households.length > 0 && (
                <div className="household-selector" style={{ marginBottom: '2rem' }}>
                    <label htmlFor="household-select"><strong>Select Family:</strong></label>
                    <select
                        id="household-select"
                        value={selectedHouseholdId ?? ''}
                        onChange={e => setSelectedHouseholdId(e.target.value)}
                        style={{ marginLeft: '0.5rem', padding: '0.5rem', borderRadius: '4px', border: '1px solid #ddd' }}
                    >
                        {households.map(h => (
                            <option key={h.id} value={h.id}>{h.name}</option>
                        ))}
                    </select>
                </div>
            )}

            {households.length === 0 ? (
                <div className="no-household-section">
                    <p>You are not part of any household. Please create one from the Dashboard.</p>
                </div>
            ) : loading ? (
                <p>Loading...</p>
            ) : (
                <div className="budgets-content">
                    {/* Add Category Section */}
                    <div style={{ marginBottom: '2rem' }}>
                        {showAddCategory ? (
                            <form onSubmit={handleAddCategory} className="add-category-form" style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start' }}>
                                <input
                                    type="text"
                                    value={newCategoryName}
                                    onChange={e => setNewCategoryName(e.target.value)}
                                    placeholder="Category name"
                                    required
                                    className="input"
                                    style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #ddd' }}
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
                            <button type="button" className="primary-btn" onClick={() => setShowAddCategory(true)}>
                                Add New Category
                            </button>
                        )}
                    </div>

                    {/* Budget Categories List */}
                    <div className="categories-section">
                        <h2>Budget Categories</h2>
                        <div className="categories-list" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '1rem', marginTop: '1rem' }}>
                            {categories.length === 0 ? (
                                <div>No categories yet. Add one to get started!</div>
                            ) : (
                                categories.map(cat => {
                                    const budget = budgets.find(b => b.categoryId === cat.id);
                                    return (
                                        <div key={cat.id} className="category-box" style={{
                                            border: '1px solid #ddd',
                                            borderRadius: '8px',
                                            padding: '1.5rem',
                                            background: '#fafbfc',
                                            boxShadow: '0 2px 4px rgba(0,0,0,0.08)',
                                            transition: 'box-shadow 0.2s'
                                        }}>
                                            <h3 style={{ margin: '0 0 1rem 0', fontSize: '1.1rem' }}>{cat.name}</h3>
                                            <div style={{ marginBottom: '1rem', color: '#555' }}>
                                                Current Limit: {budget ? <strong>${budget.limit.toFixed(2)}</strong> : <span style={{ color: '#888' }}>Not set</span>}
                                            </div>
                                            <form onSubmit={e => { e.preventDefault(); handleSetBudget(cat.id); }} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                                <input
                                                    type="number"
                                                    min="0"
                                                    step="0.01"
                                                    value={budgetInputs[cat.id] ?? ''}
                                                    onChange={e => setBudgetInputs(prev => ({ ...prev, [cat.id]: e.target.value }))}
                                                    placeholder="Enter budget limit"
                                                    className="input"
                                                    style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #ddd' }}
                                                />
                                                <button
                                                    type="submit"
                                                    className="primary-btn"
                                                    disabled={budgetLoading[cat.id] || !budgetInputs[cat.id] || isNaN(Number(budgetInputs[cat.id]))}
                                                    style={{ width: '100%' }}
                                                >
                                                    {budgetLoading[cat.id] ? 'Saving...' : (budget ? 'Update Limit' : 'Set Limit')}
                                                </button>
                                            </form>
                                            {budgetError[cat.id] && <div className="error-msg" style={{ marginTop: '0.5rem' }}>{budgetError[cat.id]}</div>}
                                        </div>
                                    );
                                })
                            )}
                        </div>
                    </div>

                    {/* Categories Summary Table */}
                    {dashboard && dashboard.categories.length > 0 && (
                        <div style={{ marginTop: '3rem' }}>
                            <h2>Budget Overview</h2>
                            <table className="dashboard-categories-table" style={{ width: '100%', marginTop: '1rem', borderCollapse: 'collapse' }}>
                                <thead>
                                    <tr style={{ background: '#f5f5f5', borderBottom: '2px solid #ddd' }}>
                                        <th style={{ padding: '1rem', textAlign: 'left' }}>Category</th>
                                        <th style={{ padding: '1rem', textAlign: 'right' }}>Budget Limit</th>
                                        <th style={{ padding: '1rem', textAlign: 'right' }}>Spent</th>
                                        <th style={{ padding: '1rem', textAlign: 'right' }}>Remaining</th>
                                        <th style={{ padding: '1rem', textAlign: 'center' }}>Progress</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {dashboard.categories.map(cat => (
                                        <tr key={cat.categoryId} style={{ borderBottom: '1px solid #eee' }}>
                                            <td style={{ padding: '1rem' }}>{cat.categoryName}</td>
                                            <td style={{ padding: '1rem', textAlign: 'right' }}>${cat.budgetLimit.toFixed(2)}</td>
                                            <td style={{ padding: '1rem', textAlign: 'right', color: cat.spent > cat.budgetLimit ? '#e53e3e' : '#333' }}>
                                                ${cat.spent.toFixed(2)}
                                            </td>
                                            <td style={{ padding: '1rem', textAlign: 'right', color: cat.remaining < 0 ? '#e53e3e' : '#38a169' }}>
                                                ${cat.remaining.toFixed(2)}
                                            </td>
                                            <td style={{ padding: '1rem', textAlign: 'center' }}>
                                                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', justifyContent: 'center' }}>
                                                    <progress
                                                        value={cat.progress}
                                                        max={1}
                                                        style={{
                                                            width: '100px',
                                                            height: '8px',
                                                            accentColor: cat.progress > 0.9 ? '#e53e3e' : cat.progress > 0.7 ? '#f59e0b' : '#38a169'
                                                        }}
                                                    />
                                                    <span>{(cat.progress * 100).toFixed(0)}%</span>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

export default Budgets;
