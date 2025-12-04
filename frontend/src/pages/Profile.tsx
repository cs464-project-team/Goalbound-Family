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
        <div className="dashboard-container" style={{ maxWidth: '800px', margin: '0 auto', padding: '2rem' }}>
            <div style={{ marginBottom: '2rem' }}>
                <h1 className="dashboard-title" style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>Profile</h1>
                <p style={{ color: '#666', fontSize: '1.1rem' }}>Manage your account information</p>
            </div>

            <div style={{
                background: 'white',
                padding: '2rem',
                borderRadius: '12px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.08)'
            }}>
                <div style={{ marginBottom: '2rem' }}>
                    <h2 style={{ fontSize: '1.5rem', fontWeight: '600', marginBottom: '1.5rem' }}>Account Information</h2>

                    <div style={{ display: 'grid', gap: '1.5rem' }}>
                        <div>
                            <label style={{ display: 'block', fontSize: '0.9rem', fontWeight: '600', color: '#666', marginBottom: '0.5rem' }}>
                                Email Address
                            </label>
                            <div style={{
                                padding: '0.75rem 1rem',
                                background: '#f7fafc',
                                borderRadius: '8px',
                                border: '1px solid #e2e8f0',
                                fontSize: '1rem'
                            }}>
                                {user.email}
                            </div>
                        </div>

                        <div>
                            <label style={{ display: 'block', fontSize: '0.9rem', fontWeight: '600', color: '#666', marginBottom: '0.5rem' }}>
                                User ID
                            </label>
                            <div style={{
                                padding: '0.75rem 1rem',
                                background: '#f7fafc',
                                borderRadius: '8px',
                                border: '1px solid #e2e8f0',
                                fontSize: '0.85rem',
                                fontFamily: 'monospace',
                                color: '#666'
                            }}>
                                {user.id}
                            </div>
                        </div>

                        <div>
                            <label style={{ display: 'block', fontSize: '0.9rem', fontWeight: '600', color: '#666', marginBottom: '0.5rem' }}>
                                Account Created
                            </label>
                            <div style={{
                                padding: '0.75rem 1rem',
                                background: '#f7fafc',
                                borderRadius: '8px',
                                border: '1px solid #e2e8f0',
                                fontSize: '1rem'
                            }}>
                                {user.created_at ? new Date(user.created_at).toLocaleDateString('en-US', {
                                    year: 'numeric',
                                    month: 'long',
                                    day: 'numeric'
                                }) : 'N/A'}
                            </div>
                        </div>
                    </div>
                </div>

                <div style={{
                    marginTop: '2rem',
                    paddingTop: '2rem',
                    borderTop: '1px solid #e2e8f0'
                }}>
                    <h3 style={{ fontSize: '1.2rem', fontWeight: '600', marginBottom: '1rem' }}>Profile Settings</h3>
                    <p style={{ color: '#666', fontSize: '0.95rem' }}>
                        Additional profile settings and customization options will be available here in future updates.
                    </p>
                </div>
            </div>
        </div>
    );
}

export default Profile;
