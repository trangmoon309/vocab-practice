import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as wordSetsApi from '../api/wordsets'
import type { WordSet, WordSetInput } from '../types'

export function WordSetsPage() {
  const navigate = useNavigate()
  const [wordSets, setWordSets] = useState<WordSet[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    wordSetsApi.getWordSets().then((r) => setWordSets(r.data)).finally(() => setLoading(false))
  }, [])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      const input: WordSetInput = { title, description: description || null }
      const r = await wordSetsApi.createWordSet(input)
      navigate(`/wordsets/${r.data.id}`)
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-3xl px-4 py-8">
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-gray-800">My Word Sets</h2>
          <div className="flex gap-2">
            <Link
              to="/chat"
              className="rounded border border-indigo-600 px-4 py-2 text-sm text-indigo-600 hover:bg-indigo-50"
            >
              AI Chat
            </Link>
            <button
              onClick={() => setShowForm(true)}
              className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700"
            >
              + New set
            </button>
          </div>
        </div>

        {showForm && (
          <form onSubmit={handleCreate} className="mb-6 rounded-lg bg-white p-4 shadow space-y-3">
            <input
              type="text"
              placeholder="Set title"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
            />
            <input
              type="text"
              placeholder="Description (optional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
            />
            <div className="flex gap-2">
              <button
                type="submit"
                disabled={saving}
                className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700 disabled:opacity-50"
              >
                {saving ? 'Creating…' : 'Create'}
              </button>
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        )}

        {loading ? (
          <p className="text-sm text-gray-400">Loading…</p>
        ) : wordSets.length === 0 ? (
          <div className="rounded-lg bg-white p-8 text-center text-gray-400 shadow">
            No word sets yet. Create one to get started!
          </div>
        ) : (
          <ul className="space-y-3">
            {wordSets.map((ws) => (
              <li key={ws.id}>
                <Link
                  to={`/wordsets/${ws.id}`}
                  className="flex items-center justify-between rounded-lg bg-white p-4 shadow hover:shadow-md transition-shadow"
                >
                  <div>
                    <p className="font-medium text-gray-800">{ws.title}</p>
                    {ws.description && (
                      <p className="mt-0.5 text-sm text-gray-500">{ws.description}</p>
                    )}
                  </div>
                  <span className="text-sm text-gray-400">{ws.wordCount} words</span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}
