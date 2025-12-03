import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate } from 'react-router-dom';
import { Download, Receipt as ReceiptIcon, FileText } from 'lucide-react';
import '../styles/Dashboard.css';

type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number;
};

type ExpenseDto = {
    id: string;
    householdId: string;
    userId: string;
    categoryId: string;
    categoryName: string;
    amount: number;
    date: string;
    description?: string;
    receiptId?: string;
};

type ReceiptDto = {
    id: string;
    userId: string;
    householdId?: string;
    imagePath: string;
    originalFileName: string;
    status: number;
    merchantName?: string;
    receiptDate?: string;
    totalAmount?: number;
    uploadedAt: string;
};

type GroupedExpense = {
    receiptId: string;
    receipt: ReceiptDto;
    expenses: ExpenseDto[];
    totalAmount: number;
};

function Expenses() {
    const { session } = useAuthContext();
    const [households, setHouseholds] = useState<Household[]>([]);
    const [selectedHouseholdId, setSelectedHouseholdId] = useState<string | null>(null);
    const [expenses, setExpenses] = useState<ExpenseDto[]>([]);
    const [receipts, setReceipts] = useState<Map<string, ReceiptDto>>(new Map());
    const [loading, setLoading] = useState(false);
    const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
    const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth() + 1);
    const [viewMode, setViewMode] = useState<'expenses' | 'receipts'>('receipts');

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
        setLoading(true);

        fetch(`/api/expenses/${selectedHouseholdId}/${selectedYear}/${selectedMonth}`)
            .then(res => res.json())
            .then(async (expensesData: ExpenseDto[]) => {
                setExpenses(expensesData);

                // Fetch receipt details for expenses that have receiptId
                const receiptIds = [...new Set(expensesData
                    .filter(e => e.receiptId)
                    .map(e => e.receiptId!))];

                const receiptsMap = new Map<string, ReceiptDto>();
                await Promise.all(
                    receiptIds.map(async (receiptId) => {
                        try {
                            const res = await fetch(`/api/receipts/${receiptId}`);
                            if (res.ok) {
                                const receipt = await res.json();
                                receiptsMap.set(receiptId, receipt);
                            }
                        } catch (err) {
                            console.error(`Failed to fetch receipt ${receiptId}:`, err);
                        }
                    })
                );

                setReceipts(receiptsMap);
                setLoading(false);
            })
            .catch(err => {
                console.error('Failed to fetch expenses:', err);
                setLoading(false);
            });
    }, [selectedHouseholdId, selectedYear, selectedMonth]);

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    const manualExpenses = expenses.filter(e => !e.receiptId);
    const receiptExpenses = expenses.filter(e => e.receiptId);

    // Group expenses by receipt
    const groupedByReceipt = (): GroupedExpense[] => {
        const groups: { [receiptId: string]: GroupedExpense } = {};

        receiptExpenses.forEach(expense => {
            const receiptId = expense.receiptId!;
            if (!groups[receiptId]) {
                const receipt = receipts.get(receiptId);
                if (receipt) {
                    groups[receiptId] = {
                        receiptId,
                        receipt,
                        expenses: [],
                        totalAmount: 0
                    };
                }
            }

            if (groups[receiptId]) {
                groups[receiptId].expenses.push(expense);
                groups[receiptId].totalAmount += expense.amount;
            }
        });

        return Object.values(groups);
    };

    const handleDownloadReceipt = (imagePath: string, fileName: string) => {
        // Create a temporary link element to trigger download
        const link = document.createElement('a');
        link.href = imagePath;
        link.download = fileName || 'receipt.jpg';
        link.target = '_blank';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const formatDate = (dateStr: string) => {
        return new Date(dateStr).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    const months = [
        'January', 'February', 'March', 'April', 'May', 'June',
        'July', 'August', 'September', 'October', 'November', 'December'
    ];

    const years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i);

    return (
        <div className="dashboard-container">
            <h1 className="dashboard-title">Expense History</h1>

            {/* Household Selector */}
            {households.length > 0 && (
                <div className="household-selector" style={{ marginBottom: '1rem' }}>
                    <label htmlFor="household-select"><strong>Select Household:</strong></label>
                    <select
                        id="household-select"
                        value={selectedHouseholdId ?? ''}
                        onChange={e => setSelectedHouseholdId(e.target.value)}
                        style={{ marginLeft: '0.5rem', padding: '0.5rem', borderRadius: '4px' }}
                    >
                        {households.map(h => (
                            <option key={h.id} value={h.id}>{h.name}</option>
                        ))}
                    </select>
                </div>
            )}

            {/* Period Selector */}
            <div style={{ marginBottom: '1.5rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <div>
                    <label htmlFor="month-select"><strong>Month:</strong></label>
                    <select
                        id="month-select"
                        value={selectedMonth}
                        onChange={e => setSelectedMonth(Number(e.target.value))}
                        style={{ marginLeft: '0.5rem', padding: '0.5rem', borderRadius: '4px' }}
                    >
                        {months.map((month, idx) => (
                            <option key={idx} value={idx + 1}>{month}</option>
                        ))}
                    </select>
                </div>
                <div>
                    <label htmlFor="year-select"><strong>Year:</strong></label>
                    <select
                        id="year-select"
                        value={selectedYear}
                        onChange={e => setSelectedYear(Number(e.target.value))}
                        style={{ marginLeft: '0.5rem', padding: '0.5rem', borderRadius: '4px' }}
                    >
                        {years.map(year => (
                            <option key={year} value={year}>{year}</option>
                        ))}
                    </select>
                </div>
            </div>

            {/* View Mode Toggle */}
            <div style={{ marginBottom: '1.5rem' }}>
                <button
                    className={viewMode === 'receipts' ? 'primary-btn' : 'secondary-btn'}
                    onClick={() => setViewMode('receipts')}
                    style={{ marginRight: '0.5rem' }}
                >
                    Group by Receipts
                </button>
                <button
                    className={viewMode === 'expenses' ? 'primary-btn' : 'secondary-btn'}
                    onClick={() => setViewMode('expenses')}
                >
                    Individual Expenses
                </button>
            </div>

            {selectedHouseholdId && (
                loading ? (
                    <p>Loading...</p>
                ) : (
                    <div className="dashboard-data-section">
                        {/* Summary */}
                        <div style={{
                            marginBottom: '2rem',
                            padding: '1rem',
                            background: '#f0f4f8',
                            borderRadius: '8px',
                            display: 'flex',
                            gap: '2rem'
                        }}>
                            <div>
                                <strong>Total Expenses:</strong> ${expenses.reduce((sum, e) => sum + e.amount, 0).toFixed(2)}
                            </div>
                            <div>
                                <strong>From Receipts:</strong> {receiptExpenses.length}
                            </div>
                            <div>
                                <strong>Manual Entry:</strong> {manualExpenses.length}
                            </div>
                        </div>

                        {viewMode === 'receipts' ? (
                            <>
                                {/* Receipt-based Expenses */}
                                {groupedByReceipt().length > 0 && (
                                    <div style={{ marginBottom: '2rem' }}>
                                        <h2 style={{ marginBottom: '1rem' }}>Receipt-Based Expenses</h2>
                                        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                                            {groupedByReceipt().map(group => (
                                                <div
                                                    key={group.receiptId}
                                                    style={{
                                                        border: '1px solid #ddd',
                                                        borderRadius: '8px',
                                                        padding: '1.5rem',
                                                        background: '#fff',
                                                        boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                                                    }}
                                                >
                                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '1rem' }}>
                                                        <div style={{ flex: 1 }}>
                                                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                                                                <ReceiptIcon size={20} />
                                                                <h3 style={{ margin: 0 }}>
                                                                    {group.receipt.merchantName || 'Unknown Merchant'}
                                                                </h3>
                                                            </div>
                                                            <div style={{ color: '#666', fontSize: '0.9rem' }}>
                                                                {formatDate(group.receipt.receiptDate || group.receipt.uploadedAt)}
                                                            </div>
                                                            <div style={{ marginTop: '0.5rem', color: '#333' }}>
                                                                <strong>Total:</strong> ${group.totalAmount.toFixed(2)}
                                                            </div>
                                                        </div>
                                                        <button
                                                            className="primary-btn"
                                                            onClick={() => handleDownloadReceipt(
                                                                group.receipt.imagePath,
                                                                group.receipt.originalFileName
                                                            )}
                                                            style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
                                                        >
                                                            <Download size={16} />
                                                            Download Receipt
                                                        </button>
                                                    </div>

                                                    {/* Individual expenses under this receipt */}
                                                    <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #eee' }}>
                                                        <strong style={{ fontSize: '0.9rem', color: '#555' }}>
                                                            Split Across Members:
                                                        </strong>
                                                        <div style={{ marginTop: '0.5rem' }}>
                                                            {group.expenses.map(expense => (
                                                                <div
                                                                    key={expense.id}
                                                                    style={{
                                                                        padding: '0.75rem',
                                                                        background: '#f9f9f9',
                                                                        borderRadius: '4px',
                                                                        marginTop: '0.5rem',
                                                                        display: 'flex',
                                                                        justifyContent: 'space-between'
                                                                    }}
                                                                >
                                                                    <div>
                                                                        <div style={{ fontWeight: '500' }}>
                                                                            {expense.categoryName}
                                                                        </div>
                                                                        {expense.description && (
                                                                            <div style={{ fontSize: '0.85rem', color: '#666', marginTop: '0.25rem' }}>
                                                                                {expense.description}
                                                                            </div>
                                                                        )}
                                                                    </div>
                                                                    <div style={{ fontWeight: '600', color: '#2563eb' }}>
                                                                        ${expense.amount.toFixed(2)}
                                                                    </div>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {/* Manual Expenses */}
                                {manualExpenses.length > 0 && (
                                    <div>
                                        <h2 style={{ marginBottom: '1rem' }}>Manual Expenses</h2>
                                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                            {manualExpenses.map(expense => (
                                                <div
                                                    key={expense.id}
                                                    style={{
                                                        border: '1px solid #e5e7eb',
                                                        borderRadius: '8px',
                                                        padding: '1rem',
                                                        background: '#fff',
                                                        display: 'flex',
                                                        justifyContent: 'space-between',
                                                        alignItems: 'center'
                                                    }}
                                                >
                                                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                                                        <FileText size={20} color="#6b7280" />
                                                        <div>
                                                            <div style={{ fontWeight: '500' }}>
                                                                {expense.categoryName}
                                                            </div>
                                                            {expense.description && (
                                                                <div style={{ fontSize: '0.85rem', color: '#666', marginTop: '0.25rem' }}>
                                                                    {expense.description}
                                                                </div>
                                                            )}
                                                            <div style={{ fontSize: '0.85rem', color: '#999', marginTop: '0.25rem' }}>
                                                                {formatDate(expense.date)}
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div style={{ fontWeight: '600', fontSize: '1.1rem', color: '#2563eb' }}>
                                                        ${expense.amount.toFixed(2)}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {expenses.length === 0 && (
                                    <div style={{ textAlign: 'center', padding: '3rem', color: '#666' }}>
                                        No expenses found for {months[selectedMonth - 1]} {selectedYear}
                                    </div>
                                )}
                            </>
                        ) : (
                            /* Individual Expenses View */
                            <div>
                                <h2 style={{ marginBottom: '1rem' }}>All Expenses</h2>
                                {expenses.length > 0 ? (
                                    <table className="dashboard-categories-table">
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Category</th>
                                                <th>Description</th>
                                                <th>Amount</th>
                                                <th>Type</th>
                                                <th>Action</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {expenses.map(expense => {
                                                const receipt = expense.receiptId ? receipts.get(expense.receiptId) : null;
                                                return (
                                                    <tr key={expense.id}>
                                                        <td>{formatDate(expense.date)}</td>
                                                        <td>{expense.categoryName}</td>
                                                        <td>{expense.description || '-'}</td>
                                                        <td style={{ fontWeight: '600' }}>${expense.amount.toFixed(2)}</td>
                                                        <td>
                                                            {expense.receiptId ? (
                                                                <span style={{
                                                                    background: '#dbeafe',
                                                                    color: '#1e40af',
                                                                    padding: '0.25rem 0.5rem',
                                                                    borderRadius: '4px',
                                                                    fontSize: '0.85rem'
                                                                }}>
                                                                    Receipt
                                                                </span>
                                                            ) : (
                                                                <span style={{
                                                                    background: '#f3f4f6',
                                                                    color: '#4b5563',
                                                                    padding: '0.25rem 0.5rem',
                                                                    borderRadius: '4px',
                                                                    fontSize: '0.85rem'
                                                                }}>
                                                                    Manual
                                                                </span>
                                                            )}
                                                        </td>
                                                        <td>
                                                            {receipt && (
                                                                <button
                                                                    className="secondary-btn"
                                                                    onClick={() => handleDownloadReceipt(
                                                                        receipt.imagePath,
                                                                        receipt.originalFileName
                                                                    )}
                                                                    style={{
                                                                        padding: '0.35rem 0.75rem',
                                                                        fontSize: '0.85rem',
                                                                        display: 'flex',
                                                                        alignItems: 'center',
                                                                        gap: '0.35rem'
                                                                    }}
                                                                >
                                                                    <Download size={14} />
                                                                    Receipt
                                                                </button>
                                                            )}
                                                        </td>
                                                    </tr>
                                                );
                                            })}
                                        </tbody>
                                    </table>
                                ) : (
                                    <div style={{ textAlign: 'center', padding: '3rem', color: '#666' }}>
                                        No expenses found for {months[selectedMonth - 1]} {selectedYear}
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                )
            )}
        </div>
    );
}

export default Expenses;
