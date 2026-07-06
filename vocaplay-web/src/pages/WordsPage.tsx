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
  englishDefinition: '',
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
    <form onSubmit={onSubmit} className="bento-card animate-pop-in mb-5 space-y-3 border border-lavender-100 p-5">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <input
          type="text" placeholder="English word" required
          value={input.english}
          onChange={(e) => setInput({ ...input, english: e.target.value })}
          className="input-pastel"
        />
        <input
          type="text" placeholder="Vietnamese meaning" required
          value={input.vietnamese}
          onChange={(e) => setInput({ ...input, vietnamese: e.target.value })}
          className="input-pastel"
        />
        <input
          type="text" placeholder="Pronunciation (optional)"
          value={input.pronunciation ?? ''}
          onChange={(e) => setInput({ ...input, pronunciation: e.target.value || null })}
          className="input-pastel"
        />
        <input
          type="text" placeholder="Example sentence (optional)"
          value={input.exampleSentence ?? ''}
          onChange={(e) => setInput({ ...input, exampleSentence: e.target.value || null })}
          className="input-pastel"
        />
        <input
          type="text" placeholder="English definition (optional, for Definition Match)"
          value={input.englishDefinition ?? ''}
          onChange={(e) => setInput({ ...input, englishDefinition: e.target.value || null })}
          className="input-pastel sm:col-span-2"
        />
      </div>
      <div className="flex gap-2 pt-1">
        <button type="submit" disabled={saving} className="btn-coral">
          {saving ? 'Saving…' : submitLabel}
        </button>
        <button type="button" onClick={onCancel} className="btn-ghost">
          Cancel
        </button>
      </div>
    </form>
  )
}

const LEVEL_COLORS: Record<string, string> = {
  A1: 'bg-mint-100 text-mint-600',
  A2: 'bg-mint-100 text-mint-600',
  B1: 'bg-lavender-100 text-lavender-600',
  B2: 'bg-lavender-100 text-lavender-600',
  C1: 'bg-coral-50 text-coral-600',
  C2: 'bg-coral-50 text-coral-600',
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
      englishDefinition: word.englishDefinition ?? '',
    })
    setShowAddForm(false)
  }

  const readyToPlay = words.length >= 4

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-cream-100 text-ink-400">
        Loading your words…
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-cream-100">
      <Navbar />
      <main className="mx-auto max-w-4xl px-4 py-8 sm:px-6">
        {/* Bento header grid */}
        <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="bento-card col-span-1 bg-mint-100 p-5 sm:col-span-1">
            <p className="text-xs font-semibold uppercase tracking-wide text-mint-600">Total words</p>
            <p className="mt-2 font-display text-3xl font-bold text-ink-700">{words.length}</p>
            <p className="mt-1 text-sm text-ink-500">words in your list</p>
          </div>

          <div className="bento-card bg-lavender-100 p-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-lavender-600">Ready to play?</p>
            <p className="mt-2 font-display text-lg font-bold text-ink-700">
              {readyToPlay ? 'Let\'s go! 🎉' : `Need ${4 - words.length} more`}
            </p>
            <p className="mt-1 text-sm text-ink-500">minimum 4 words to start a game</p>
          </div>

          <div className="bento-card flex flex-col justify-between bg-white p-5">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-coral-500">Quick action</p>
              <p className="mt-2 font-display text-lg font-bold text-ink-700">Add a new word</p>
            </div>
            <button
              onClick={() => { setShowAddForm(true); setEditingWord(null); setInput(EMPTY_INPUT) }}
              className="btn-coral mt-3 w-full"
            >
              + Add word
            </button>
          </div>
        </div>

        {/* Action row */}
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <h2 className="font-display text-xl font-bold text-ink-700">My Words</h2>
          <div className="flex gap-2">
            <Link
              to="/game"
              className={`btn-coral ${!readyToPlay ? 'pointer-events-none opacity-40' : ''}`}
            >
              ▶ Play
            </Link>
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
          <div className="bento-card flex flex-col items-center gap-2 p-10 text-center">
            <span className="text-4xl">🌱</span>
            <p className="font-display text-lg font-semibold text-ink-700">Your word garden is empty</p>
            <p className="text-sm text-ink-400">Add your first word to start growing your vocabulary.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {words.map((word) => (
              <div key={word.id} className={editingWord?.id === word.id ? 'sm:col-span-2' : ''}>
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
                  <div className="bento-card bento-card-hover group p-4">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="font-display text-base font-bold text-ink-700">{word.english}</span>
                          {word.level && (
                            <span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${LEVEL_COLORS[word.level] ?? 'bg-cream-200 text-ink-500'}`}>
                              {word.level}
                            </span>
                          )}
                        </div>
                        <p className="mt-0.5 text-sm text-ink-500">{word.vietnamese}</p>
                        {word.pronunciation && (
                          <p className="mt-0.5 text-xs text-ink-400">/{word.pronunciation}/</p>
                        )}
                        {word.exampleSentence && (
                          <p className="mt-1.5 text-xs italic text-ink-400">"{word.exampleSentence}"</p>
                        )}
                        {word.englishDefinition && (
                          <p className="mt-1 text-xs text-ink-400">📖 {word.englishDefinition}</p>
                        )}
                      </div>
                      <div className="flex shrink-0 gap-1 opacity-0 transition-opacity group-hover:opacity-100">
                        <button
                          onClick={() => startEdit(word)}
                          className="rounded-bento px-2 py-1 text-xs font-semibold text-lavender-600 hover:bg-lavender-50"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(word.id)}
                          className="rounded-bento px-2 py-1 text-xs font-semibold text-coral-500 hover:bg-coral-50"
                        >
                          Delete
                        </button>
                      </div>
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
