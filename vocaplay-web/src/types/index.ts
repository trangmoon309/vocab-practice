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
}

export interface WordInput {
  english: string;
  vietnamese: string;
  pronunciation?: string | null;
  level?: CefrLevel | null;
  type?: WordType | null;
  exampleSentence?: string | null;
}

export interface BulkAddResult {
  added: number;
  skipped: number;
  skippedReasons: string[];
}

// WordSet
export interface WordSet {
  id: string;
  title: string;
  description: string | null;
  wordCount: number;
  createdAt: string;
}

export interface WordSetDetail {
  id: string;
  title: string;
  description: string | null;
  wordCount: number;
  words: Word[];
  createdAt: string;
}

export interface WordSetInput {
  title: string;
  description?: string | null;
}

// Game
export interface GamePairItem {
  id: string;
  english: string;
  vietnamese: string;
}

export interface GamePairs {
  wordSetId: string;
  wordSetTitle: string;
  pairs: GamePairItem[];
}

export interface GameSession {
  id: string;
  wordSetId: string;
  score: number;
  totalPairs: number;
  completedAt: string;
}

export interface SaveGameSessionInput {
  wordSetId: string;
  score: number;
  totalPairs: number;
}

// Chat
export type ChatActionType = 'BULK_ADD_WORDS' | 'QUIZ_START';

export interface ChatAction {
  type: ChatActionType;
  wordSetId: string | null;
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
  wordSetId?: string | null;
}
