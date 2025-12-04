import { useEffect, useState, useRef } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate, Link } from 'react-router-dom';
import { Download, Receipt as ReceiptIcon, FileText, Calendar, Tag } from 'lucide-react';
import '../styles/Dashboard.css';
import { getApiUrl } from '../config/api';
import { authenticatedFetch } from '../services/authService';

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
    const [viewMode, setViewMode] = useState<'expenses' | 'receipts'>('expenses');
    const [expandedReceipts, setExpandedReceipts] = useState<Set<string>>(new Set());
    const [expandedExpenses, setExpandedExpenses] = useState<Set<string>>(new Set());
    const [expenseReceiptDetails, setExpenseReceiptDetails] = useState<Map<string, ReceiptDto>>(new Map());

    useEffect(() => {
        if (!session?.user?.id) return;
        authenticatedFetch(getApiUrl(`/api/householdmembers/user/${session.user.id}`))
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
        if (!session?.user?.id) return;

        const fetchData = async () => {
            if (isMounted.current) {
                setLoading(true);
            }
            try {
                // Fetch receipts only when in receipts view and a household is selected
                if (viewMode === 'receipts' && selectedHouseholdId) {
                    const receiptsRes = await authenticatedFetch(getApiUrl(`/api/receipts/household/${selectedHouseholdId}`));
                    const receiptsData: ReceiptDto[] = await receiptsRes.json();

                    // Filter receipts by the selected month/year
                    const filteredReceipts = receiptsData.filter(r => {
                        const date = new Date(r.receiptDate || r.uploadedAt);
                        return date.getFullYear() === selectedYear && date.getMonth() + 1 === selectedMonth;
                    });

                    setReceipts(filteredReceipts);
                }

                // Fetch expenses for the current user (always fetch for expenses view)
                if (viewMode === 'expenses') {
                    const expensesRes = await authenticatedFetch(getApiUrl(`/api/expenses/user/${session.user.id}/${selectedYear}/${selectedMonth}`));
                    const expensesData: ExpenseDto[] = await expensesRes.json();
                    setExpenses(expensesData);
                }

                setLoading(false);
            } catch (_err) {
                setLoading(false);
            }
        };

        fetchData();
    }, [viewMode, selectedHouseholdId, selectedYear, selectedMonth, session?.user?.id]);

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
                const res = await authenticatedFetch(getApiUrl(`/api/receipts/${receiptId}`));
                if (res.ok) {
                    const receiptData: ReceiptDto = await res.json();
                    setExpenseReceiptDetails(prev => new Map(prev).set(receiptId, receiptData));
                }
            } catch (err) {
                console.error(`Failed to fetch receipt ${receiptId}:`, err);
            }
        }
    };

    const handleDownloadReceipt = async (imagePath: string, fileName: string) => {
        try {
            // Fetch the image from Supabase
            const response = await fetch(imagePath);
            if (!response.ok) {
                throw new Error('Failed to fetch receipt image');
            }

            // Convert to blob
            const blob = await response.blob();

            // Create blob URL
            const blobUrl = window.URL.createObjectURL(blob);

            // Create temporary link element to trigger download
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = fileName || 'receipt.jpg';
            document.body.appendChild(link);
            link.click();

            // Cleanup
            document.body.removeChild(link);
            window.URL.revokeObjectURL(blobUrl);
        } catch (error) {
            console.error('Error downloading receipt:', error);
            alert('Failed to download receipt. Please try again.');
        }
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
        <div className="dashboard-container" style={{ maxWidth: '1400px', margin: '0 auto', padding: '1rem' }}>
            <div style={{ marginBottom: '2rem', paddingBottom: '1rem' }}>
                <h1 className="dashboard-title" style={{ fontSize: 'clamp(1.75rem, 5vw, 2.75rem)', fontWeight: '700', marginBottom: '0.75rem', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', letterSpacing: '-0.5px' }}>Expense History</h1>
                <p style={{ color: '#64748b', fontSize: 'clamp(0.9rem, 2.5vw, 1.05rem)', fontWeight: '400' }}>Track and manage all your household expenses</p>
            </div>

            {/* Household Selector - Only show for Receipts view */}
            {viewMode === 'receipts' && households.length > 0 && (
                <div className="household-selector" style={{
                    marginBottom: '1.5rem',
                    padding: 'clamp(1rem, 2.5vw, 1.5rem)',
                    background: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                    border: '1px solid rgba(0,0,0,0.05)',
                    display: 'flex',
                    flexWrap: 'wrap',
                    alignItems: 'center',
                    gap: '0.75rem'
                }}>
                    <label htmlFor="household-select" style={{ fontWeight: '600', marginRight: '1rem' }}>
                        Select Household:
                    </label>
                    <select
                        id="household-select"
                        value={selectedHouseholdId ?? ''}
                        onChange={e => setSelectedHouseholdId(e.target.value)}
                        style={{
                            marginLeft: '0.5rem',
                            padding: '0.6rem 1rem',
                            borderRadius: '8px',
                            border: '2px solid #e2e8f0',
                            fontSize: '0.95rem',
                            transition: 'all 0.2s ease',
                            cursor: 'pointer'
                        }}
                    >
                        {households.map(h => (
                            <option key={h.id} value={h.id}>{h.name}</option>
                        ))}
                    </select>
                </div>
            )}

            {/* No household message for Receipts view */}
            {viewMode === 'receipts' && households.length === 0 && (
                <div style={{
                    marginBottom: '1.5rem',
                    padding: 'clamp(1.5rem, 3vw, 2.5rem)',
                    background: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                    border: '1px solid rgba(0,0,0,0.05)',
                    textAlign: 'center'
                }}>
                    <p style={{ fontSize: '1.1rem', marginBottom: '1rem', color: '#64748b' }}>
                        You need to be part of a household to view receipts.
                    </p>
                    <Link to="/dashboard" style={{
                        display: 'inline-block',
                        padding: '0.75rem 1.5rem',
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                        color: 'white',
                        borderRadius: '10px',
                        textDecoration: 'none',
                        fontWeight: '600',
                        transition: 'all 0.2s ease',
                        boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)'
                    }}>
                        Go to Dashboard to Create a Household
                    </Link>
                </div>
            )}

            {/* Period Selector */}
            <div style={{
                marginBottom: '1.5rem',
                padding: 'clamp(1rem, 2.5vw, 1.5rem)',
                background: 'white',
                borderRadius: '12px',
                boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.05)',
                display: 'flex',
                gap: 'clamp(0.75rem, 2vw, 1.5rem)',
                alignItems: 'center',
                flexWrap: 'wrap'
            }}>
                <div>
                    <label htmlFor="month-select" style={{ fontWeight: '600', marginRight: '0.75rem' }}>Month:</label>
                    <select
                        id="month-select"
                        value={selectedMonth}
                        onChange={e => setSelectedMonth(Number(e.target.value))}
                        style={{
                            padding: '0.6rem 1rem',
                            borderRadius: '8px',
                            border: '2px solid #e2e8f0',
                            fontSize: '0.95rem',
                            transition: 'all 0.2s ease',
                            cursor: 'pointer'
                        }}
                    >
                        {months.map((month, idx) => (
                            <option key={idx} value={idx + 1}>{month}</option>
                        ))}
                    </select>
                </div>
                <div>
                    <label htmlFor="year-select" style={{ fontWeight: '600', marginRight: '0.75rem' }}>Year:</label>
                    <select
                        id="year-select"
                        value={selectedYear}
                        onChange={e => setSelectedYear(Number(e.target.value))}
                        style={{
                            padding: '0.6rem 1rem',
                            borderRadius: '8px',
                            border: '2px solid #e2e8f0',
                            fontSize: '0.95rem',
                            transition: 'all 0.2s ease',
                            cursor: 'pointer'
                        }}
                    >
                        {years.map(year => (
                            <option key={year} value={year}>{year}</option>
                        ))}
                    </select>
                </div>
            </div>

            {/* View Mode Toggle */}
            <div style={{
                marginBottom: '1.5rem',
                padding: '0.5rem',
                background: 'white',
                borderRadius: '12px',
                boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                border: '1px solid rgba(0,0,0,0.05)',
                display: 'inline-flex',
                gap: '0.5rem'
            }}>
                <button
                    className={viewMode === 'expenses' ? 'primary-btn' : 'secondary-btn'}
                    onClick={() => setViewMode('expenses')}
                    style={{
                        padding: '0.65rem 1.5rem',
                        borderRadius: '8px',
                        transition: 'all 0.2s ease',
                        fontWeight: '500',
                        boxShadow: viewMode === 'expenses' ? '0 2px 6px rgba(102, 126, 234, 0.3)' : 'none'
                    }}
                >
                    My Expenses
                </button>
                <button
                    className={viewMode === 'receipts' ? 'primary-btn' : 'secondary-btn'}
                    onClick={() => setViewMode('receipts')}
                    style={{
                        padding: '0.65rem 1.5rem',
                        borderRadius: '8px',
                        transition: 'all 0.2s ease',
                        fontWeight: '500',
                        boxShadow: viewMode === 'receipts' ? '0 2px 6px rgba(102, 126, 234, 0.3)' : 'none'
                    }}
                >
                    Receipts
                </button>
            </div>

            {(viewMode === 'expenses' || (viewMode === 'receipts' && selectedHouseholdId)) && (
                loading ? (
                    <p>Loading...</p>
                ) : (
                    <div className="dashboard-data-section">
                        {/* Summary */}
                        <div style={{
                            marginBottom: '2rem',
                            padding: 'clamp(1.25rem, 3vw, 2rem)',
                            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                            borderRadius: '12px',
                            display: 'flex',
                            gap: 'clamp(1.5rem, 4vw, 3rem)',
                            flexWrap: 'wrap',
                            boxShadow: '0 4px 12px rgba(102, 126, 234, 0.25)',
                            color: 'white'
                        }}>
                            {viewMode === 'receipts' ? (
                                <>
                                    <div>
                                        <div style={{ fontSize: '0.85rem', opacity: 0.9, marginBottom: '0.25rem' }}>Total Receipts</div>
                                        <div style={{ fontSize: '1.75rem', fontWeight: 'bold' }}>{receipts.length}</div>
                                    </div>
                                    <div>
                                        <div style={{ fontSize: '0.85rem', opacity: 0.9, marginBottom: '0.25rem' }}>Total Amount</div>
                                        <div style={{ fontSize: '1.75rem', fontWeight: 'bold' }}>${receipts.reduce((sum, r) => sum + (r.totalAmount || 0), 0).toFixed(2)}</div>
                                    </div>
                                </>
                            ) : (
                                <>
                                    <div>
                                        <div style={{ fontSize: '0.85rem', opacity: 0.9, marginBottom: '0.25rem' }}>
                                            My Total Expenses
                                        </div>
                                        <div style={{ fontSize: '1.75rem', fontWeight: 'bold' }}>${expenses.reduce((sum, e) => sum + e.amount, 0).toFixed(2)}</div>
                                    </div>
                                    <div>
                                        <div style={{ fontSize: '0.85rem', opacity: 0.9, marginBottom: '0.25rem' }}>Number of Expenses</div>
                                        <div style={{ fontSize: '1.75rem', fontWeight: 'bold' }}>{expenses.length}</div>
                                    </div>
                                </>
                            )}
                        </div>

                        {viewMode === 'receipts' ? (
                            /* Receipts View */
                            <div>
                                <h2 style={{ marginBottom: '1rem' }}>
                                    Household Receipts
                                </h2>
                                <p style={{ color: '#64748b', fontSize: '0.9rem', marginBottom: '1rem' }}>
                                    Showing all receipts for {households.find(h => h.id === selectedHouseholdId)?.name || 'this household'}
                                </p>
                                {receipts.length > 0 ? (
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                                        {receipts.map(receipt => (
                                            <div
                                                key={receipt.id}
                                                style={{
                                                    border: '1px solid rgba(0,0,0,0.08)',
                                                    borderRadius: '12px',
                                                    padding: '1.75rem',
                                                    background: '#fff',
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
                                                }}
                                            >
                                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', flexWrap: 'wrap', gap: '1rem' }}>
                                                    <div style={{ flex: '1 1 250px', minWidth: 0 }}>
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
                                                    <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                                                        <button
                                                            className="secondary-btn"
                                                            onClick={() => toggleReceipt(receipt.id)}
                                                            style={{
                                                                display: 'flex',
                                                                alignItems: 'center',
                                                                gap: '0.5rem',
                                                                borderRadius: '8px',
                                                                transition: 'all 0.2s ease',
                                                                padding: '0.6rem 1rem'
                                                            }}
                                                        >
                                                            {expandedReceipts.has(receipt.id) ? 'Hide' : 'Show'} Items
                                                        </button>
                                                        <button
                                                            className="primary-btn"
                                                            onClick={() => handleDownloadReceipt(
                                                                receipt.imagePath,
                                                                receipt.originalFileName
                                                            )}
                                                            style={{
                                                                display: 'flex',
                                                                alignItems: 'center',
                                                                gap: '0.5rem',
                                                                borderRadius: '8px',
                                                                transition: 'all 0.2s ease',
                                                                padding: '0.6rem 1rem',
                                                                boxShadow: '0 2px 6px rgba(102, 126, 234, 0.3)'
                                                            }}
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
                                <h2 style={{ marginBottom: '1rem' }}>
                                    My Expenses
                                </h2>
                                <p style={{ color: '#64748b', fontSize: '0.9rem', marginBottom: '1rem' }}>
                                    Showing all your expenses across all households
                                </p>
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
                                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', flexWrap: 'wrap', gap: '1rem' }}>
                                                            <div style={{ flex: '1 1 250px', minWidth: 0 }}>
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
                                        No expenses found in {months[selectedMonth - 1]} {selectedYear}
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
