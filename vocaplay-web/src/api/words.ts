import { api } from './axios';
import type { BulkAddResult, Word, WordInput } from '../types';

export function getWords() {
  return api.get<Word[]>('/words');
}

export function addWord(input: WordInput) {
  return api.post<Word>('/words', input);
}

export function updateWord(wordId: string, input: WordInput) {
  return api.put<Word>(`/words/${wordId}`, input);
}

export function deleteWord(wordId: string) {
  return api.delete<void>(`/words/${wordId}`);
}

export function bulkAddWords(words: WordInput[]) {
  return api.post<BulkAddResult>('/words/bulk', { words });
}
