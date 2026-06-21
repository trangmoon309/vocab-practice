import { useEffect, useRef, useState } from 'react'
import { ChatMessageBubble } from './ChatMessageBubble'
import type { ChatMessage } from '../../types'

export function ChatWindow({
  messages,
  sending,
  onSend,
  onClear,
}: {
  messages: ChatMessage[]
  sending: boolean
  onSend: (text: string) => Promise<void>
  onClear: () => Promise<void>
}) {
  const [input, setInput] = useState('')
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, sending])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim()) return
    const text = input
    setInput('')
    await onSend(text)
  }

  return (
    <div className="flex h-full flex-col">
      <div className="flex-1 space-y-3 overflow-y-auto p-4">
        {messages.length === 0 && (
          <div className="flex h-full flex-col items-center justify-center gap-2 text-center">
            <span className="text-3xl">🤖</span>
            <p className="text-sm font-medium text-ink-500">
              Ask me to add words, quiz you, or explain vocabulary!
            </p>
          </div>
        )}
        {messages.map((msg) => (
          <ChatMessageBubble key={msg.id} message={msg} />
        ))}
        {sending && (
          <div className="flex justify-start">
            <div className="rounded-bento bg-lavender-50 px-3.5 py-2 text-sm text-ink-400">Thinking…</div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      <div className="flex items-center justify-between border-t border-lavender-100 px-4 py-2">
        <button onClick={onClear} className="text-xs font-medium text-ink-400 hover:text-coral-500">
          Clear history
        </button>
      </div>

      <form onSubmit={handleSubmit} className="flex gap-2 border-t border-lavender-100 p-3">
        <input
          type="text"
          placeholder="Type a message…"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          disabled={sending}
          className="input-pastel flex-1 disabled:opacity-50"
        />
        <button type="submit" disabled={sending || !input.trim()} className="btn-coral px-4">
          Send
        </button>
      </form>
    </div>
  )
}
