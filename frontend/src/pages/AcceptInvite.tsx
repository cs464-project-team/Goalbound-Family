import React, { useEffect, useState, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const baseUrl = API_BASE_URL ?? "";

const AcceptInvite: React.FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { session } = useAuth();
    const [status, setStatus] = useState<'loading' | 'success' | 'error' | 'unauthenticated'>('loading');
    const [message, setMessage] = useState('');
    const hasAttemptedRef = useRef(false);

    useEffect(() => {
        const token = searchParams.get('token');
        if (!token) {
            setTimeout(() => {
                setStatus('error');
                setMessage('Invalid or missing invitation token.');
            }, 0);
            localStorage.removeItem('pendingInviteToken');
            return;
        }

        // Check if user is authenticated
        if (!session) {
            setTimeout(() => {
                setStatus('unauthenticated');
            }, 0);
            localStorage.setItem('pendingInviteToken', token);
            return;
        }

        // Prevent duplicate API calls
        if (hasAttemptedRef.current) return;
        hasAttemptedRef.current = true;

        // Call backend API to accept invite
        // console.log("[AcceptInvite] Accepting invite with token:", token, "userId:", session.user.id);
        fetch(`${baseUrl}/api/invitations/accept?token=${encodeURIComponent(token)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                token: token,
                userId: session.user.id
            })
        })
            .then(async (res) => {
                if (res.ok) {
                    setStatus('success');
                    setMessage('Invitation accepted! You can now access your household.');
                    // Clear the pending token
                    localStorage.removeItem('pendingInviteToken');
                } else {
                    const data = await res.json().catch(() => ({}));
                    setStatus('error');
                    setMessage(data.message || 'Failed to accept invitation. The invite may be invalid, expired, or already used.');
                    // Clear the pending token on error
                    localStorage.removeItem('pendingInviteToken');
                }
            })
            .catch(() => {
                setStatus('error');
                setMessage('Network error. Please try again later.');
                // Clear the pending token on error
                localStorage.removeItem('pendingInviteToken');
            });
    }, [searchParams, session]);

    const handleLogin = () => {
        navigate('/auth');
    };

    return (
        <div style={{ maxWidth: 400, margin: '80px auto', textAlign: 'center' }}>
            <h2>Accept Invitation</h2>
            {status === 'loading' && <p>Processing your invitation...</p>}
            {status === 'unauthenticated' && (
                <>
                    <p>You need to log in or sign up to accept this invitation.</p>
                    <button
                        onClick={handleLogin}
                        style={{
                            padding: '10px 20px',
                            backgroundColor: '#007bff',
                            color: 'white',
                            border: 'none',
                            borderRadius: '5px',
                            cursor: 'pointer',
                            marginTop: '10px'
                        }}
                    >
                        Login / Sign Up
                    </button>
                </>
            )}
            {status === 'success' && (
                <>
                    <p style={{ color: 'green' }}>{message}</p>
                    <button
                        onClick={() => navigate('/dashboard')}
                        style={{
                            padding: '10px 20px',
                            backgroundColor: '#28a745',
                            color: 'white',
                            border: 'none',
                            borderRadius: '5px',
                            cursor: 'pointer',
                            marginTop: '10px'
                        }}
                    >
                        Go to Dashboard
                    </button>
                </>
            )}
            {status === 'error' && <p style={{ color: 'red' }}>{message}</p>}
        </div>
    );
};

export default AcceptInvite;
