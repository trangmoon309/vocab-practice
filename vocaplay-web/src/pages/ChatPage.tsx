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
    <div className="flex min-h-screen flex-col bg-gray-50">
      <Navbar />
      <div className="mx-auto flex w-full max-w-2xl flex-1 flex-col px-4 py-6">
        <div className="mb-4 flex items-center justify-between">
          <Link to="/" className="text-sm text-indigo-600 hover:underline">← My Words</Link>
          <button onClick={handleClear} className="text-sm text-gray-400 hover:text-red-500">
            Clear history
          </button>
        </div>

        <div className="flex-1 overflow-y-auto rounded-lg bg-white shadow p-4 space-y-3 min-h-[400px] max-h-[60vh]">
          {messages.length === 0 && (
            <p className="text-center text-sm text-gray-400 mt-8">
              Ask me to add words, quiz you, or explain vocabulary!
            </p>
          )}
          {messages.map((msg) => (
            <div key={msg.id} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-[75%] rounded-lg px-4 py-2 text-sm ${
                msg.role === 'user'
                  ? 'bg-indigo-600 text-white'
                  : 'bg-gray-100 text-gray-800'
              }`}>
                {msg.content}
              </div>
            </div>
          ))}
          {sending && (
            <div className="flex justify-start">
              <div className="rounded-lg bg-gray-100 px-4 py-2 text-sm text-gray-400">Thinking…</div>
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
            className="flex-1 rounded border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none disabled:opacity-50"
          />
          <button
            type="submit"
            disabled={sending || !input.trim()}
            className="rounded bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700 disabled:opacity-50"
          >
            Send
          </button>
        </form>
      </div>
    </div>
  )
}
