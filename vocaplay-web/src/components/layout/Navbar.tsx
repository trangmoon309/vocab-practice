import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const NAV_LINKS = [
  { to: '/', label: 'My Words' },
  { to: '/game', label: 'Play' },
];

export function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav className="sticky top-0 z-10 border-b border-lavender-100 bg-cream-50/90 backdrop-blur-sm">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3 sm:px-6">
        <Link to="/" className="flex items-center gap-2 font-display text-xl font-bold text-ink-700">
          <span className="flex h-8 w-8 items-center justify-center rounded-bento bg-mint-200 text-base">
            🌱
          </span>
          VocaPlay
        </Link>

        <div className="hidden items-center gap-1 rounded-bento bg-white p-1 shadow-soft sm:flex">
          {NAV_LINKS.map((link) => {
            const active = location.pathname === link.to;
            return (
              <Link
                key={link.to}
                to={link.to}
                className={`rounded-bento px-4 py-1.5 text-sm font-semibold transition-colors ${
                  active
                    ? 'bg-lavender-100 text-ink-700'
                    : 'text-ink-500 hover:bg-cream-100 hover:text-ink-700'
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </div>

        <div className="flex items-center gap-3">
          {user && (
            <span className="hidden text-sm font-medium text-ink-500 sm:inline">
              Hi, {user.displayName.split(' ')[0]} 👋
            </span>
          )}
          <button
            onClick={handleLogout}
            className="rounded-bento bg-white px-3 py-1.5 text-sm font-semibold text-ink-500 shadow-soft transition-colors hover:bg-coral-50 hover:text-coral-600"
          >
            Logout
          </button>
        </div>
      </div>

      <div className="flex items-center justify-center gap-1 border-t border-lavender-100 bg-white px-2 py-1.5 sm:hidden">
        {NAV_LINKS.map((link) => {
          const active = location.pathname === link.to;
          return (
            <Link
              key={link.to}
              to={link.to}
              className={`flex-1 rounded-bento px-3 py-1.5 text-center text-sm font-semibold transition-colors ${
                active ? 'bg-lavender-100 text-ink-700' : 'text-ink-500'
              }`}
            >
              {link.label}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
