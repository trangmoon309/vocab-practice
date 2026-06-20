import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as wordsApi from '../api/words'
import type { Word, WordInput } from '../types'

const EMPTY_INPUT: WordInput = {
  english: '',
  vietnamese: '',
  pronunciation: '',
  level: null,
  type: null,
  exampleSentence: '',
}

function WordForm({
  input,
  setInput,
  saving,
  onSubmit,
  onCancel,
  submitLabel,
}: {
  input: WordInput
  setInput: (input: WordInput) => void
  saving: boolean
  onSubmit: (e: React.FormEvent) => Promise<void>
  onCancel: () => void
  submitLabel: string
}) {
  return (
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
          onClick={onCancel}
          className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50">
          Cancel
        </button>
      </div>
    </form>
  )
}

export function WordsPage() {
  const [words, setWords] = useState<Word[]>([])
  const [loading, setLoading] = useState(true)
  const [showAddForm, setShowAddForm] = useState(false)
  const [input, setInput] = useState<WordInput>(EMPTY_INPUT)
  const [editingWord, setEditingWord] = useState<Word | null>(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    wordsApi.getWords().then((r) => setWords(r.data)).finally(() => setLoading(false))
  }, [])

  const refresh = async () => {
    const r = await wordsApi.getWords()
    setWords(r.data)
  }

  const cancelForm = () => { setShowAddForm(false); setEditingWord(null); setInput(EMPTY_INPUT) }

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await wordsApi.addWord(input)
      setInput(EMPTY_INPUT)
      setShowAddForm(false)
      await refresh()
    } finally {
      setSaving(false)
    }
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingWord) return
    setSaving(true)
    try {
      await wordsApi.updateWord(editingWord.id, input)
      setEditingWord(null)
      setInput(EMPTY_INPUT)
      await refresh()
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (wordId: string) => {
    if (!confirm('Delete this word?')) return
    await wordsApi.deleteWord(wordId)
    await refresh()
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

  if (loading) return <div className="flex h-screen items-center justify-center text-gray-400">Loading…</div>

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-3xl px-4 py-8">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-800">My Words</h2>
            <p className="text-sm text-gray-400">{words.length} words</p>
          </div>
          <div className="flex gap-2">
            <Link
              to="/chat"
              className="rounded border border-indigo-600 px-4 py-2 text-sm text-indigo-600 hover:bg-indigo-50"
            >
              AI Chat
            </Link>
            <Link
              to="/game"
              className="rounded bg-green-600 px-4 py-2 text-sm text-white hover:bg-green-700"
            >
              Play
            </Link>
            <button
              onClick={() => { setShowAddForm(true); setEditingWord(null); setInput(EMPTY_INPUT) }}
              className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700"
            >
              + Add word
            </button>
          </div>
        </div>

        {showAddForm && (
          <WordForm
            input={input}
            setInput={setInput}
            saving={saving}
            onSubmit={handleAdd}
            onCancel={cancelForm}
            submitLabel="Add word"
          />
        )}

        {words.length === 0 ? (
          <div className="rounded-lg bg-white p-8 text-center text-gray-400 shadow">
            No words yet. Add some!
          </div>
        ) : (
          <div className="space-y-2">
            {words.map((word) => (
              <div key={word.id}>
                {editingWord?.id === word.id ? (
                  <WordForm
                    input={input}
                    setInput={setInput}
                    saving={saving}
                    onSubmit={handleUpdate}
                    onCancel={cancelForm}
                    submitLabel="Save"
                  />
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
