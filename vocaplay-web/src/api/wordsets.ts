import { api } from './axios';
import type {
  BulkAddResult,
  WordInput,
  WordSet,
  WordSetDetail,
  WordSetInput,
} from '../types';

export function getWordSets() {
  return api.get<WordSet[]>('/wordsets');
}

export function getWordSet(id: string) {
  return api.get<WordSetDetail>(`/wordsets/${id}`);
}

export function createWordSet(input: WordSetInput) {
  return api.post<WordSet>('/wordsets', input);
}

export function updateWordSet(id: string, input: WordSetInput) {
  return api.put<WordSet>(`/wordsets/${id}`, input);
}

export function deleteWordSet(id: string) {
  return api.delete<void>(`/wordsets/${id}`);
}

export function addWord(setId: string, input: WordInput) {
  return api.post<WordSetDetail>(`/wordsets/${setId}/words`, input);
}

export function updateWord(setId: string, wordId: string, input: WordInput) {
  return api.put<WordSetDetail>(`/wordsets/${setId}/words/${wordId}`, input);
}

export function deleteWord(setId: string, wordId: string) {
  return api.delete<void>(`/wordsets/${setId}/words/${wordId}`);
}

export function bulkAddWords(setId: string, words: WordInput[]) {
  return api.post<BulkAddResult>(`/wordsets/${setId}/words/bulk`, { words });
}
