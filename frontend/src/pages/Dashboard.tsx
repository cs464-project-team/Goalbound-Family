

function Dashboard({ onLogout }: { onLogout: () => void }) {
    return (
        <div style={{ padding: '2rem', textAlign: 'center' }}>
            <h1>Dashboard</h1>
            <p>You are now logged in!</p>
            <button onClick={onLogout}>Sign Out</button>
        </div>
    );
}

export default Dashboard