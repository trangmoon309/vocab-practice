import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as wordSetsApi from '../api/wordsets'
import type { Word, WordInput, WordSetDetail } from '../types'

const EMPTY_INPUT: WordInput = {
  english: '',
  vietnamese: '',
  pronunciation: '',
  level: null,
  type: null,
  exampleSentence: '',
}

export function WordSetDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [set, setSet] = useState<WordSetDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [showAddForm, setShowAddForm] = useState(false)
  const [input, setInput] = useState<WordInput>(EMPTY_INPUT)
  const [editingWord, setEditingWord] = useState<Word | null>(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!id) return
    wordSetsApi.getWordSet(id).then((r) => setSet(r.data)).finally(() => setLoading(false))
  }, [id])

  const refresh = async () => {
    if (!id) return
    const r = await wordSetsApi.getWordSet(id)
    setSet(r.data)
  }

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id) return
    setSaving(true)
    try {
      await wordSetsApi.addWord(id, input)
      setInput(EMPTY_INPUT)
      setShowAddForm(false)
      await refresh()
    } finally {
      setSaving(false)
    }
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!id || !editingWord) return
    setSaving(true)
    try {
      await wordSetsApi.updateWord(id, editingWord.id, input)
      setEditingWord(null)
      setInput(EMPTY_INPUT)
      await refresh()
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (wordId: string) => {
    if (!id || !confirm('Delete this word?')) return
    await wordSetsApi.deleteWord(id, wordId)
    await refresh()
  }

  const handleDeleteSet = async () => {
    if (!id || !confirm('Delete this entire word set?')) return
    await wordSetsApi.deleteWordSet(id)
    navigate('/')
  }

  const startEdit = (word: Word) => {
    setEditingWord(word)
    setInput({
      english: word.english,
      vietnamese: word.vietnamese,
      pronunciation: word.pronunciation ?? '',
      level: word.level,
      type: word.type,
      exampleSentence: word.exampleSentence ?? '',
    })
    setShowAddForm(false)
  }

  const WordForm = ({ onSubmit, submitLabel }: { onSubmit: (e: React.FormEvent) => Promise<void>, submitLabel: string }) => (
    <form onSubmit={onSubmit} className="rounded-lg bg-white p-4 shadow space-y-3 mb-4">
      <div className="grid grid-cols-2 gap-3">
        <input
          type="text" placeholder="English" required
          value={input.english}
          onChange={(e) => setInput({ ...input, english: e.target.value })}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
        />
        <input
          type="text" placeholder="Vietnamese" required
          value={input.vietnamese}
          onChange={(e) => setInput({ ...input, vietnamese: e.target.value })}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
        />
        <input
          type="text" placeholder="Pronunciation (optional)"
          value={input.pronunciation ?? ''}
          onChange={(e) => setInput({ ...input, pronunciation: e.target.value || null })}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
        />
        <input
          type="text" placeholder="Example sentence (optional)"
          value={input.exampleSentence ?? ''}
          onChange={(e) => setInput({ ...input, exampleSentence: e.target.value || null })}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none"
        />
      </div>
      <div className="flex gap-2">
        <button type="submit" disabled={saving}
          className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700 disabled:opacity-50">
          {saving ? 'Saving…' : submitLabel}
        </button>
        <button type="button"
          onClick={() => { setShowAddForm(false); setEditingWord(null); setInput(EMPTY_INPUT) }}
          className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50">
          Cancel
        </button>
      </div>
    </form>
  )

  if (loading) return <div className="flex h-screen items-center justify-center text-gray-400">Loading…</div>
  if (!set) return null

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-3xl px-4 py-8">
        <div className="mb-2 flex items-center gap-2">
          <Link to="/" className="text-sm text-indigo-600 hover:underline">← Word Sets</Link>
        </div>
        <div className="mb-6 flex items-start justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-800">{set.title}</h2>
            {set.description && <p className="text-sm text-gray-500">{set.description}</p>}
            <p className="text-sm text-gray-400">{set.wordCount} words</p>
          </div>
          <div className="flex gap-2">
            <Link
              to={`/wordsets/${id}/game`}
              className="rounded bg-green-600 px-3 py-2 text-sm text-white hover:bg-green-700"
            >
              Play
            </Link>
            <button
              onClick={() => { setShowAddForm(true); setEditingWord(null); setInput(EMPTY_INPUT) }}
              className="rounded bg-indigo-600 px-3 py-2 text-sm text-white hover:bg-indigo-700"
            >
              + Add word
            </button>
            <button
              onClick={handleDeleteSet}
              className="rounded border border-red-300 px-3 py-2 text-sm text-red-600 hover:bg-red-50"
            >
              Delete set
            </button>
          </div>
        </div>

        {showAddForm && <WordForm onSubmit={handleAdd} submitLabel="Add word" />}

        {set.words.length === 0 ? (
          <div className="rounded-lg bg-white p-8 text-center text-gray-400 shadow">
            No words yet. Add some!
          </div>
        ) : (
          <div className="space-y-2">
            {set.words.map((word) => (
              <div key={word.id}>
                {editingWord?.id === word.id ? (
                  <WordForm onSubmit={handleUpdate} submitLabel="Save" />
                ) : (
                  <div className="flex items-center justify-between rounded-lg bg-white p-4 shadow">
                    <div>
                      <span className="font-medium text-gray-800">{word.english}</span>
                      <span className="mx-2 text-gray-400">—</span>
                      <span className="text-gray-700">{word.vietnamese}</span>
                      {word.pronunciation && (
                        <span className="ml-2 text-sm text-gray-400">/{word.pronunciation}/</span>
                      )}
                      {word.exampleSentence && (
                        <p className="mt-0.5 text-xs text-gray-400 italic">{word.exampleSentence}</p>
                      )}
                    </div>
                    <div className="flex gap-2">
                      <button onClick={() => startEdit(word)}
                        className="text-sm text-indigo-600 hover:underline">Edit</button>
                      <button onClick={() => handleDelete(word.id)}
                        className="text-sm text-red-500 hover:underline">Delete</button>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
