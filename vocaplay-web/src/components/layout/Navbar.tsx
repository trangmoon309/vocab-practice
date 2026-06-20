import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav className="flex items-center justify-between bg-indigo-600 px-6 py-4 text-white">
      <Link to="/" className="text-xl font-bold">
        VocaPlay
      </Link>
      <div className="flex items-center gap-4">
        {user && <span className="text-sm">{user.displayName}</span>}
        <button
          onClick={handleLogout}
          className="rounded bg-indigo-700 px-3 py-1 text-sm hover:bg-indigo-800"
        >
          Logout
        </button>
      </div>
    </nav>
  );
}
