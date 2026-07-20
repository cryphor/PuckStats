// PuckStats API client — typed fetch wrapper for the backend

const API_BASE = process.env.NEXT_PUBLIC_API_URL || '';

async function fetchApi<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ error: 'Request failed' }));
    throw new Error(error.error || `HTTP ${res.status}`);
  }

  return res.json();
}

// Types
export interface PlayerProfile {
  steamId: string;
  username: string;
  avatarUrl: string;
  ratings: PlayerRatings;
  percentiles: Percentiles;
  archetype: string;
  scoutingReport: ScoutingReport;
  // Basic
  totalMatches: number;
  totalWins: number;
  totalLosses: number;
  totalOtl: number;
  // Scoring
  totalGoals: number;
  totalAssists: number;
  totalPoints: number;
  plusMinus: number;
  totalPim: number;
  totalTimeOnIce: number;
  totalShots: number;
  totalShotAttempts: number;
  totalHits: number;
  // Shooting detail
  highDangerGoals: number;
  lowDangerGoals: number;
  powerplayGoals: number;
  shorthandedGoals: number;
  gameWinningGoals: number;
  // Passing
  primaryAssists: number;
  secondaryAssists: number;
  totalPasses: number;
  passesCompleted: number;
  dangerousPasses: number;
  // Faceoffs
  faceoffsTaken: number;
  faceoffsWon: number;
  // Possession
  totalPossessionTime: number;
  totalPuckTouches: number;
  turnovers: number;
  recoveries: number;
  // Defense
  totalBlocks: number;
  totalInterceptions: number;
  takeaways: number;
  stickChecks: number;
  pokeChecks: number;
  // Special teams
  powerplayAssists: number;
  shorthandedAssists: number;
  // On-ice
  onIceGoalsFor: number;
  onIceGoalsAgainst: number;
  // Legacy
  totalSaves: number;
  winRate: number;
  recentMatches: RecentMatch[];
  lastUpdated: string;
}

export interface PlayerRatings {
  skating: number;
  shooting: number;
  stickhandling: number;
  passing: number;
  inputs: number;
  stickMotion: number;
  offensivePlay: number;
  defensivePlay: number;
  positioning: number;
  gameSense: number;
  overall: number;
  archetype: string;
}

export interface Percentiles {
  skatingPercentile: number;
  shootingPercentile: number;
  stickhandlingPercentile: number;
  passingPercentile: number;
  inputsPercentile: number;
  stickMotionPercentile: number;
  offensivePlayPercentile: number;
  defensivePlayPercentile: number;
  positioningPercentile: number;
  gameSensePercentile: number;
  overallPercentile: number;
}

export interface ScoutingReport {
  strengths: string[];
  weaknesses: string[];
  overallRating: number;
  archetype: string;
  archetypeDescription: string;
  summary: string;
  keyMoments: KeyMoment[];
  improvementSuggestions: string[];
}

export interface KeyMoment {
  timestamp: number;
  type: string;
  description: string;
  severity: string;
}

export interface RecentMatch {
  matchId: string;
  date: string;
  type: string;
  team: string;
  teamScore: number;
  opponentScore: number;
  goals: number;
  assists: number;
  rating: number;
}

export interface DashboardData {
  overallRating: number;
  ratings: PlayerRatings;
  percentiles: Percentiles;
  trends: RatingTrend[];
  recentGames: RecentMatch[];
  sessionsThisWeek: number;
  hoursPlayed: number;
}

export interface RatingTrend {
  category: string;
  points: TrendPoint[];
}

export interface TrendPoint {
  date: string;
  rating: number;
}

export interface LeaderboardEntry {
  rank: number;
  steamId: string;
  username: string;
  rating: number;
  matchesPlayed: number;
  archetype: string;
}

export interface HeatmapData {
  type: string;
  rinkWidth: number;
  rinkHeight: number;
  cells: HeatmapCell[];
  maxIntensity: number;
}

export interface HeatmapCell {
  x: number;
  y: number;
  intensity: number;
}

export interface PassingNetwork {
  nodes: PassingNode[];
  edges: PassingEdge[];
}

export interface PassingNode {
  steamId: string;
  username: string;
  position: { x: number; y: number };
  influence: number;
}

export interface PassingEdge {
  sourceSteamId: string;
  targetSteamId: string;
  passCount: number;
  completedCount: number;
  successRate: number;
}

export interface CompareResult {
  playerA: PlayerProfile;
  playerB: PlayerProfile;
  differences: Record<string, number>;
}

// API Functions
export const api = {
  getPlayer: (steamId: string) =>
    fetchApi<PlayerProfile>(`/api/player/${steamId}`),

  getPlayerAnalytics: (steamId: string) =>
    fetchApi<any>(`/api/player/${steamId}/analytics`),

  getRatingHistory: (steamId: string, days = 90) =>
    fetchApi<RatingTrend[]>(`/api/player/${steamId}/rating-history?days=${days}`),

  getDashboard: (steamId: string) =>
    fetchApi<DashboardData>(`/api/player/${steamId}/dashboard`),

  getLeaderboard: (category = 'Overall', page = 1, pageSize = 50) =>
    fetchApi<LeaderboardEntry[]>(`/api/player/leaderboard/${category}?page=${page}&pageSize=${pageSize}`),

  comparePlayers: (playerA: string, playerB: string) =>
    fetchApi<CompareResult>(`/api/player/compare?playerA=${playerA}&playerB=${playerB}`),

  getPlayerReplays: (steamId: string, page = 1) =>
    fetchApi<any[]>(`/api/player/${steamId}/replays?page=${page}`),

  uploadReplay: (file: File, steamId?: string) => {
    const formData = new FormData();
    formData.append('file', file);
    if (steamId) formData.append('steamId', steamId);
    return fetch(`${API_BASE}/api/replay/upload`, { method: 'POST', body: formData }).then(r => r.json());
  },

  getReplay: (replayId: number) =>
    fetchApi<any>(`/api/replay/${replayId}`),

  analyzeReplay: (replayId: number) =>
    fetchApi<any>(`/api/replay/${replayId}/analyze`, { method: 'POST' }),

  getGoalBreakdowns: (replayId: number) =>
    fetchApi<any[]>(`/api/replay/${replayId}/goals`),

  getShiftAnalytics: (replayId: number, steamId: string) =>
    fetchApi<any[]>(`/api/replay/${replayId}/shifts/${steamId}`),
};
