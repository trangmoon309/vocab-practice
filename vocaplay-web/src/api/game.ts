import { api } from './axios';
import type { GameMode, GamePairs, GameSession, SaveGameSessionInput } from '../types';

export function getGamePairs(mode: GameMode) {
  return api.get<GamePairs>('/game/pairs', { params: { mode } });
}

export function saveGameSession(input: SaveGameSessionInput) {
  return api.post<GameSession>('/game/sessions', input);
}

export function getGameSessions() {
  return api.get<GameSession[]>('/game/sessions');
}
