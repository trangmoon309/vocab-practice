import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function LoginPage() {
  const { login, isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate('/', { replace: true })
    }
  }, [isAuthenticated, isLoading, navigate])
  const savedEmail = localStorage.getItem('rememberedEmail') ?? ''
  const [email, setEmail] = useState(savedEmail)
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(!!savedEmail)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      if (rememberMe) {
        localStorage.setItem('rememberedEmail', email)
      } else {
        localStorage.removeItem('rememberedEmail')
      }
      await login(email, password, rememberMe)
      navigate('/')
    } catch {
      setError('Invalid email or password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-cream-100 via-lavender-50 to-mint-50 px-4">
      <div className="bento-card animate-pop-in w-full max-w-sm p-8">
        <div className="mb-6 flex items-center gap-2">
          <span className="flex h-10 w-10 items-center justify-center rounded-bento bg-mint-200 text-lg">🌱</span>
          <h1 className="font-display text-2xl font-bold text-ink-700">VocaPlay</h1>
        </div>
        <p className="mb-6 text-sm text-ink-500">Welcome back! Let's keep your streak going.</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="email" className="mb-1 block text-sm font-semibold text-ink-600">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="input-pastel w-full"
            />
          </div>
          <div>
            <label htmlFor="password" className="mb-1 block text-sm font-semibold text-ink-600">Password</label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="input-pastel w-full"
            />
          </div>
          <label className="flex cursor-pointer items-center gap-2">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              className="h-4 w-4 rounded accent-coral-400"
            />
            <span className="text-sm text-ink-500">Remember me</span>
          </label>
          {error && <p className="rounded-bento bg-coral-50 px-3 py-2 text-sm text-coral-600">{error}</p>}
          <button
            type="submit"
            disabled={loading}
            className="btn-coral w-full"
          >
            {loading ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
        <p className="mt-5 text-center text-sm text-ink-500">
          Don't have an account?{' '}
          <Link to="/register" className="font-semibold text-lavender-600 hover:underline">
            Register
          </Link>
        </p>
      </div>
    </div>
  )
}
