import { useAuthContext } from '../context/AuthProvider';
import { Navigate } from 'react-router-dom';
import '../styles/Dashboard.css';

function Profile() {
    const { session } = useAuthContext();

    if (!session) {
        return <Navigate to="/auth" replace />;
    }

    const user = session.user;

    return (
        <div className="dashboard-container" style={{ maxWidth: '900px', margin: '0 auto', padding: '1rem', minHeight: '100vh' }}>
            <div style={{ marginBottom: '2rem', paddingBottom: '1rem', textAlign: 'center' }}>
                <h1 className="dashboard-title" style={{
                    fontSize: 'clamp(1.75rem, 5vw, 2.75rem)',
                    fontWeight: '700',
                    marginBottom: '0.75rem',
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    letterSpacing: '-0.5px'
                }}>
                    Profile
                </h1>
                <p style={{ color: '#64748b', fontSize: 'clamp(0.9rem, 2.5vw, 1.15rem)', fontWeight: '400' }}>Manage your account information</p>
            </div>

            <div style={{
                background: 'white',
                padding: 'clamp(1.25rem, 3vw, 2.5rem)',
                borderRadius: '16px',
                boxShadow: '0 4px 12px rgba(0,0,0,0.08), 0 2px 4px rgba(0,0,0,0.04)',
                border: '1px solid rgba(0,0,0,0.05)'
            }}>
                <div style={{ marginBottom: '2.5rem' }}>
                    {/* Profile Avatar Section */}
                    <div style={{
                        display: 'flex',
                        justifyContent: 'center',
                        marginBottom: '2.5rem'
                    }}>
                        <div style={{
                            width: '120px',
                            height: '120px',
                            borderRadius: '50%',
                            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            color: 'white',
                            fontSize: '3rem',
                            fontWeight: '700',
                            boxShadow: '0 8px 16px rgba(102, 126, 234, 0.3)',
                            letterSpacing: '2px'
                        }}>
                            {user.email?.charAt(0).toUpperCase()}
                        </div>
                    </div>

                    <h2 style={{
                        fontSize: 'clamp(1.25rem, 3.5vw, 1.75rem)',
                        fontWeight: '600',
                        marginBottom: '2rem',
                        color: '#1e293b',
                        textAlign: 'center',
                        paddingBottom: '1rem',
                        borderBottom: '2px solid #e2e8f0'
                    }}>
                        Account Information
                    </h2>

                    <div style={{ display: 'grid', gap: '1.75rem' }}>
                        <div style={{
                            padding: '1.5rem',
                            background: 'linear-gradient(135deg, #f8fafc 0%, #e0e7ff 100%)',
                            borderRadius: '12px',
                            border: '2px solid #e2e8f0',
                            transition: 'all 0.2s ease'
                        }}>
                            <label style={{
                                display: 'block',
                                fontSize: '0.85rem',
                                fontWeight: '700',
                                color: '#64748b',
                                marginBottom: '0.75rem',
                                textTransform: 'uppercase',
                                letterSpacing: '0.5px'
                            }}>
                                Email Address
                            </label>
                            <div style={{
                                fontSize: '1.1rem',
                                fontWeight: '500',
                                color: '#1e293b'
                            }}>
                                {user.email}
                            </div>
                        </div>

                        <div style={{
                            padding: '1.5rem',
                            background: 'linear-gradient(135deg, #f8fafc 0%, #e0e7ff 100%)',
                            borderRadius: '12px',
                            border: '2px solid #e2e8f0',
                            transition: 'all 0.2s ease'
                        }}>
                            <label style={{
                                display: 'block',
                                fontSize: '0.85rem',
                                fontWeight: '700',
                                color: '#64748b',
                                marginBottom: '0.75rem',
                                textTransform: 'uppercase',
                                letterSpacing: '0.5px'
                            }}>
                                User ID
                            </label>
                            <div style={{
                                fontSize: '0.9rem',
                                fontFamily: 'monospace',
                                color: '#475569',
                                wordBreak: 'break-all'
                            }}>
                                {user.id}
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
}

export default Profile;
