import supabase from "../services/supabaseClient"





function Dashboard({ onLogout }: { onLogout: () => void }) {
    const handleLogout = async () => {
        await supabase.auth.signOut();
        onLogout()
    };

    return (
        <div style={{ padding: '2rem', textAlign: 'center' }}>
            <h1>Dashboard</h1>
            <p>You are now logged in!</p>
            <button onClick={handleLogout}>Sign Out</button>
        </div>
    );
}

export default Dashboard