import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Navbar } from '../components/layout/Navbar'
import * as chatApi from '../api/chat'
import type { ChatMessage } from '../types'

export function ChatPage() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [sending, setSending] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    chatApi.getChatHistory().then((r) => setMessages(r.data))
  }, [])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim()) return
    const text = input.trim()
    setInput('')
    setSending(true)

    const optimistic: ChatMessage = {
      id: `temp-${Date.now()}`,
      role: 'user',
      content: text,
      createdAt: new Date().toISOString(),
    }
    setMessages((m) => [...m, optimistic])

    try {
      const r = await chatApi.sendChatMessage({ message: text })
      const reply: ChatMessage = {
        id: `reply-${Date.now()}`,
        role: 'assistant',
        content: r.data.reply,
        createdAt: new Date().toISOString(),
      }
      setMessages((m) => [...m, reply])
    } finally {
      setSending(false)
    }
  }

  const handleClear = async () => {
    if (!confirm('Clear chat history?')) return
    await chatApi.clearChatHistory()
    setMessages([])
  }

  return (
    <div className="flex min-h-screen flex-col bg-cream-100">
      <Navbar />
      <div className="mx-auto flex w-full max-w-2xl flex-1 flex-col px-4 py-6 sm:px-6">
        <div className="mb-4 flex items-center justify-between">
          <Link to="/" className="text-sm font-semibold text-lavender-600 hover:underline">← My Words</Link>
          <button onClick={handleClear} className="text-sm font-medium text-ink-400 hover:text-coral-500">
            Clear history
          </button>
        </div>

        <div className="bento-card flex-1 space-y-3 overflow-y-auto p-5 min-h-[420px] max-h-[60vh]">
          {messages.length === 0 && (
            <div className="flex h-full flex-col items-center justify-center gap-2 text-center">
              <span className="text-3xl">🤖</span>
              <p className="text-sm font-medium text-ink-500">
                Ask me to add words, quiz you, or explain vocabulary!
              </p>
            </div>
          )}
          {messages.map((msg) => (
            <div key={msg.id} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-[75%] rounded-bento px-4 py-2.5 text-sm shadow-soft ${
                msg.role === 'user'
                  ? 'bg-coral-500 text-white'
                  : 'bg-lavender-100 text-ink-700'
              }`}>
                {msg.content}
              </div>
            </div>
          ))}
          {sending && (
            <div className="flex justify-start">
              <div className="rounded-bento bg-lavender-50 px-4 py-2.5 text-sm text-ink-400">Thinking…</div>
            </div>
          )}
          <div ref={bottomRef} />
        </div>

        <form onSubmit={handleSend} className="mt-3 flex gap-2">
          <input
            type="text"
            placeholder="Type a message…"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            disabled={sending}
            className="input-pastel flex-1 disabled:opacity-50"
          />
          <button
            type="submit"
            disabled={sending || !input.trim()}
            className="btn-coral"
          >
            Send
          </button>
        </form>
      </div>
    </div>
  )
}
