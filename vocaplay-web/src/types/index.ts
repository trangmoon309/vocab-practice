// Auth
export interface User {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

// Word
export type CefrLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2';

export type WordType =
  | 'Noun'
  | 'Verb'
  | 'Adjective'
  | 'Adverb'
  | 'Preposition'
  | 'Conjunction'
  | 'Pronoun'
  | 'Interjection';

export interface Word {
  id: string;
  english: string;
  vietnamese: string;
  pronunciation: string | null;
  level: CefrLevel | null;
  type: WordType | null;
  exampleSentence: string | null;
  englishDefinition: string | null;
}

export interface WordInput {
  english: string;
  vietnamese: string;
  pronunciation?: string | null;
  level?: CefrLevel | null;
  type?: WordType | null;
  exampleSentence?: string | null;
  englishDefinition?: string | null;
}

export interface BulkAddResult {
  added: number;
  skipped: number;
  skippedReasons: string[];
}

// Game
export type GameMode = 'Translation' | 'Definition';

export interface GameModeOption {
  mode: GameMode;
  name: string;
  description: string;
  emoji: string;
}

export const GAME_MODES: GameModeOption[] = [
  {
    mode: 'Translation',
    name: 'Translation Match',
    description: 'Match each English word with its Vietnamese meaning.',
    emoji: '🌐',
  },
  {
    mode: 'Definition',
    name: 'Definition Match',
    description: 'Match each English word with its English definition.',
    emoji: '📖',
  },
];

export interface GamePairItem {
  id: string;
  english: string;
  match: string;
}

export interface GamePairs {
  mode: GameMode;
  pairs: GamePairItem[];
}

export interface GameSession {
  id: string;
  score: number;
  totalPairs: number;
  completedAt: string;
}

export interface SaveGameSessionInput {
  score: number;
  totalPairs: number;
}

// Chat
export type ChatActionType = 'BULK_ADD_WORDS' | 'QUIZ_START';

export interface ChatAction {
  type: ChatActionType;
  wordsAdded: number | null;
}

export interface ChatResponse {
  reply: string;
  action: ChatAction | null;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface ChatRequest {
  message: string;
}
