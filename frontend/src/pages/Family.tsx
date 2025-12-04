import { useEffect, useState } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { Navigate, useNavigate } from 'react-router-dom';
import { getApiUrl } from '../config/api';
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
        fetch(getApiUrl(`/api/householdmembers/user/${session.user.id}`))
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
        fetch(getApiUrl(`/api/householdmembers/${selectedHouseholdId}`))
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

            const res = await fetch(getApiUrl(`/api/householdmembers/${membership.id}?requestingUserId=${session.user.id}`), {
                method: 'DELETE'
            });

            if (!res.ok) {
                const errorData = await res.json().catch(() => ({ message: 'Failed to leave household' }));
                throw new Error(errorData.message || 'Failed to leave household');
            }

            // Refresh households list
            const updatedHouseholds = await fetch(getApiUrl(`/api/householdmembers/user/${session.user.id}`))
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
            const res = await fetch(getApiUrl(`/api/householdmembers/${memberToRemove.id}?requestingUserId=${session.user.id}`), {
                method: 'DELETE'
            });

            if (!res.ok) {
                const errorData = await res.json().catch(() => ({ message: 'Failed to remove member' }));
                throw new Error(errorData.message || 'Failed to remove member');
            }

            // Refresh members list
            const updatedMembers = await fetch(getApiUrl(`/api/householdmembers/${selectedHouseholdId}`))
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
        <div className="dashboard-container" style={{ maxWidth: '1000px', margin: '0 auto', padding: '2rem' }}>
            <div style={{ marginBottom: '2rem' }}>
                <h1 className="dashboard-title" style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>Family Management</h1>
                <p style={{ color: '#666', fontSize: '1.1rem' }}>Manage your household memberships</p>
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
                        padding: '1.5rem',
                        background: 'white',
                        borderRadius: '12px',
                        boxShadow: '0 2px 4px rgba(0,0,0,0.08)'
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
                                borderRadius: '8px',
                                border: '1px solid #ddd',
                                fontSize: '1rem',
                                minWidth: '250px'
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
                            padding: '2rem',
                            borderRadius: '12px',
                            boxShadow: '0 2px 4px rgba(0,0,0,0.08)',
                            marginBottom: '2rem'
                        }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
                                <div>
                                    <h2 style={{ fontSize: '1.8rem', fontWeight: '600', marginBottom: '0.5rem' }}>{selectedHousehold.name}</h2>
                                    <p style={{ color: '#666', fontSize: '0.95rem' }}>
                                        {isOwner ? 'You are the owner of this household' : 'You are a member of this household'}
                                    </p>
                                </div>
                                {!isOwner && (
                                    <button
                                        type="button"
                                        onClick={() => setShowLeaveConfirm(true)}
                                        style={{
                                            padding: '0.75rem 1.5rem',
                                            borderRadius: '8px',
                                            border: '1px solid #e53e3e',
                                            background: 'white',
                                            color: '#e53e3e',
                                            fontWeight: '600',
                                            cursor: 'pointer',
                                            transition: 'all 0.2s'
                                        }}
                                        onMouseOver={e => {
                                            e.currentTarget.style.background = '#e53e3e';
                                            e.currentTarget.style.color = 'white';
                                        }}
                                        onMouseOut={e => {
                                            e.currentTarget.style.background = 'white';
                                            e.currentTarget.style.color = '#e53e3e';
                                        }}
                                    >
                                        Leave Household
                                    </button>
                                )}
                            </div>

                            {/* Members List */}
                            <div>
                                <h3 style={{ fontSize: '1.3rem', fontWeight: '600', marginBottom: '1rem' }}>
                                    Members ({members.length})
                                </h3>
                                <div style={{ display: 'grid', gap: '1rem' }}>
                                    {loading ? (
                                        <p style={{ color: '#666' }}>Loading members...</p>
                                    ) : members.length === 0 ? (
                                        <p style={{ color: '#666' }}>No members found.</p>
                                    ) : (
                                        members.map(member => (
                                            <div
                                                key={member.id}
                                                style={{
                                                    padding: '1.5rem',
                                                    background: '#f7fafc',
                                                    borderRadius: '8px',
                                                    display: 'flex',
                                                    justifyContent: 'space-between',
                                                    alignItems: 'center'
                                                }}
                                            >
                                                <div>
                                                    <div style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '0.25rem' }}>
                                                        {member.userName || `${member.firstName} ${member.lastName}`.trim() || member.email}
                                                        {member.userId === session.user.id && (
                                                            <span style={{
                                                                marginLeft: '0.75rem',
                                                                fontSize: '0.85rem',
                                                                padding: '0.25rem 0.75rem',
                                                                background: '#667eea',
                                                                color: 'white',
                                                                borderRadius: '12px',
                                                                fontWeight: '500'
                                                            }}>
                                                                You
                                                            </span>
                                                        )}
                                                        {member.userId === selectedHousehold.parentId && (
                                                            <span style={{
                                                                marginLeft: '0.75rem',
                                                                fontSize: '0.85rem',
                                                                padding: '0.25rem 0.75rem',
                                                                background: '#f59e0b',
                                                                color: 'white',
                                                                borderRadius: '12px',
                                                                fontWeight: '500'
                                                            }}>
                                                                Owner
                                                            </span>
                                                        )}
                                                    </div>
                                                    <div style={{ fontSize: '0.85rem', color: '#666' }}>
                                                        {member.email}
                                                    </div>
                                                    <div style={{ fontSize: '0.85rem', color: '#999', marginTop: '0.25rem' }}>
                                                        Joined {new Date(member.joinedAt).toLocaleDateString('en-US', {
                                                            year: 'numeric',
                                                            month: 'short',
                                                            day: 'numeric'
                                                        })}
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
                                                            padding: '0.5rem 1rem',
                                                            borderRadius: '6px',
                                                            border: '1px solid #e53e3e',
                                                            background: 'white',
                                                            color: '#e53e3e',
                                                            fontWeight: '600',
                                                            cursor: 'pointer',
                                                            fontSize: '0.9rem',
                                                            transition: 'all 0.2s'
                                                        }}
                                                        onMouseOver={e => {
                                                            e.currentTarget.style.background = '#e53e3e';
                                                            e.currentTarget.style.color = 'white';
                                                        }}
                                                        onMouseOut={e => {
                                                            e.currentTarget.style.background = 'white';
                                                            e.currentTarget.style.color = '#e53e3e';
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
