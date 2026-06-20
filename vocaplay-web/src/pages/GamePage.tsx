import { useEffect, useState, useCallback } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as gameApi from '../api/game'
import type { GamePairItem } from '../types'

type Card = { id: string; text: string; lang: 'en' | 'vi'; pairId: string }
type CardState = 'idle' | 'selected' | 'matched' | 'wrong'

export function GamePage() {
  const { id } = useParams<{ id: string }>()
  const [cards, setCards] = useState<Card[]>([])
  const [cardStates, setCardStates] = useState<Record<string, CardState>>({})
  const [selected, setSelected] = useState<Card | null>(null)
  const [score, setScore] = useState(0)
  const [total, setTotal] = useState(0)
  const [done, setDone] = useState(false)
  const [title, setTitle] = useState('')
  const [loading, setLoading] = useState(true)

  const buildCards = useCallback((pairs: GamePairItem[]) => {
    const shuffled = [...pairs]
      .sort(() => Math.random() - 0.5)
      .slice(0, 8)

    const all: Card[] = [
      ...shuffled.map((p) => ({ id: `en-${p.id}`, text: p.english, lang: 'en' as const, pairId: p.id })),
      ...shuffled.map((p) => ({ id: `vi-${p.id}`, text: p.vietnamese, lang: 'vi' as const, pairId: p.id })),
    ].sort(() => Math.random() - 0.5)

    setCards(all)
    setTotal(shuffled.length)
    const states: Record<string, CardState> = {}
    all.forEach((c) => (states[c.id] = 'idle'))
    setCardStates(states)
  }, [])

  useEffect(() => {
    if (!id) return
    gameApi.getGamePairs(id).then((r) => {
      setTitle(r.data.wordSetTitle)
      buildCards(r.data.pairs)
    }).finally(() => setLoading(false))
  }, [id, buildCards])

  const handleSelect = (card: Card) => {
    if (cardStates[card.id] === 'matched' || cardStates[card.id] === 'selected') return

    if (!selected) {
      setSelected(card)
      setCardStates((s) => ({ ...s, [card.id]: 'selected' }))
      return
    }

    if (selected.id === card.id) return

    if (selected.pairId === card.pairId && selected.lang !== card.lang) {
      // match
      const newStates = { ...cardStates, [selected.id]: 'matched' as const, [card.id]: 'matched' as const }
      setCardStates(newStates)
      setSelected(null)
      const newScore = score + 1
      setScore(newScore)
      if (newScore === total) {
        setDone(true)
        gameApi.saveGameSession({ wordSetId: id!, score: newScore, totalPairs: total }).catch(() => {})
      }
    } else {
      // wrong
      setCardStates((s) => ({ ...s, [selected.id]: 'wrong', [card.id]: 'wrong' }))
      setTimeout(() => {
        setCardStates((s) => ({ ...s, [selected.id]: 'idle', [card.id]: 'idle' }))
        setSelected(null)
      }, 800)
    }
  }

  const stateClasses: Record<CardState, string> = {
    idle: 'bg-white border-gray-200 hover:border-indigo-400 hover:shadow cursor-pointer',
    selected: 'bg-indigo-50 border-indigo-500 cursor-pointer',
    matched: 'bg-green-50 border-green-400 opacity-60 cursor-default',
    wrong: 'bg-red-50 border-red-400 cursor-default',
  }

  if (loading) return <div className="flex h-screen items-center justify-center text-gray-400">Loading…</div>

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="mx-auto max-w-2xl px-4 py-8">
        <div className="mb-4 flex items-center gap-2">
          <Link to={`/wordsets/${id}`} className="text-sm text-indigo-600 hover:underline">← {title}</Link>
        </div>

        {done ? (
          <div className="rounded-lg bg-white p-10 text-center shadow">
            <p className="text-4xl font-bold text-green-600">🎉</p>
            <p className="mt-2 text-xl font-semibold">All matched!</p>
            <p className="mt-1 text-gray-500">{score} / {total} pairs</p>
            <div className="mt-6 flex justify-center gap-3">
              <button
                onClick={() => { setDone(false); setScore(0); setLoading(true); gameApi.getGamePairs(id!).then((r) => { buildCards(r.data.pairs); setLoading(false) }) }}
                className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700"
              >
                Play again
              </button>
              <Link to={`/wordsets/${id}`} className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50">
                Back to set
              </Link>
            </div>
          </div>
        ) : (
          <>
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-semibold text-gray-700">Match the pairs</h2>
              <span className="text-sm text-gray-400">{score} / {total} matched</span>
            </div>
            <div className="grid grid-cols-4 gap-3">
              {cards.map((card) => (
                <button
                  key={card.id}
                  onClick={() => handleSelect(card)}
                  className={`rounded-lg border-2 p-3 text-sm font-medium text-gray-700 transition-all ${stateClasses[cardStates[card.id]]}`}
                >
                  {card.text}
                </button>
              ))}
            </div>
          </>
        )}
      </main>
    </div>
  )
}
