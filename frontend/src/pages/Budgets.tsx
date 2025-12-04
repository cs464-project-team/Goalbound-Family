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
        <div className="dashboard-container" style={{ maxWidth: '1400px', margin: '0 auto', padding: '2rem', minHeight: '100vh' }}>
            <div style={{ marginBottom: '2.5rem', paddingBottom: '1rem' }}>
                <h1 className="dashboard-title" style={{ fontSize: '2.75rem', fontWeight: '700', marginBottom: '0.75rem', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', letterSpacing: '-0.5px' }}>Budget Management</h1>
                <p style={{ color: '#64748b', fontSize: '1.15rem', fontWeight: '400' }}>Set and monitor your household budget limits</p>
            </div>

            {/* Household Selector */}
            {households.length > 0 && (
                <div className="household-selector" style={{
                    marginBottom: '2rem',
                    padding: '1.75rem',
                    background: 'white',
                    borderRadius: '16px',
                    boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                    border: '1px solid rgba(0,0,0,0.05)'
                }}>
                    <label htmlFor="household-select" style={{ fontWeight: '600', fontSize: '1rem', marginRight: '1rem' }}>Select Family:</label>
                    <select
                        id="household-select"
                        value={selectedHouseholdId ?? ''}
                        onChange={e => setSelectedHouseholdId(e.target.value)}
                        style={{
                            padding: '0.75rem 1rem',
                            borderRadius: '10px',
                            border: '2px solid #e2e8f0',
                            fontSize: '1rem',
                            minWidth: '250px',
                            transition: 'all 0.2s ease',
                            cursor: 'pointer',
                            background: 'white'
                        }}
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
                    <div style={{
                        marginBottom: '2rem',
                        padding: '1.5rem',
                        background: 'white',
                        borderRadius: '16px',
                        boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                        border: '1px solid rgba(0,0,0,0.05)'
                    }}>
                        {showAddCategory ? (
                            <form onSubmit={handleAddCategory} className="add-category-form" style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start', flexWrap: 'wrap' }}>
                                <input
                                    type="text"
                                    value={newCategoryName}
                                    onChange={e => setNewCategoryName(e.target.value)}
                                    placeholder="Category name"
                                    required
                                    className="input"
                                    style={{
                                        padding: '0.75rem 1rem',
                                        borderRadius: '10px',
                                        border: '2px solid #e2e8f0',
                                        fontSize: '1rem',
                                        flex: '1',
                                        minWidth: '250px',
                                        transition: 'border-color 0.2s ease'
                                    }}
                                />
                                <button type="submit" className="primary-btn" disabled={addingCategory} style={{
                                    padding: '0.75rem 1.5rem',
                                    borderRadius: '10px',
                                    transition: 'all 0.2s ease',
                                    boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)'
                                }}>
                                    {addingCategory ? 'Adding...' : 'Add Category'}
                                </button>
                                <button type="button" className="secondary-btn" onClick={() => setShowAddCategory(false)} style={{
                                    padding: '0.75rem 1.5rem',
                                    borderRadius: '10px',
                                    transition: 'all 0.2s ease'
                                }}>
                                    Cancel
                                </button>
                                {categoryError && <div className="error-msg" style={{ width: '100%', marginTop: '0.5rem' }}>{categoryError}</div>}
                            </form>
                        ) : (
                            <button type="button" className="primary-btn" onClick={() => setShowAddCategory(true)} style={{
                                padding: '0.75rem 1.5rem',
                                borderRadius: '10px',
                                transition: 'all 0.2s ease',
                                boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)'
                            }}>
                                + Add New Category
                            </button>
                        )}
                    </div>

                    {/* Budget Categories List */}
                    <div className="categories-section" style={{ marginBottom: '2.5rem' }}>
                        <h2 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1.5rem', color: '#1e293b' }}>Budget Categories</h2>
                        <div className="categories-list" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1.25rem', marginTop: '1rem' }}>
                            {categories.length === 0 ? (
                                <div>No categories yet. Add one to get started!</div>
                            ) : (
                                categories.map(cat => {
                                    const budget = budgets.find(b => b.categoryId === cat.id);
                                    return (
                                        <div key={cat.id} className="category-box" style={{
                                            border: '1px solid rgba(0,0,0,0.08)',
                                            borderRadius: '16px',
                                            padding: '1.75rem',
                                            background: 'white',
                                            boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                                            transition: 'all 0.2s ease',
                                            cursor: 'default'
                                        }}
                                        onMouseEnter={(e) => {
                                            e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.1), 0 2px 6px rgba(0,0,0,0.06)';
                                            e.currentTarget.style.transform = 'translateY(-2px)';
                                        }}
                                        onMouseLeave={(e) => {
                                            e.currentTarget.style.boxShadow = '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)';
                                            e.currentTarget.style.transform = 'translateY(0)';
                                        }}>
                                            <h3 style={{ margin: '0 0 1.25rem 0', fontSize: '1.2rem', fontWeight: '600', color: '#1e293b' }}>{cat.name}</h3>
                                            <div style={{ marginBottom: '1.25rem', padding: '1rem', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', borderRadius: '10px', color: 'white' }}>
                                                <div style={{ fontSize: '0.85rem', opacity: 0.9, marginBottom: '0.25rem' }}>Current Limit</div>
                                                <div style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>
                                                    {budget ? `$${budget.limit.toFixed(2)}` : 'Not set'}
                                                </div>
                                            </div>
                                            <form onSubmit={e => { e.preventDefault(); handleSetBudget(cat.id); }} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                                <input
                                                    type="number"
                                                    min="0"
                                                    step="0.01"
                                                    value={budgetInputs[cat.id] ?? ''}
                                                    onChange={e => setBudgetInputs(prev => ({ ...prev, [cat.id]: e.target.value }))}
                                                    placeholder="Enter budget limit"
                                                    className="input"
                                                    style={{
                                                        padding: '0.75rem 1rem',
                                                        borderRadius: '10px',
                                                        border: '2px solid #e2e8f0',
                                                        fontSize: '1rem',
                                                        transition: 'border-color 0.2s ease'
                                                    }}
                                                />
                                                <button
                                                    type="submit"
                                                    className="primary-btn"
                                                    disabled={budgetLoading[cat.id] || !budgetInputs[cat.id] || isNaN(Number(budgetInputs[cat.id]))}
                                                    style={{
                                                        width: '100%',
                                                        padding: '0.75rem',
                                                        borderRadius: '10px',
                                                        transition: 'all 0.2s ease',
                                                        boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)',
                                                        fontWeight: '500'
                                                    }}
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
                        <div style={{
                            marginTop: '3rem',
                            padding: '2rem',
                            background: 'white',
                            borderRadius: '16px',
                            boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                            border: '1px solid rgba(0,0,0,0.05)'
                        }}>
                            <h2 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1.5rem', color: '#1e293b' }}>Budget Overview</h2>
                            <div style={{ overflowX: 'auto' }}>
                                <table className="dashboard-categories-table" style={{ width: '100%', marginTop: '1rem', borderCollapse: 'collapse' }}>
                                <thead>
                                    <tr style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white' }}>
                                        <th style={{ padding: '1rem 1.25rem', textAlign: 'left', fontWeight: '600', fontSize: '0.95rem' }}>Category</th>
                                        <th style={{ padding: '1rem 1.25rem', textAlign: 'right', fontWeight: '600', fontSize: '0.95rem' }}>Budget Limit</th>
                                        <th style={{ padding: '1rem 1.25rem', textAlign: 'right', fontWeight: '600', fontSize: '0.95rem' }}>Spent</th>
                                        <th style={{ padding: '1rem 1.25rem', textAlign: 'right', fontWeight: '600', fontSize: '0.95rem' }}>Remaining</th>
                                        <th style={{ padding: '1rem 1.25rem', textAlign: 'center', fontWeight: '600', fontSize: '0.95rem' }}>Progress</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {dashboard.categories.map((cat, idx) => (
                                        <tr key={cat.categoryId} style={{
                                            borderBottom: '1px solid #e2e8f0',
                                            background: idx % 2 === 0 ? '#ffffff' : '#f8fafc',
                                            transition: 'background 0.2s ease'
                                        }}
                                        onMouseEnter={(e) => {
                                            e.currentTarget.style.background = '#f1f5f9';
                                        }}
                                        onMouseLeave={(e) => {
                                            e.currentTarget.style.background = idx % 2 === 0 ? '#ffffff' : '#f8fafc';
                                        }}>
                                            <td style={{ padding: '1.25rem', fontWeight: '500' }}>{cat.categoryName}</td>
                                            <td style={{ padding: '1.25rem', textAlign: 'right', fontWeight: '500' }}>${cat.budgetLimit.toFixed(2)}</td>
                                            <td style={{ padding: '1.25rem', textAlign: 'right', color: cat.spent > cat.budgetLimit ? '#e53e3e' : '#333', fontWeight: '500' }}>
                                                ${cat.spent.toFixed(2)}
                                            </td>
                                            <td style={{ padding: '1.25rem', textAlign: 'right', color: cat.remaining < 0 ? '#e53e3e' : '#38a169', fontWeight: '600' }}>
                                                ${cat.remaining.toFixed(2)}
                                            </td>
                                            <td style={{ padding: '1.25rem', textAlign: 'center' }}>
                                                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', justifyContent: 'center' }}>
                                                    <div style={{
                                                        width: '120px',
                                                        height: '10px',
                                                        background: '#e2e8f0',
                                                        borderRadius: '5px',
                                                        overflow: 'hidden'
                                                    }}>
                                                        <div style={{
                                                            width: `${Math.min(cat.progress * 100, 100)}%`,
                                                            height: '100%',
                                                            background: cat.progress > 0.9 ? '#e53e3e' : cat.progress > 0.7 ? '#f59e0b' : '#38a169',
                                                            transition: 'width 0.3s ease'
                                                        }} />
                                                    </div>
                                                    <span style={{ fontWeight: '600', fontSize: '0.9rem' }}>{(cat.progress * 100).toFixed(0)}%</span>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

export default Budgets;
