import { api } from './axios';
import type { GamePairs, GameSession, SaveGameSessionInput } from '../types';

export function getGamePairs(setId: string) {
  return api.get<GamePairs>(`/wordsets/${setId}/game`);
}

export function saveGameSession(input: SaveGameSessionInput) {
  return api.post<GameSession>('/game/sessions', input);
}

export function getGameSessions() {
  return api.get<GameSession[]>('/game/sessions');
}
