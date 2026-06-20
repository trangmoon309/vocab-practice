import { api } from './axios';
import type { ChatMessage, ChatRequest, ChatResponse } from '../types';

export function sendChatMessage(request: ChatRequest) {
  return api.post<ChatResponse>('/chat', request);
}

export function getChatHistory() {
  return api.get<ChatMessage[]>('/chat/history');
}

export function clearChatHistory() {
  return api.delete<void>('/chat/history');
}
