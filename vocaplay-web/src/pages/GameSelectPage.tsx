import { Link } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import { GAME_MODES } from '../types'

export function GameSelectPage() {
  return (
    <div className="min-h-screen bg-cream-100">
      <Navbar />
      <main className="mx-auto max-w-3xl px-4 py-8 sm:px-6">
        <div className="mb-2">
          <Link to="/" className="text-sm font-semibold text-lavender-600 hover:underline">← My Words</Link>
        </div>
        <h2 className="mb-1 font-display text-xl font-bold text-ink-700">Choose a game</h2>
        <p className="mb-6 text-sm text-ink-500">Pick how you want to practice your words today.</p>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {GAME_MODES.map((game) => (
            <Link
              key={game.mode}
              to={`/game/play?mode=${game.mode}`}
              className="bento-card bento-card-hover group flex flex-col items-start gap-3 p-6"
            >
              <span className="flex h-12 w-12 items-center justify-center rounded-bento bg-lavender-100 text-2xl">
                {game.emoji}
              </span>
              <div>
                <h3 className="font-display text-lg font-bold text-ink-700">{game.name}</h3>
                <p className="mt-1 text-sm text-ink-500">{game.description}</p>
              </div>
              <span className="mt-auto text-sm font-semibold text-coral-500 group-hover:underline">
                Play →
              </span>
            </Link>
          ))}
        </div>
      </main>
    </div>
  )
}
