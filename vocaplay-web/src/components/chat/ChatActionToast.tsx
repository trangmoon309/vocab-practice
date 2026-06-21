import { useEffect } from 'react'
import type { ChatAction } from '../../types'

export function ChatActionToast({ action, onDismiss }: { action: ChatAction; onDismiss: () => void }) {
  useEffect(() => {
    const timer = setTimeout(onDismiss, 4000)
    return () => clearTimeout(timer)
  }, [action, onDismiss])

  if (action.type !== 'BULK_ADD_WORDS') return null

  return (
    <div className="animate-pop-in flex items-center justify-between gap-2 bg-mint-100 px-4 py-2 text-sm font-semibold text-mint-600">
      <span>✅ Added {action.wordsAdded ?? 0} word{action.wordsAdded === 1 ? '' : 's'} to your list!</span>
      <button onClick={onDismiss} className="text-mint-600/70 hover:text-mint-600" aria-label="Dismiss">
        ✕
      </button>
    </div>
  )
}
