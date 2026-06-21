import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import { useChat } from '../../hooks/useChat'
import { ChatWindow } from './ChatWindow'
import { ChatActionToast } from './ChatActionToast'

const HIDDEN_ROUTES = ['/login', '/register']

export function ChatWidget() {
  const { isAuthenticated } = useAuth()
  const location = useLocation()
  const [open, setOpen] = useState(false)
  const enabled = isAuthenticated && !HIDDEN_ROUTES.includes(location.pathname)
  const { messages, sending, sendMessage, clearHistory, lastAction, dismissAction } = useChat(enabled && open)

  if (!enabled) return null

  return (
    <>
      {open && (
        <div className="bento-card animate-pop-in fixed bottom-24 right-4 z-40 flex h-[480px] w-[340px] flex-col overflow-hidden border border-lavender-100 sm:right-6">
          <div className="flex items-center justify-between bg-lavender-100 px-4 py-3">
            <span className="font-display text-sm font-bold text-ink-700">🤖 AI Tutor</span>
            <button
              onClick={() => setOpen(false)}
              className="rounded-bento px-2 py-1 text-sm text-ink-500 hover:bg-white/60"
              aria-label="Close chat"
            >
              ✕
            </button>
          </div>
          {lastAction && <ChatActionToast action={lastAction} onDismiss={dismissAction} />}
          <div className="flex-1 overflow-hidden">
            <ChatWindow messages={messages} sending={sending} onSend={sendMessage} onClear={clearHistory} />
          </div>
        </div>
      )}

      <button
        onClick={() => setOpen((o) => !o)}
        className="fixed bottom-5 right-4 z-40 flex h-14 w-14 items-center justify-center rounded-full bg-coral-500 text-2xl text-white shadow-soft-lg transition-transform duration-150 hover:scale-105 hover:bg-coral-600 active:scale-95 sm:right-6"
        aria-label={open ? 'Close chat' : 'Open chat'}
      >
        {open ? '✕' : '💬'}
      </button>
    </>
  )
}
