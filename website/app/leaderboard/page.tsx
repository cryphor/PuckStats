'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { api, type LeaderboardEntry } from '@/lib/api';
import { Trophy, Loader2, ChevronLeft, ChevronRight } from 'lucide-react';

const CATEGORIES = ['Overall', 'Skating', 'Shooting', 'Stickhandling', 'Passing', 'Inputs', 'GameSense'];

export default function LeaderboardPage() {
  const [category, setCategory] = useState('Overall');
  const [page, setPage] = useState(1);
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api.getLeaderboard(category, page, 50)
      .then(setEntries)
      .finally(() => setLoading(false));
  }, [category, page]);

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-2xl font-bold text-text mb-2 flex items-center gap-2">
        <Trophy size={24} className="text-accent" />
        Leaderboards
      </h1>
      <p className="text-sm text-muted mb-6">Global rankings across all skill categories</p>

      {/* Category tabs */}
      <div className="flex items-center gap-0 border-b border-border mb-6 overflow-x-auto">
        {CATEGORIES.map((cat) => (
          <button
            key={cat}
            onClick={() => { setCategory(cat); setPage(1); }}
            className={`tab whitespace-nowrap ${category === cat ? 'active' : ''}`}
          >
            {cat}
          </button>
        ))}
      </div>

      {/* Table */}
      {loading ? (
        <div className="flex justify-center py-20">
          <Loader2 size={32} className="text-accent animate-spin" />
        </div>
      ) : (
        <div className="glass-panel overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left text-xs text-muted font-medium px-4 py-3 w-16">Rank</th>
                <th className="text-left text-xs text-muted font-medium px-4 py-3">Player</th>
                <th className="text-center text-xs text-muted font-medium px-4 py-3 w-20">Rating</th>
                <th className="text-center text-xs text-muted font-medium px-4 py-3 w-24">Matches</th>
                <th className="text-right text-xs text-muted font-medium px-4 py-3 w-32">Archetype</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((entry, i) => (
                <tr
                  key={entry.steamId}
                  className="border-b border-border/50 hover:bg-accent/5 transition-colors"
                >
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <span className={`text-sm font-bold tabular-nums ${
                        entry.rank <= 3 ? 'text-accent text-glow' : 'text-muted'
                      }`}>
                        #{entry.rank}
                      </span>
                      {entry.rank <= 3 && (
                        <Trophy
                          size={14}
                          className={
                            entry.rank === 1 ? 'text-yellow-400' :
                            entry.rank === 2 ? 'text-gray-300' :
                            'text-orange-400'
                          }
                        />
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <Link href={`/player/${entry.steamId}`} className="text-sm text-text hover:text-accent transition-colors font-medium">
                      {entry.username || entry.steamId.substring(0, 12)}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className="text-sm font-bold text-text tabular-nums">{entry.rating}</span>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className="text-xs text-muted">{entry.matchesPlayed}</span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <span className="text-xs text-muted">{entry.archetype}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="flex items-center justify-between px-4 py-3 border-t border-border">
            <button
              onClick={() => setPage(Math.max(1, page - 1))}
              disabled={page === 1}
              className="btn-ghost p-1.5 disabled:opacity-30"
            >
              <ChevronLeft size={16} />
            </button>
            <span className="text-xs text-muted">Page {page}</span>
            <button
              onClick={() => setPage(page + 1)}
              disabled={entries.length < 50}
              className="btn-ghost p-1.5 disabled:opacity-30"
            >
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
