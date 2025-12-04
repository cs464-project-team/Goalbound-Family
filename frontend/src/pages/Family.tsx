import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate, useNavigate } from 'react-router-dom';
import { getApiUrl } from '../config/api';
import { authenticatedFetch } from '../services/authService';
import '../styles/Dashboard.css';

type Household = {
    id: string;
    name: string;
    parentId: string;
    memberCount: number;
};

type HouseholdMember = {
    id: string;
    userId: string;
    firstName: string;
    lastName: string;
    email: string;
    userName: string;
    role: string;
    joinedAt: string;
};

function Family() {
    const { session } = useAuthContext();
    const navigate = useNavigate();
    const [households, setHouseholds] = useState<Household[]>([]);
    const [selectedHouseholdId, setSelectedHouseholdId] = useState<string | null>(null);
    const [members, setMembers] = useState<HouseholdMember[]>([]);
    const [loading, setLoading] = useState(false);
    const [leavingHousehold, setLeavingHousehold] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [showLeaveConfirm, setShowLeaveConfirm] = useState(false);
    const [removingMember, setRemovingMember] = useState(false);
    const [memberToRemove, setMemberToRemove] = useState<HouseholdMember | null>(null);

    useEffect(() => {
        if (!session?.user?.id) return;
        setLoading(true);
        authenticatedFetch(getApiUrl(`/api/households/user/${session.user.id}`))
            .then(res => res.json())
            .then((data: Household[]) => {
                setHouseholds(data);
                if (data.length > 0) setSelectedHouseholdId(data[0].id);
                setLoading(false);
            })
            .catch(() => setLoading(false));
    }, [session]);

    useEffect(() => {
        if (!selectedHouseholdId) return;
        authenticatedFetch(getApiUrl(`/api/householdmembers/${selectedHouseholdId}`))
            .then(res => res.json())
            .then((data: HouseholdMember[]) => setMembers(data))
            .catch(() => setMembers([]));
    }, [selectedHouseholdId]);

    const handleLeaveHousehold = async () => {
        if (!selectedHouseholdId || !session?.user?.id) return;

        setLeavingHousehold(true);
        setError(null);

        try {
            // Find the membership to delete
            const membership = members.find(m => m.userId === session.user.id);
            if (!membership) {
                throw new Error('Membership not found');
            }

            const res = await authenticatedFetch(getApiUrl(`/api/householdmembers/${membership.id}?requestingUserId=${session.user.id}`), {
                method: 'DELETE'
            });

            if (!res.ok) {
                const errorData = await res.json().catch(() => ({ message: 'Failed to leave household' }));
                throw new Error(errorData.message || 'Failed to leave household');
            }

            // Refresh households list
            const updatedHouseholds = await authenticatedFetch(getApiUrl(`/api/households/user/${session.user.id}`))
                .then(r => r.json());

            setHouseholds(updatedHouseholds);

            if (updatedHouseholds.length > 0) {
                setSelectedHouseholdId(updatedHouseholds[0].id);
            } else {
                setSelectedHouseholdId(null);
                // Navigate to dashboard if no households left
                navigate('/dashboard');
            }

            setShowLeaveConfirm(false);
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setLeavingHousehold(false);
        }
    };

    const handleRemoveMember = async () => {
        if (!memberToRemove || !session?.user?.id) return;

        setRemovingMember(true);
        setError(null);

        try {
            const res = await authenticatedFetch(getApiUrl(`/api/householdmembers/${memberToRemove.id}?requestingUserId=${session.user.id}`), {
                method: 'DELETE'
            });

            if (!res.ok) {
                const errorData = await res.json().catch(() => ({ message: 'Failed to remove member' }));
                throw new Error(errorData.message || 'Failed to remove member');
            }

            // Refresh members list
            const updatedMembers = await authenticatedFetch(getApiUrl(`/api/householdmembers/${selectedHouseholdId}`))
                .then(r => r.json());

            setMembers(updatedMembers);
            setMemberToRemove(null);
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Unknown error');
        } finally {
            setRemovingMember(false);
        }
    };

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    const selectedHousehold = households.find(h => h.id === selectedHouseholdId);
    const isOwner = selectedHousehold?.parentId === session.user.id;

    return (
        <div className="dashboard-container" style={{ maxWidth: '1100px', margin: '0 auto', padding: '1rem', minHeight: '100vh' }}>
            <div style={{ marginBottom: '2rem', paddingBottom: '1rem' }}>
                <h1 className="dashboard-title" style={{
                    fontSize: 'clamp(1.75rem, 5vw, 2.75rem)',
                    fontWeight: '700',
                    marginBottom: '0.75rem',
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    letterSpacing: '-0.5px'
                }}>
                    Family Management
                </h1>
                <p style={{ color: '#64748b', fontSize: 'clamp(0.9rem, 2.5vw, 1.15rem)', fontWeight: '400' }}>Manage your household memberships</p>
            </div>

            {households.length === 0 ? (
                <div style={{
                    background: 'white',
                    padding: '3rem',
                    borderRadius: '12px',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.08)',
                    textAlign: 'center'
                }}>
                    <p style={{ fontSize: '1.1rem', marginBottom: '1.5rem' }}>You are not part of any household yet.</p>
                    <button
                        type="button"
                        className="primary-btn"
                        onClick={() => navigate('/dashboard')}
                        style={{ padding: '0.75rem 2rem', borderRadius: '8px' }}
                    >
                        Go to Dashboard to Create One
                    </button>
                </div>
            ) : (
                <>
                    {/* Household Selector */}
                    <div style={{
                        marginBottom: '2rem',
                        padding: 'clamp(1rem, 3vw, 1.75rem)',
                        background: 'white',
                        borderRadius: '16px',
                        boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                        border: '1px solid rgba(0,0,0,0.05)',
                        display: 'flex',
                        flexWrap: 'wrap',
                        alignItems: 'center',
                        gap: '1rem'
                    }}>
                        <label htmlFor="household-select" style={{ fontWeight: '600', fontSize: '1rem', marginRight: '1rem' }}>
                            Select Household:
                        </label>
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

                    {/* Household Details */}
                    {selectedHousehold && (
                        <div style={{
                            background: 'white',
                            padding: 'clamp(1.25rem, 3vw, 2.5rem)',
                            borderRadius: '16px',
                            boxShadow: '0 2px 8px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)',
                            border: '1px solid rgba(0,0,0,0.05)',
                            marginBottom: '2rem'
                        }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexWrap: 'wrap', gap: '1rem' }}>
                                <div>
                                    <h2 style={{
                                        fontSize: 'clamp(1.5rem, 4vw, 2rem)',
                                        fontWeight: '700',
                                        marginBottom: '0.75rem',
                                        color: '#1e293b'
                                    }}>
                                        {selectedHousehold.name}
                                    </h2>
                                    <div style={{
                                        display: 'inline-block',
                                        padding: '0.5rem 1rem',
                                        background: isOwner ? 'linear-gradient(135deg, #f59e0b 0%, #f97316 100%)' : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                                        color: 'white',
                                        borderRadius: '20px',
                                        fontSize: '0.9rem',
                                        fontWeight: '600',
                                        boxShadow: '0 2px 6px rgba(0,0,0,0.15)'
                                    }}>
                                        {isOwner ? '👑 Owner' : '👤 Member'}
                                    </div>
                                </div>
                                {!isOwner && (
                                    <button
                                        type="button"
                                        onClick={() => setShowLeaveConfirm(true)}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '10px',
                                            border: '2px solid #e53e3e',
                                            background: 'white',
                                            color: '#e53e3e',
                                            fontWeight: '600',
                                            cursor: 'pointer',
                                            transition: 'all 0.2s',
                                            boxShadow: '0 2px 6px rgba(229, 62, 62, 0.2)'
                                        }}
                                        onMouseOver={e => {
                                            e.currentTarget.style.background = '#e53e3e';
                                            e.currentTarget.style.color = 'white';
                                            e.currentTarget.style.transform = 'translateY(-2px)';
                                            e.currentTarget.style.boxShadow = '0 4px 12px rgba(229, 62, 62, 0.3)';
                                        }}
                                        onMouseOut={e => {
                                            e.currentTarget.style.background = 'white';
                                            e.currentTarget.style.color = '#e53e3e';
                                            e.currentTarget.style.transform = 'translateY(0)';
                                            e.currentTarget.style.boxShadow = '0 2px 6px rgba(229, 62, 62, 0.2)';
                                        }}
                                    >
                                        Leave Household
                                    </button>
                                )}
                            </div>

                            {/* Members List */}
                            <div>
                                <h3 style={{
                                    fontSize: 'clamp(1.25rem, 3vw, 1.5rem)',
                                    fontWeight: '600',
                                    marginBottom: '1.5rem',
                                    color: '#1e293b',
                                    paddingBottom: '0.75rem',
                                    borderBottom: '2px solid #e2e8f0'
                                }}>
                                    Members ({members.length})
                                </h3>
                                <div style={{ display: 'grid', gap: '1.25rem' }}>
                                    {loading ? (
                                        <p style={{ color: '#666' }}>Loading members...</p>
                                    ) : members.length === 0 ? (
                                        <p style={{ color: '#666' }}>No members found.</p>
                                    ) : (
                                        members.map(member => (
                                            <div
                                                key={member.id}
                                                style={{
                                                    padding: '1.75rem',
                                                    background: 'white',
                                                    borderRadius: '12px',
                                                    display: 'flex',
                                                    justifyContent: 'space-between',
                                                    alignItems: 'center',
                                                    border: '2px solid #e2e8f0',
                                                    boxShadow: '0 2px 4px rgba(0,0,0,0.04)',
                                                    transition: 'all 0.2s ease'
                                                }}
                                                onMouseEnter={(e) => {
                                                    e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.08)';
                                                    e.currentTarget.style.transform = 'translateY(-2px)';
                                                    e.currentTarget.style.borderColor = '#667eea';
                                                }}
                                                onMouseLeave={(e) => {
                                                    e.currentTarget.style.boxShadow = '0 2px 4px rgba(0,0,0,0.04)';
                                                    e.currentTarget.style.transform = 'translateY(0)';
                                                    e.currentTarget.style.borderColor = '#e2e8f0';
                                                }}
                                            >
                                                <div>
                                                    <div style={{ fontSize: '1.15rem', fontWeight: '600', marginBottom: '0.5rem', color: '#1e293b' }}>
                                                        {member.userName || `${member.firstName} ${member.lastName}`.trim() || member.email}
                                                        {member.userId === session.user.id && (
                                                            <span style={{
                                                                marginLeft: '0.75rem',
                                                                fontSize: '0.85rem',
                                                                padding: '0.35rem 0.85rem',
                                                                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                                                                color: 'white',
                                                                borderRadius: '16px',
                                                                fontWeight: '600',
                                                                boxShadow: '0 2px 4px rgba(102, 126, 234, 0.3)'
                                                            }}>
                                                                You
                                                            </span>
                                                        )}
                                                        {member.userId === selectedHousehold.parentId && (
                                                            <span style={{
                                                                marginLeft: '0.75rem',
                                                                fontSize: '0.85rem',
                                                                padding: '0.35rem 0.85rem',
                                                                background: 'linear-gradient(135deg, #f59e0b 0%, #f97316 100%)',
                                                                color: 'white',
                                                                borderRadius: '16px',
                                                                fontWeight: '600',
                                                                boxShadow: '0 2px 4px rgba(245, 158, 11, 0.3)'
                                                            }}>
                                                                Owner
                                                            </span>
                                                        )}
                                                    </div>
                                                    <div style={{ fontSize: '0.95rem', color: '#64748b', marginBottom: '0.25rem' }}>
                                                        {member.email}
                                                    </div>
                                                    <div style={{ fontSize: '0.85rem', color: '#94a3b8', marginTop: '0.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                                        <span>📅</span>
                                                        <span>
                                                            Joined {new Date(member.joinedAt).toLocaleDateString('en-US', {
                                                                year: 'numeric',
                                                                month: 'short',
                                                                day: 'numeric'
                                                            })}
                                                        </span>
                                                    </div>
                                                </div>
                                                {isOwner && member.userId !== session.user.id && (
                                                    <button
                                                        type="button"
                                                        onClick={() => {
                                                            setMemberToRemove(member);
                                                            setError(null);
                                                        }}
                                                        style={{
                                                            padding: '0.6rem 1.25rem',
                                                            borderRadius: '8px',
                                                            border: '2px solid #e53e3e',
                                                            background: 'white',
                                                            color: '#e53e3e',
                                                            fontWeight: '600',
                                                            cursor: 'pointer',
                                                            fontSize: '0.9rem',
                                                            transition: 'all 0.2s',
                                                            boxShadow: '0 2px 4px rgba(229, 62, 62, 0.2)'
                                                        }}
                                                        onMouseOver={e => {
                                                            e.currentTarget.style.background = '#e53e3e';
                                                            e.currentTarget.style.color = 'white';
                                                            e.currentTarget.style.transform = 'translateY(-2px)';
                                                            e.currentTarget.style.boxShadow = '0 4px 8px rgba(229, 62, 62, 0.3)';
                                                        }}
                                                        onMouseOut={e => {
                                                            e.currentTarget.style.background = 'white';
                                                            e.currentTarget.style.color = '#e53e3e';
                                                            e.currentTarget.style.transform = 'translateY(0)';
                                                            e.currentTarget.style.boxShadow = '0 2px 4px rgba(229, 62, 62, 0.2)';
                                                        }}
                                                    >
                                                        Remove
                                                    </button>
                                                )}
                                            </div>
                                        ))
                                    )}
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Leave Confirmation Modal */}
                    {showLeaveConfirm && (
                        <div style={{
                            position: 'fixed',
                            top: 0,
                            left: 0,
                            right: 0,
                            bottom: 0,
                            background: 'rgba(0,0,0,0.5)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            zIndex: 1000
                        }}>
                            <div style={{
                                background: 'white',
                                padding: '2rem',
                                borderRadius: '12px',
                                maxWidth: '500px',
                                width: '90%',
                                boxShadow: '0 10px 25px rgba(0,0,0,0.2)'
                            }}>
                                <h3 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1rem' }}>
                                    Leave Household?
                                </h3>
                                <p style={{ color: '#666', marginBottom: '1.5rem', lineHeight: '1.6' }}>
                                    Are you sure you want to leave <strong>{selectedHousehold?.name}</strong>?
                                    You will need a new invitation to rejoin this household.
                                </p>
                                {error && (
                                    <div style={{
                                        padding: '1rem',
                                        background: '#fee',
                                        color: '#e53e3e',
                                        borderRadius: '8px',
                                        marginBottom: '1rem',
                                        fontSize: '0.9rem'
                                    }}>
                                        {error}
                                    </div>
                                )}
                                <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setShowLeaveConfirm(false);
                                            setError(null);
                                        }}
                                        disabled={leavingHousehold}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '8px',
                                            border: '1px solid #ddd',
                                            background: 'white',
                                            color: '#333',
                                            fontWeight: '600',
                                            cursor: 'pointer'
                                        }}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="button"
                                        onClick={handleLeaveHousehold}
                                        disabled={leavingHousehold}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '8px',
                                            border: 'none',
                                            background: '#e53e3e',
                                            color: 'white',
                                            fontWeight: '600',
                                            cursor: leavingHousehold ? 'not-allowed' : 'pointer',
                                            opacity: leavingHousehold ? 0.6 : 1
                                        }}
                                    >
                                        {leavingHousehold ? 'Leaving...' : 'Yes, Leave Household'}
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Remove Member Confirmation Modal */}
                    {memberToRemove && (
                        <div style={{
                            position: 'fixed',
                            top: 0,
                            left: 0,
                            right: 0,
                            bottom: 0,
                            background: 'rgba(0,0,0,0.5)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            zIndex: 1000
                        }}>
                            <div style={{
                                background: 'white',
                                padding: '2rem',
                                borderRadius: '12px',
                                maxWidth: '500px',
                                width: '90%',
                                boxShadow: '0 10px 25px rgba(0,0,0,0.2)'
                            }}>
                                <h3 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1rem' }}>
                                    Remove Member?
                                </h3>
                                <p style={{ color: '#666', marginBottom: '1.5rem', lineHeight: '1.6' }}>
                                    Are you sure you want to remove <strong>{memberToRemove.userName || memberToRemove.email}</strong> from <strong>{selectedHousehold?.name}</strong>?
                                </p>
                                {error && (
                                    <div style={{
                                        padding: '1rem',
                                        background: '#fee',
                                        color: '#e53e3e',
                                        borderRadius: '8px',
                                        marginBottom: '1rem',
                                        fontSize: '0.9rem'
                                    }}>
                                        {error}
                                    </div>
                                )}
                                <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setMemberToRemove(null);
                                            setError(null);
                                        }}
                                        disabled={removingMember}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '8px',
                                            border: '1px solid #ddd',
                                            background: 'white',
                                            color: '#333',
                                            fontWeight: '600',
                                            cursor: 'pointer'
                                        }}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="button"
                                        onClick={handleRemoveMember}
                                        disabled={removingMember}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '8px',
                                            border: 'none',
                                            background: '#e53e3e',
                                            color: 'white',
                                            fontWeight: '600',
                                            cursor: removingMember ? 'not-allowed' : 'pointer',
                                            opacity: removingMember ? 0.6 : 1
                                        }}
                                    >
                                        {removingMember ? 'Removing...' : 'Yes, Remove Member'}
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}

export default Family;
