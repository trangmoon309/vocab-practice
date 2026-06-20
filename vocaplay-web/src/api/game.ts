import { api } from './axios';
import type { GamePairs, GameSession, SaveGameSessionInput } from '../types';

export function getGamePairs() {
  return api.get<GamePairs>('/game/pairs');
}

export function saveGameSession(input: SaveGameSessionInput) {
  return api.post<GameSession>('/game/sessions', input);
}

export function getGameSessions() {
  return api.get<GameSession[]>('/game/sessions');
}
