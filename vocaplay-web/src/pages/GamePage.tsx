import { useEffect, useState, useCallback } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as gameApi from '../api/game'
import { GAME_MODES, type GameMode, type GamePairItem } from '../types'

type Card = { id: string; text: string; side: 'left' | 'right'; pairId: string }
type CardState = 'idle' | 'selected' | 'matched' | 'wrong'

function getErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const response = (err as { response?: { data?: { message?: string } } }).response
    if (response?.data?.message) return response.data.message
  }
  return 'Something went wrong loading the game. Please try again.'
}

export function GamePage() {
  const [searchParams] = useSearchParams()
  const mode = (searchParams.get('mode') as GameMode) || 'Translation'
  const gameInfo = GAME_MODES.find((g) => g.mode === mode) ?? GAME_MODES[0]

  const [cards, setCards] = useState<Card[]>([])
  const [cardStates, setCardStates] = useState<Record<string, CardState>>({})
  const [selected, setSelected] = useState<Card | null>(null)
  const [score, setScore] = useState(0)
  const [total, setTotal] = useState(0)
  const [done, setDone] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const buildCards = useCallback((pairs: GamePairItem[]) => {
    const shuffled = [...pairs]
      .sort(() => Math.random() - 0.5)
      .slice(0, 8)

    const all: Card[] = [
      ...shuffled.map((p) => ({ id: `l-${p.id}`, text: p.english, side: 'left' as const, pairId: p.id })),
      ...shuffled.map((p) => ({ id: `r-${p.id}`, text: p.match, side: 'right' as const, pairId: p.id })),
    ].sort(() => Math.random() - 0.5)

    setCards(all)
    setTotal(shuffled.length)
    const states: Record<string, CardState> = {}
    all.forEach((c) => (states[c.id] = 'idle'))
    setCardStates(states)
  }, [])

  const loadGame = useCallback(() => {
    setLoading(true)
    setError(null)
    gameApi.getGamePairs(mode)
      .then((r) => buildCards(r.data.pairs))
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }, [mode, buildCards])

  useEffect(() => {
    loadGame()
  }, [loadGame])

  const handleSelect = (card: Card) => {
    if (cardStates[card.id] === 'matched' || cardStates[card.id] === 'selected') return

    if (!selected) {
      setSelected(card)
      setCardStates((s) => ({ ...s, [card.id]: 'selected' }))
      return
    }

    if (selected.id === card.id) return

    if (selected.pairId === card.pairId && selected.side !== card.side) {
      // match
      const newStates = { ...cardStates, [selected.id]: 'matched' as const, [card.id]: 'matched' as const }
      setCardStates(newStates)
      setSelected(null)
      const newScore = score + 1
      setScore(newScore)
      if (newScore === total) {
        setDone(true)
        gameApi.saveGameSession({ score: newScore, totalPairs: total }).catch(() => {})
      }
    } else {
      // wrong
      setCardStates((s) => ({ ...s, [selected.id]: 'wrong', [card.id]: 'wrong' }))
      setTimeout(() => {
        setCardStates((s) => ({ ...s, [selected.id]: 'idle', [card.id]: 'idle' }))
        setSelected(null)
      }, 700)
    }
  }

  const stateClasses: Record<CardState, string> = {
    idle: 'bg-white border-lavender-100 hover:border-lavender-300 hover:shadow-soft-hover hover:-translate-y-0.5 cursor-pointer',
    selected: 'bg-lavender-100 border-lavender-400 shadow-soft-hover cursor-pointer',
    matched: 'bg-mint-100 border-mint-300 opacity-60 cursor-default scale-[0.97]',
    wrong: 'bg-coral-50 border-coral-300 cursor-default animate-pulse',
  }

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-cream-100 text-ink-400">
        Shuffling your words…
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-cream-100">
      <Navbar />
      <main className="mx-auto max-w-2xl px-4 py-8 sm:px-6">
        <div className="mb-4">
          <Link to="/game" className="text-sm font-semibold text-lavender-600 hover:underline">← Choose a game</Link>
        </div>

        {error ? (
          <div className="bento-card animate-pop-in p-10 text-center">
            <p className="text-4xl">🤔</p>
            <p className="mt-3 font-display text-lg font-bold text-ink-700">Can't start this game yet</p>
            <p className="mt-1 text-sm text-ink-500">{error}</p>
            <div className="mt-6 flex justify-center gap-3">
              <Link to="/" className="btn-coral">Add more words</Link>
              <Link to="/game" className="btn-ghost">Choose another game</Link>
            </div>
          </div>
        ) : done ? (
          <div className="bento-card animate-pop-in p-10 text-center">
            <p className="text-5xl">🎉</p>
            <p className="mt-3 font-display text-2xl font-bold text-ink-700">All matched!</p>
            <p className="mt-1 text-ink-500">{score} / {total} pairs — nice work!</p>
            <div className="mt-6 flex justify-center gap-3">
              <button onClick={() => { setDone(false); setScore(0); loadGame() }} className="btn-coral">
                Play again
              </button>
              <Link to="/game" className="btn-ghost">
                Choose another game
              </Link>
            </div>
          </div>
        ) : (
          <>
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-display text-lg font-bold text-ink-700">{gameInfo.emoji} {gameInfo.name}</h2>
              <span className="rounded-full bg-lavender-100 px-3 py-1 text-sm font-semibold text-lavender-600">
                {score} / {total} matched
              </span>
            </div>
            <div className="grid grid-cols-4 gap-3">
              {cards.map((card) => (
                <button
                  key={card.id}
                  onClick={() => handleSelect(card)}
                  className={`rounded-bento border-2 p-3 text-sm font-semibold text-ink-700 transition-all duration-150 ${stateClasses[cardStates[card.id]]}`}
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
