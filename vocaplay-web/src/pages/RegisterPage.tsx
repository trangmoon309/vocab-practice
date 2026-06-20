import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await register(email, displayName, password)
      navigate('/')
    } catch {
      setError('Registration failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-cream-100 via-lavender-50 to-mint-50 px-4">
      <div className="bento-card animate-pop-in w-full max-w-sm p-8">
        <div className="mb-6 flex items-center gap-2">
          <span className="flex h-10 w-10 items-center justify-center rounded-bento bg-lavender-200 text-lg">✨</span>
          <h1 className="font-display text-2xl font-bold text-ink-700">Create account</h1>
        </div>
        <p className="mb-6 text-sm text-ink-500">Start building your vocabulary today.</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-semibold text-ink-600">Display name</label>
            <input
              type="text"
              required
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              className="input-pastel w-full"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-semibold text-ink-600">Email</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="input-pastel w-full"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-semibold text-ink-600">Password</label>
            <input
              type="password"
              required
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="input-pastel w-full"
            />
          </div>
          {error && <p className="rounded-bento bg-coral-50 px-3 py-2 text-sm text-coral-600">{error}</p>}
          <button
            type="submit"
            disabled={loading}
            className="btn-coral w-full"
          >
            {loading ? 'Creating account…' : 'Register'}
          </button>
        </form>
        <p className="mt-5 text-center text-sm text-ink-500">
          Already have an account?{' '}
          <Link to="/login" className="font-semibold text-lavender-600 hover:underline">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  )
}
