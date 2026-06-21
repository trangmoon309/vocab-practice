import { useEffect, useState } from 'react'
import * as chatApi from '../api/chat'
import type { ChatAction, ChatMessage } from '../types'

export function useChat(enabled: boolean) {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [loaded, setLoaded] = useState(false)
  const [sending, setSending] = useState(false)
  const [lastAction, setLastAction] = useState<ChatAction | null>(null)

  useEffect(() => {
    if (!enabled || loaded) return
    chatApi.getChatHistory().then((r) => setMessages(r.data)).finally(() => setLoaded(true))
  }, [enabled, loaded])

  const sendMessage = async (text: string) => {
    const trimmed = text.trim()
    if (!trimmed) return

    const optimistic: ChatMessage = {
      id: `temp-${Date.now()}`,
      role: 'user',
      content: trimmed,
      createdAt: new Date().toISOString(),
    }
    setMessages((m) => [...m, optimistic])
    setSending(true)

    try {
      const r = await chatApi.sendChatMessage({ message: trimmed })
      const reply: ChatMessage = {
        id: `reply-${Date.now()}`,
        role: 'assistant',
        content: r.data.reply,
        createdAt: new Date().toISOString(),
      }
      setMessages((m) => [...m, reply])
      setLastAction(r.data.action)
    } finally {
      setSending(false)
    }
  }

  const clearHistory = async () => {
    await chatApi.clearChatHistory()
    setMessages([])
    setLastAction(null)
  }

  const dismissAction = () => setLastAction(null)

  return { messages, sending, sendMessage, clearHistory, lastAction, dismissAction }
}
