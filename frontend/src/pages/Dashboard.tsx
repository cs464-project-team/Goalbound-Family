import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import '../styles/Dashboard.css';
import { Navigate, Link } from 'react-router-dom';
import { getApiUrl } from '../config/api';

type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number;
};

type Expense = {
    id: string;
    householdId: string;
    createdByUserId: string;
    categoryId: string;
    amount: number;
    date: string;
    description?: string;
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

function Dashboard() {
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

    // Invite link state
    const [inviteLink, setInviteLink] = useState<string | null>(null);
    const [generatingInvite, setGeneratingInvite] = useState(false);
    const [inviteError, setInviteError] = useState<string | null>(null);
    const [linkCopied, setLinkCopied] = useState(false);

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
            fetch(getApiUrl(`/api/dashboard/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json()),
            fetch(getApiUrl(`/api/expenses/${selectedHouseholdId}/${year}/${month}`)).then(res => res.json())
        ]).then(([dashboardData, expensesData]) => {
            setDashboard(dashboardData);
            setExpenses(expensesData);
            setLoading(false);
        });
    }, [selectedHouseholdId]);

    const handleGenerateInvite = async () => {
        if (!selectedHouseholdId || !session?.user?.id) return;
        setGeneratingInvite(true);
        setInviteError(null);
        setLinkCopied(false);
        try {
            const res = await fetch(getApiUrl('/api/invitations'), {
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

    const handleCopyLink = () => {
        if (inviteLink) {
            navigator.clipboard.writeText(inviteLink);
            setLinkCopied(true);
            setTimeout(() => setLinkCopied(false), 2000);
        }
    };

    const handleCreateHousehold = async (e: React.FormEvent) => {
        e.preventDefault();
        setCreating(true);
        setError(null);
        try {
            const res = await fetch(getApiUrl('/api/households'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: newHouseholdName, parentId: session?.user.id })
            });
            if (!res.ok) throw new Error('Failed to create household');
            setShowCreateForm(false);
            setNewHouseholdName('');
            const data: Household[] = await fetch(getApiUrl(`/api/householdmembers/user/${session?.user.id}`)).then(r => r.json());
            setHouseholds(data);
            if (data.length > 0) setSelectedHouseholdId(data[0].id);
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setCreating(false);
        }
    };

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    const now = new Date();
    const monthName = now.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });

    return (
        <div className="dashboard-container" style={{ maxWidth: '1200px', margin: '0 auto', padding: '2rem', minHeight: '100vh' }}>
            <div style={{ marginBottom: '2.5rem', paddingBottom: '1rem' }}>
                <h1 className="dashboard-title" style={{ fontSize: '2.75rem', fontWeight: '700', marginBottom: '0.75rem', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', letterSpacing: '-0.5px' }}>Dashboard</h1>
                <p style={{ color: '#64748b', fontSize: '1.15rem', fontWeight: '400' }}>Welcome back! Here's your financial overview for {monthName}</p>
            </div>

            {showCreateForm && (
                <div className="create-household-section" style={{
                    marginBottom: '2rem',
                    padding: '2rem',
                    background: 'white',
                    borderRadius: '16px',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.08), 0 2px 4px rgba(0,0,0,0.04)',
                    border: '1px solid rgba(0,0,0,0.05)',
                    transition: 'all 0.3s ease'
                }}>
                    <h2 style={{ marginBottom: '1rem' }}>Create New Household</h2>
                    <form className="create-household-form" onSubmit={handleCreateHousehold}>
                        <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                            Household Name:
                            <input
                                type="text"
                                value={newHouseholdName}
                                onChange={e => setNewHouseholdName(e.target.value)}
                                required
                                className="input"
                                style={{ display: 'block', width: '100%', marginTop: '0.5rem', padding: '0.75rem', borderRadius: '8px', border: '1px solid #ddd' }}
                            />
                        </label>
                        <div className="form-actions" style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                            <button type="submit" className="primary-btn" disabled={creating} style={{ padding: '0.75rem 1.5rem', borderRadius: '10px', transition: 'all 0.2s ease', boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)' }}>
                                {creating ? 'Creating...' : 'Create'}
                            </button>
                            <button type="button" className="secondary-btn" onClick={() => setShowCreateForm(false)} style={{ padding: '0.75rem 1.5rem', borderRadius: '10px', transition: 'all 0.2s ease' }}>
                                Cancel
                            </button>
                        </div>
                        {error && <div className="error-msg" style={{ marginTop: '1rem', color: '#e53e3e' }}>{error}</div>}
                    </form>
                </div>
            )}

            {!showCreateForm && households.length > 0 && (
                <div className="household-selector" style={{
                    marginBottom: '2rem',
                    padding: '1.75rem',
                    background: 'white',
                    borderRadius: '16px',
                    boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                    border: '1px solid rgba(0,0,0,0.05)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    flexWrap: 'wrap',
                    gap: '1rem',
                    transition: 'box-shadow 0.3s ease'
                }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                        <label htmlFor="household-select" style={{ fontWeight: '600', fontSize: '1rem' }}>Select Family:</label>
                        <select
                            id="household-select"
                            value={selectedHouseholdId ?? ''}
                            onChange={e => setSelectedHouseholdId(e.target.value)}
                            style={{
                                padding: '0.75rem 1rem',
                                borderRadius: '10px',
                                border: '2px solid #e2e8f0',
                                fontSize: '1rem',
                                minWidth: '200px',
                                transition: 'all 0.2s ease',
                                cursor: 'pointer',
                                background: 'white'
                            }}
                        >
                            {households.map(h => (
                                <option key={h.id} value={h.id}>{h.name} ({h.memberCount} members)</option>
                            ))}
                        </select>
                    </div>
                    <button
                        type="button"
                        className="primary-btn"
                        onClick={() => setShowCreateForm(true)}
                        style={{ padding: '0.75rem 1.5rem', borderRadius: '10px', whiteSpace: 'nowrap', transition: 'all 0.2s ease', boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)' }}
                    >
                        Create New Household
                    </button>
                </div>
            )}

            {!showCreateForm && households.length === 0 && (
                <div className="no-household-section" style={{
                    padding: '3rem',
                    background: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.08)',
                    textAlign: 'center'
                }}>
                    <p style={{ fontSize: '1.1rem', marginBottom: '1.5rem' }}>You are not part of any household yet.</p>
                    <button type="button" className="primary-btn" onClick={() => setShowCreateForm(true)} style={{ padding: '0.75rem 2rem', borderRadius: '8px' }}>
                        Create Your First Household
                    </button>
                </div>
            )}

            {selectedHouseholdId && households.length > 0 && (
                loading ? (
                    <div style={{ textAlign: 'center', padding: '3rem', fontSize: '1.1rem', color: '#666' }}>
                        <p>Loading your dashboard...</p>
                    </div>
                ) : (
                    <div className="dashboard-data-section">
                        {/* Summary Cards */}
                        {dashboard && (
                            <div style={{
                                display: 'grid',
                                gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
                                gap: '1.5rem',
                                marginBottom: '2rem'
                            }}>
                                <div style={{
                                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                                    padding: '2rem',
                                    borderRadius: '16px',
                                    color: 'white',
                                    boxShadow: '0 8px 16px rgba(102, 126, 234, 0.25), 0 2px 4px rgba(0,0,0,0.08)',
                                    transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                                    cursor: 'default'
                                }}
                                onMouseEnter={(e) => {
                                    e.currentTarget.style.transform = 'translateY(-4px)';
                                    e.currentTarget.style.boxShadow = '0 12px 24px rgba(102, 126, 234, 0.3), 0 4px 8px rgba(0,0,0,0.1)';
                                }}
                                onMouseLeave={(e) => {
                                    e.currentTarget.style.transform = 'translateY(0)';
                                    e.currentTarget.style.boxShadow = '0 8px 16px rgba(102, 126, 234, 0.25), 0 2px 4px rgba(0,0,0,0.08)';
                                }}>
                                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Total Budget</div>
                                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold' }}>${dashboard.totalBudget.toFixed(2)}</div>
                                    <div style={{ fontSize: '0.85rem', opacity: 0.8, marginTop: '0.5rem' }}>for {monthName}</div>
                                </div>

                                <div style={{
                                    background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                                    padding: '2rem',
                                    borderRadius: '16px',
                                    color: 'white',
                                    boxShadow: '0 8px 16px rgba(245, 87, 108, 0.25), 0 2px 4px rgba(0,0,0,0.08)',
                                    transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                                    cursor: 'default'
                                }}
                                onMouseEnter={(e) => {
                                    e.currentTarget.style.transform = 'translateY(-4px)';
                                    e.currentTarget.style.boxShadow = '0 12px 24px rgba(245, 87, 108, 0.3), 0 4px 8px rgba(0,0,0,0.1)';
                                }}
                                onMouseLeave={(e) => {
                                    e.currentTarget.style.transform = 'translateY(0)';
                                    e.currentTarget.style.boxShadow = '0 8px 16px rgba(245, 87, 108, 0.25), 0 2px 4px rgba(0,0,0,0.08)';
                                }}>
                                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Total Spent</div>
                                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold' }}>${dashboard.totalSpent.toFixed(2)}</div>
                                    <div style={{ fontSize: '0.85rem', opacity: 0.8, marginTop: '0.5rem' }}>
                                        {dashboard.totalBudget > 0 ? `${((dashboard.totalSpent / dashboard.totalBudget) * 100).toFixed(0)}% of budget` : ''}
                                    </div>
                                </div>

                                <div style={{
                                    background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                                    padding: '2rem',
                                    borderRadius: '16px',
                                    color: 'white',
                                    boxShadow: '0 8px 16px rgba(79, 172, 254, 0.25), 0 2px 4px rgba(0,0,0,0.08)',
                                    transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                                    cursor: 'default'
                                }}
                                onMouseEnter={(e) => {
                                    e.currentTarget.style.transform = 'translateY(-4px)';
                                    e.currentTarget.style.boxShadow = '0 12px 24px rgba(79, 172, 254, 0.3), 0 4px 8px rgba(0,0,0,0.1)';
                                }}
                                onMouseLeave={(e) => {
                                    e.currentTarget.style.transform = 'translateY(0)';
                                    e.currentTarget.style.boxShadow = '0 8px 16px rgba(79, 172, 254, 0.25), 0 2px 4px rgba(0,0,0,0.08)';
                                }}>
                                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Remaining</div>
                                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold' }}>
                                        ${(dashboard.totalBudget - dashboard.totalSpent).toFixed(2)}
                                    </div>
                                    <div style={{ fontSize: '0.85rem', opacity: 0.8, marginTop: '0.5rem' }}>available to spend</div>
                                </div>
                            </div>
                        )}

                        {/* Category Breakdown */}
                        {dashboard && dashboard.categories.length > 0 && (
                            <div style={{
                                background: 'white',
                                padding: '2rem',
                                borderRadius: '16px',
                                boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                                border: '1px solid rgba(0,0,0,0.05)',
                                marginBottom: '2rem',
                                transition: 'box-shadow 0.3s ease'
                            }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                                    <h2 style={{ fontSize: '1.5rem', fontWeight: '600', margin: 0 }}>Budget Breakdown by Category</h2>
                                    <Link to="/budgets" style={{
                                        padding: '0.5rem 1rem',
                                        background: '#667eea',
                                        color: 'white',
                                        borderRadius: '10px',
                                        textDecoration: 'none',
                                        fontSize: '0.9rem',
                                        transition: 'all 0.2s ease',
                                        boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)',
                                        fontWeight: '500'
                                    }}>
                                        Manage Budgets
                                    </Link>
                                </div>
                                <div style={{ display: 'grid', gap: '1rem' }}>
                                    {dashboard.categories.map(cat => {
                                                        // Calculate display percentage: if no budget set but has spending, show 100%
                                                        const displayProgress = cat.budgetLimit === 0 || cat.budgetLimit === null || cat.budgetLimit === undefined
                                                            ? (cat.spent > 0 ? 100 : 0)
                                                            : Math.min((cat.spent / cat.budgetLimit) * 100, 100);

                                                        const progressRatio = displayProgress / 100;
                                                        const progressColor = progressRatio > 0.9 ? '#e53e3e' : progressRatio > 0.7 ? '#f59e0b' : '#38a169';

                                        return (
                                            <div key={cat.categoryId} style={{
                                                padding: '1.5rem',
                                                background: '#f7fafc',
                                                borderRadius: '8px',
                                                borderLeft: `4px solid ${progressColor}`
                                            }}>
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                                                    <h3 style={{ fontSize: '1.1rem', fontWeight: '600', margin: 0 }}>{cat.categoryName}</h3>
                                                    <span style={{ fontSize: '0.9rem', color: '#666' }}>
                                                        ${cat.spent.toFixed(2)} / ${cat.budgetLimit > 0 ? cat.budgetLimit.toFixed(2) : 'Not Set'}
                                                    </span>
                                                </div>
                                                <div style={{ marginBottom: '0.5rem' }}>
                                                    <div style={{
                                                        width: '100%',
                                                        height: '10px',
                                                        background: '#e2e8f0',
                                                        borderRadius: '5px',
                                                        overflow: 'hidden'
                                                    }}>
                                                        <div style={{
                                                            width: `${displayProgress}%`,
                                                            height: '100%',
                                                            background: progressColor,
                                                            transition: 'width 0.3s ease'
                                                        }} />
                                                    </div>
                                                </div>
                                                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: '#666' }}>
                                                    <span>{displayProgress.toFixed(0)}% used</span>
                                                    <span style={{ color: cat.remaining < 0 ? '#e53e3e' : '#38a169', fontWeight: '600' }}>
                                                        {cat.budgetLimit > 0 ? (
                                                            <>{Math.abs(cat.remaining).toFixed(2)} {cat.remaining < 0 ? 'over' : 'remaining'}</>
                                                        ) : (
                                                            <>Budget not set</>
                                                        )}
                                                    </span>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}

                        {/* Recent Expenses */}
                        <div style={{
                            background: 'white',
                            padding: '2rem',
                            borderRadius: '16px',
                            boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                            border: '1px solid rgba(0,0,0,0.05)',
                            marginBottom: '2rem',
                            transition: 'box-shadow 0.3s ease'
                        }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                                <h2 style={{ fontSize: '1.5rem', fontWeight: '600', margin: 0 }}>Recent Expenses</h2>
                                <Link to="/expenses" style={{
                                    padding: '0.5rem 1rem',
                                    background: '#667eea',
                                    color: 'white',
                                    borderRadius: '10px',
                                    textDecoration: 'none',
                                    fontSize: '0.9rem',
                                    transition: 'all 0.2s ease',
                                    boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)',
                                    fontWeight: '500'
                                }}>
                                    View All
                                </Link>
                            </div>
                            {expenses.length === 0 ? (
                                <p style={{ color: '#666', textAlign: 'center', padding: '2rem' }}>No expenses recorded yet this month.</p>
                            ) : (
                                <div style={{ display: 'grid', gap: '0.75rem' }}>
                                    {expenses.slice(0, 5).map((expense, i) => (
                                        <div key={i} style={{
                                            display: 'flex',
                                            justifyContent: 'space-between',
                                            alignItems: 'center',
                                            padding: '1rem',
                                            background: '#f7fafc',
                                            borderRadius: '8px'
                                        }}>
                                            <div>
                                                <div style={{ fontWeight: '500', marginBottom: '0.25rem' }}>
                                                    {expense.description || 'Expense'}
                                                </div>
                                                <div style={{ fontSize: '0.85rem', color: '#666' }}>
                                                    {new Date(expense.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                                                </div>
                                            </div>
                                            <div style={{ fontSize: '1.1rem', fontWeight: '600', color: '#333' }}>
                                                ${expense.amount.toFixed(2)}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Invite Members */}
                        <div style={{
                            background: 'white',
                            padding: '2rem',
                            borderRadius: '16px',
                            boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                            border: '1px solid rgba(0,0,0,0.05)',
                            transition: 'box-shadow 0.3s ease'
                        }}>
                            <h2 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1rem' }}>Invite Family Members</h2>
                            <p style={{ color: '#666', marginBottom: '1.5rem' }}>Share an invite link to add members to this household.</p>
                            {!inviteLink ? (
                                <button
                                    type="button"
                                    className="primary-btn"
                                    onClick={handleGenerateInvite}
                                    disabled={generatingInvite}
                                    style={{ padding: '0.75rem 1.5rem', borderRadius: '8px' }}
                                >
                                    {generatingInvite ? 'Generating Link...' : 'Generate Invite Link'}
                                </button>
                            ) : (
                                <div>
                                    <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
                                        <input
                                            type="text"
                                            value={inviteLink}
                                            readOnly
                                            className="input"
                                            style={{
                                                flex: 1,
                                                padding: '0.75rem',
                                                borderRadius: '8px',
                                                border: '1px solid #ddd',
                                                fontSize: '0.9rem',
                                                background: '#f7fafc'
                                            }}
                                        />
                                        <button
                                            type="button"
                                            className="primary-btn"
                                            onClick={handleCopyLink}
                                            style={{ padding: '0.75rem 1.5rem', borderRadius: '8px', minWidth: '120px' }}
                                        >
                                            {linkCopied ? 'Copied!' : 'Copy Link'}
                                        </button>
                                    </div>
                                    <button
                                        type="button"
                                        className="secondary-btn"
                                        onClick={handleGenerateInvite}
                                        disabled={generatingInvite}
                                        style={{ padding: '0.5rem 1rem', borderRadius: '8px', fontSize: '0.85rem' }}
                                    >
                                        {generatingInvite ? 'Generating...' : 'Generate New Link'}
                                    </button>
                                </div>
                            )}
                            {inviteError && <div className="error-msg" style={{ marginTop: '1rem', color: '#e53e3e' }}>{inviteError}</div>}
                        </div>
                    </div>
                )
            )}
        </div>
    );
}

export default Dashboard;
