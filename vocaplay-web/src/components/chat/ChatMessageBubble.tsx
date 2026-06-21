import type { ChatMessage } from '../../types'

export function ChatMessageBubble({ message }: { message: ChatMessage }) {
  const isUser = message.role === 'user'
  return (
    <div className={`flex ${isUser ? 'justify-end' : 'justify-start'}`}>
      <div
        className={`max-w-[80%] rounded-bento px-3.5 py-2 text-sm shadow-soft ${
          isUser ? 'bg-coral-500 text-white' : 'bg-lavender-100 text-ink-700'
        }`}
      >
        {message.content}
      </div>
    </div>
  )
}
