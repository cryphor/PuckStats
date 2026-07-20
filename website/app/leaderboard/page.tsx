'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { api, type LeaderboardEntry } from '@/lib/api';

const CATS = ['Overall', 'Skating', 'Shooting', 'Stickhandling', 'Passing', 'Inputs', 'GameSense'];

export default function LeaderboardPage() {
  const [cat, setCat] = useState('Overall');
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api.getLeaderboard(cat, 1, 100).then(setEntries).finally(() => setLoading(false));
  }, [cat]);

  return (
    <div className="max-w-[1600px] mx-auto px-2 py-3">
      <div className="flex items-center gap-3 mb-3">
        <h1 className="text-sm font-bold text-[#e8e8e8]">Leaderboard</h1>
        <div className="flex items-center gap-0.5 border border-[#1d1d1d] rounded overflow-hidden">
          {CATS.map(c => (
            <button key={c} onClick={() => setCat(c)} className={`px-2.5 py-1 text-[11px] font-medium transition-colors ${cat === c ? 'bg-[#4ade80]/10 text-[#4ade80]' : 'text-[#8a8a8a] hover:text-[#e8e8e8]'}`}>{c}</button>
          ))}
        </div>
      </div>

      <div className="panel rounded overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-[#1d1d1d] bg-[#0b0b0b]">
              <th className="table-header w-10">#</th>
              <th className="table-header">Player</th>
              <th className="table-header w-16 text-right">Rating</th>
              <th className="table-header w-16 text-right">Matches</th>
              <th className="table-header w-24 text-right">Archetype</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} className="p-3 text-center text-xs text-[#555]">Loading...</td></tr>
            ) : entries.length === 0 ? (
              <tr><td colSpan={5} className="p-3 text-center text-xs text-[#555]">No data yet.</td></tr>
            ) : entries.map((e, i) => (
              <tr key={e.steamId} className="table-row">
                <td className={`table-cell text-center text-xs font-mono ${i < 3 ? 'text-[#4ade80]' : 'text-[#555]'}`}>{i + 1}</td>
                <td className="table-cell">
                  <Link href={`/player/${e.steamId}`} className="text-xs text-[#e8e8e8] hover:text-[#4ade80]">{e.username || e.steamId.substring(0, 12)}</Link>
                </td>
                <td className="table-cell text-right font-mono text-[#e8e8e8]">{e.rating}</td>
                <td className="table-cell text-right text-[#555] font-mono">{e.matchesPlayed}</td>
                <td className="table-cell text-right text-[#555] text-[10px]">{e.archetype}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
