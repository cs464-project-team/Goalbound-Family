import { useEffect, useState, useRef } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate } from 'react-router-dom';
import { Download, Receipt as ReceiptIcon, FileText, Calendar, Tag } from 'lucide-react';
import '../styles/Dashboard.css';
import { getApiUrl } from '../config/api';

type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number;
};

type ExpenseDto = {
    id: string;
    householdId: string;
    householdName: string;
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
    items: ReceiptItemDto[];
};

type ReceiptItemDto = {
    id: string;
    itemName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    assignments?: ItemAssignment[];
};

type ItemAssignment = {
    householdMemberName: string;
    assignedQuantity: number;
    totalAmount: number;
};

function Expenses() {
    const { session } = useAuthContext();
    const [households, setHouseholds] = useState<Household[]>([]);
    const [selectedHouseholdId, setSelectedHouseholdId] = useState<string | null>(null);
    const [expenses, setExpenses] = useState<ExpenseDto[]>([]);
    const [receipts, setReceipts] = useState<ReceiptDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
    const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth() + 1);
    const [viewMode, setViewMode] = useState<'expenses' | 'receipts'>('receipts');
    const [expandedReceipts, setExpandedReceipts] = useState<Set<string>>(new Set());
    const [expandedExpenses, setExpandedExpenses] = useState<Set<string>>(new Set());
    const [expenseReceiptDetails, setExpenseReceiptDetails] = useState<Map<string, ReceiptDto>>(new Map());

    useEffect(() => {
        if (!session?.user?.id) return;
        fetch(getApiUrl(`/api/householdmembers/user/${session.user.id}`))
            .then(res => res.json())
            .then((data: Household[]) => {
                setHouseholds(data);
                if (data.length > 0) setSelectedHouseholdId(data[0].id);
            });
    }, [session]);

    // Using a ref to track if the component is still mounted
    const isMounted = useRef(true);

    useEffect(() => {
        return () => {
            isMounted.current = false;
        };
    }, []);

    useEffect(() => {
        if (!selectedHouseholdId || !session?.user?.id) return;
        
        const fetchData = async () => {
            if (isMounted.current) {
                setLoading(true);
            }
            try {
                // Fetch receipts for the household
                const receiptsRes = await fetch(getApiUrl(`/api/receipts/household/${selectedHouseholdId}`));
                const receiptsData: ReceiptDto[] = await receiptsRes.json();

                // Filter receipts by the selected month/year
                const filteredReceipts = receiptsData.filter(r => {
                    const date = new Date(r.receiptDate || r.uploadedAt);
                    return date.getFullYear() === selectedYear && date.getMonth() + 1 === selectedMonth;
                });

                setReceipts(filteredReceipts);

                // Fetch expenses for the current user only
                const expensesRes = await fetch(getApiUrl(`/api/expenses/user/${session.user.id}/${selectedYear}/${selectedMonth}`));
                const expensesData: ExpenseDto[] = await expensesRes.json();
                setExpenses(expensesData);

                setLoading(false);
            } catch (_err) {
                setLoading(false);
            }
        };

        fetchData();
    }, [selectedHouseholdId, selectedYear, selectedMonth, session?.user?.id]);

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    const toggleReceipt = (receiptId: string) => {
        setExpandedReceipts(prev => {
            const newSet = new Set(prev);
            if (newSet.has(receiptId)) {
                newSet.delete(receiptId);
            } else {
                newSet.add(receiptId);
            }
            return newSet;
        });
    };

    const toggleExpense = async (expenseId: string, receiptId: string | undefined) => {
        if (!receiptId) return;

        setExpandedExpenses(prev => {
            const newSet = new Set(prev);
            if (newSet.has(expenseId)) {
                newSet.delete(expenseId);
            } else {
                newSet.add(expenseId);
            }
            return newSet;
        });

        // Fetch receipt details if not already loaded
        if (!expenseReceiptDetails.has(receiptId)) {
            try {
                const res = await fetch(getApiUrl(`/api/receipts/${receiptId}`));
                if (res.ok) {
                    const receiptData: ReceiptDto = await res.json();
                    setExpenseReceiptDetails(prev => new Map(prev).set(receiptId, receiptData));
                }
            } catch (err) {
                console.error(`Failed to fetch receipt ${receiptId}:`, err);
            }
        }
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
                    Receipts
                </button>
                <button
                    className={viewMode === 'expenses' ? 'primary-btn' : 'secondary-btn'}
                    onClick={() => setViewMode('expenses')}
                >
                    My Expenses
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
                            {viewMode === 'receipts' ? (
                                <>
                                    <div>
                                        <strong>Total Receipts:</strong> {receipts.length}
                                    </div>
                                    <div>
                                        <strong>Total Amount:</strong> ${receipts.reduce((sum, r) => sum + (r.totalAmount || 0), 0).toFixed(2)}
                                    </div>
                                </>
                            ) : (
                                <>
                                    <div>
                                        <strong>My Total Expenses:</strong> ${expenses.reduce((sum, e) => sum + e.amount, 0).toFixed(2)}
                                    </div>
                                    <div>
                                        <strong>Number of Expenses:</strong> {expenses.length}
                                    </div>
                                </>
                            )}
                        </div>

                        {viewMode === 'receipts' ? (
                            /* Receipts View */
                            <div>
                                <h2 style={{ marginBottom: '1rem' }}>Household Receipts</h2>
                                {receipts.length > 0 ? (
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                                        {receipts.map(receipt => (
                                            <div
                                                key={receipt.id}
                                                style={{
                                                    border: '1px solid #ddd',
                                                    borderRadius: '8px',
                                                    padding: '1.5rem',
                                                    background: '#fff',
                                                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                                                }}
                                            >
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                                                    <div style={{ flex: 1 }}>
                                                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                                                            <ReceiptIcon size={20} />
                                                            <h3 style={{ margin: 0 }}>
                                                                {receipt.merchantName || 'Unknown Merchant'}
                                                            </h3>
                                                        </div>
                                                        <div style={{ color: '#666', fontSize: '0.9rem' }}>
                                                            {formatDate(receipt.receiptDate || receipt.uploadedAt)}
                                                        </div>
                                                        <div style={{ marginTop: '0.5rem', color: '#333' }}>
                                                            <strong>Total:</strong> ${(receipt.totalAmount || 0).toFixed(2)}
                                                        </div>
                                                        <div style={{ marginTop: '0.25rem', color: '#666', fontSize: '0.85rem' }}>
                                                            {receipt.items?.length || 0} item(s)
                                                        </div>
                                                    </div>
                                                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                                                        <button
                                                            className="secondary-btn"
                                                            onClick={() => toggleReceipt(receipt.id)}
                                                            style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
                                                        >
                                                            {expandedReceipts.has(receipt.id) ? 'Hide' : 'Show'} Items
                                                        </button>
                                                        <button
                                                            className="primary-btn"
                                                            onClick={() => handleDownloadReceipt(
                                                                receipt.imagePath,
                                                                receipt.originalFileName
                                                            )}
                                                            style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
                                                        >
                                                            <Download size={16} />
                                                            Download
                                                        </button>
                                                    </div>
                                                </div>

                                                {/* Expandable Items Section */}
                                                {expandedReceipts.has(receipt.id) && receipt.items && receipt.items.length > 0 && (
                                                    <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #eee' }}>
                                                        <strong style={{ fontSize: '0.9rem', color: '#555', marginBottom: '0.5rem', display: 'block' }}>
                                                            Receipt Items:
                                                        </strong>
                                                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                                            {receipt.items.map(item => (
                                                                <div
                                                                    key={item.id}
                                                                    style={{
                                                                        padding: '0.75rem',
                                                                        background: '#f9f9f9',
                                                                        borderRadius: '4px'
                                                                    }}
                                                                >
                                                                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                                                                        <div>
                                                                            <div style={{ fontWeight: '500' }}>{item.itemName}</div>
                                                                            <div style={{ fontSize: '0.85rem', color: '#666', marginTop: '0.25rem' }}>
                                                                                Qty: {item.quantity} × ${item.unitPrice.toFixed(2)}
                                                                            </div>
                                                                        </div>
                                                                        <div style={{ fontWeight: '600', color: '#2563eb' }}>
                                                                            ${item.totalPrice.toFixed(2)}
                                                                        </div>
                                                                    </div>

                                                                    {/* Show assignments if available */}
                                                                    {item.assignments && item.assignments.length > 0 && (
                                                                        <div style={{ marginTop: '0.5rem', paddingTop: '0.5rem', borderTop: '1px solid #e5e7eb' }}>
                                                                            <div style={{ fontSize: '0.8rem', color: '#666', marginBottom: '0.25rem' }}>
                                                                                Assigned to:
                                                                            </div>
                                                                            {item.assignments.map((assignment, idx) => (
                                                                                <div key={idx} style={{ fontSize: '0.8rem', color: '#555', paddingLeft: '0.5rem' }}>
                                                                                    • {assignment.householdMemberName}: {assignment.assignedQuantity} qty - ${assignment.totalAmount.toFixed(2)}
                                                                                </div>
                                                                            ))}
                                                                        </div>
                                                                    )}
                                                                </div>
                                                            ))}
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div style={{ textAlign: 'center', padding: '3rem', color: '#666' }}>
                                        No receipts found for {months[selectedMonth - 1]} {selectedYear}
                                    </div>
                                )}
                            </div>
                        ) : (
                            /* My Expenses View */
                            <div>
                                <h2 style={{ marginBottom: '1rem' }}>My Expenses</h2>
                                {expenses.length > 0 ? (
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                                        {expenses
                                            .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
                                            .map(expense => {
                                                const receipt = expense.receiptId ? receipts.find(r => r.id === expense.receiptId) : null;
                                                return (
                                                    <div
                                                        key={expense.id}
                                                        style={{
                                                            border: '1px solid #e5e7eb',
                                                            borderRadius: '8px',
                                                            padding: '1.25rem',
                                                            background: '#fff',
                                                            boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
                                                            transition: 'box-shadow 0.2s ease',
                                                            cursor: 'default'
                                                        }}
                                                        onMouseEnter={(e) => {
                                                            e.currentTarget.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';
                                                        }}
                                                        onMouseLeave={(e) => {
                                                            e.currentTarget.style.boxShadow = '0 1px 3px rgba(0,0,0,0.06)';
                                                        }}
                                                    >
                                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                                                            <div style={{ flex: 1 }}>
                                                                {/* Category and Type Badge */}
                                                                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
                                                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                                                        <Tag size={18} color="#6b7280" />
                                                                        <span style={{ fontWeight: '500' }}>
                                                                            {expense.categoryName}
                                                                        </span>
                                                                    </div>
                                                                    {expense.receiptId ? (
                                                                        <span style={{
                                                                            background: '#dbeafe',
                                                                            color: '#1e40af',
                                                                            padding: '0.25rem 0.65rem',
                                                                            borderRadius: '12px',
                                                                            fontSize: '0.8rem',
                                                                            fontWeight: '500',
                                                                            display: 'flex',
                                                                            alignItems: 'center',
                                                                            gap: '0.35rem'
                                                                        }}>
                                                                            <ReceiptIcon size={12} />
                                                                            Receipt
                                                                        </span>
                                                                    ) : (
                                                                        <span style={{
                                                                            background: '#f3f4f6',
                                                                            color: '#4b5563',
                                                                            padding: '0.25rem 0.65rem',
                                                                            borderRadius: '12px',
                                                                            fontSize: '0.8rem',
                                                                            fontWeight: '500',
                                                                            display: 'flex',
                                                                            alignItems: 'center',
                                                                            gap: '0.35rem'
                                                                        }}>
                                                                            <FileText size={12} />
                                                                            Manual
                                                                        </span>
                                                                    )}
                                                                </div>

                                                                {/* Household Name */}
                                                                <div style={{
                                                                    fontSize: '0.85rem',
                                                                    color: '#6b7280',
                                                                    marginBottom: '0.5rem'
                                                                }}>
                                                                    Household: <span style={{ fontWeight: '500' }}>{expense.householdName}</span>
                                                                </div>

                                                                {/* Description */}
                                                                {expense.description && (
                                                                    <div style={{
                                                                        fontSize: '0.85rem',
                                                                        color: '#666',
                                                                        marginTop: '0.25rem'
                                                                    }}>
                                                                        {expense.description}
                                                                    </div>
                                                                )}

                                                                {/* Date */}
                                                                <div style={{
                                                                    display: 'flex',
                                                                    alignItems: 'center',
                                                                    gap: '0.35rem',
                                                                    color: '#999',
                                                                    fontSize: '0.85rem',
                                                                    marginTop: '0.25rem'
                                                                }}>
                                                                    <Calendar size={14} />
                                                                    <span>{formatDate(expense.date)}</span>
                                                                </div>
                                                            </div>

                                                            {/* Amount and Action */}
                                                            <div style={{
                                                                display: 'flex',
                                                                flexDirection: 'column',
                                                                alignItems: 'flex-end',
                                                                gap: '0.75rem',
                                                                marginLeft: '1.5rem'
                                                            }}>
                                                                <div style={{
                                                                    fontWeight: '600',
                                                                    fontSize: '1.1rem',
                                                                    color: '#2563eb'
                                                                }}>
                                                                    ${expense.amount.toFixed(2)}
                                                                </div>
                                                                {expense.receiptId && (
                                                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                                                        <button
                                                                            className="primary-btn"
                                                                            onClick={() => toggleExpense(expense.id, expense.receiptId)}
                                                                            style={{
                                                                                padding: '0.5rem 1rem',
                                                                                fontSize: '0.85rem',
                                                                                display: 'flex',
                                                                                alignItems: 'center',
                                                                                gap: '0.5rem',
                                                                                whiteSpace: 'nowrap'
                                                                            }}
                                                                        >
                                                                            <ReceiptIcon size={16} />
                                                                            {expandedExpenses.has(expense.id) ? 'Hide' : 'View'} Receipt
                                                                        </button>
                                                                        {receipt && (
                                                                            <button
                                                                                className="secondary-btn"
                                                                                onClick={() => handleDownloadReceipt(
                                                                                    receipt.imagePath,
                                                                                    receipt.originalFileName
                                                                                )}
                                                                                style={{
                                                                                    padding: '0.5rem 1rem',
                                                                                    fontSize: '0.85rem',
                                                                                    display: 'flex',
                                                                                    alignItems: 'center',
                                                                                    gap: '0.5rem',
                                                                                    whiteSpace: 'nowrap'
                                                                                }}
                                                                            >
                                                                                <Download size={16} />
                                                                                Download
                                                                            </button>
                                                                        )}
                                                                    </div>
                                                                )}
                                                            </div>
                                                        </div>

                                                        {/* Expandable Receipt Details */}
                                                        {expense.receiptId && expandedExpenses.has(expense.id) && expenseReceiptDetails.has(expense.receiptId) && (
                                                            <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid #e5e7eb' }}>
                                                                {(() => {
                                                                    const receiptDetail = expenseReceiptDetails.get(expense.receiptId!);
                                                                    if (!receiptDetail) return null;

                                                                    return (
                                                                        <div>
                                                                            <div style={{ marginBottom: '1rem' }}>
                                                                                <div style={{ fontSize: '0.9rem', fontWeight: '600', marginBottom: '0.5rem' }}>
                                                                                    Receipt Details
                                                                                </div>
                                                                                <div style={{ fontSize: '0.85rem', color: '#666' }}>
                                                                                    <div><strong>Merchant:</strong> {receiptDetail.merchantName || 'Unknown'}</div>
                                                                                    <div><strong>Date:</strong> {formatDate(receiptDetail.receiptDate || receiptDetail.uploadedAt)}</div>
                                                                                    <div><strong>Total:</strong> ${(receiptDetail.totalAmount || 0).toFixed(2)}</div>
                                                                                </div>
                                                                            </div>

                                                                            {receiptDetail.items && receiptDetail.items.length > 0 && (
                                                                                <div>
                                                                                    <strong style={{ fontSize: '0.9rem', color: '#555', marginBottom: '0.5rem', display: 'block' }}>
                                                                                        Receipt Items:
                                                                                    </strong>
                                                                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                                                                                        {receiptDetail.items.map(item => (
                                                                                            <div
                                                                                                key={item.id}
                                                                                                style={{
                                                                                                    padding: '0.75rem',
                                                                                                    background: '#f9f9f9',
                                                                                                    borderRadius: '4px'
                                                                                                }}
                                                                                            >
                                                                                                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                                                                                                    <div>
                                                                                                        <div style={{ fontWeight: '500' }}>{item.itemName}</div>
                                                                                                        <div style={{ fontSize: '0.85rem', color: '#666', marginTop: '0.25rem' }}>
                                                                                                            Qty: {item.quantity} × ${item.unitPrice.toFixed(2)}
                                                                                                        </div>
                                                                                                    </div>
                                                                                                    <div style={{ fontWeight: '600', color: '#2563eb' }}>
                                                                                                        ${item.totalPrice.toFixed(2)}
                                                                                                    </div>
                                                                                                </div>

                                                                                                {item.assignments && item.assignments.length > 0 && (
                                                                                                    <div style={{ marginTop: '0.5rem', paddingTop: '0.5rem', borderTop: '1px solid #e5e7eb' }}>
                                                                                                        <div style={{ fontSize: '0.8rem', color: '#666', marginBottom: '0.25rem' }}>
                                                                                                            Assigned to:
                                                                                                        </div>
                                                                                                        {item.assignments.map((assignment, idx) => (
                                                                                                            <div key={idx} style={{ fontSize: '0.8rem', color: '#555', paddingLeft: '0.5rem' }}>
                                                                                                                • {assignment.householdMemberName}: {assignment.assignedQuantity} qty - ${assignment.totalAmount.toFixed(2)}
                                                                                                            </div>
                                                                                                        ))}
                                                                                                    </div>
                                                                                                )}
                                                                                            </div>
                                                                                        ))}
                                                                                    </div>
                                                                                </div>
                                                                            )}
                                                                        </div>
                                                                    );
                                                                })()}
                                                            </div>
                                                        )}
                                                    </div>
                                                );
                                            })}
                                    </div>
                                ) : (
                                    <div style={{ textAlign: 'center', padding: '3rem', color: '#666' }}>
                                        No expenses found for you in {months[selectedMonth - 1]} {selectedYear}
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
