'use client';

import { useState } from 'react';
import { api, type CompareResult } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';
import { Loader2, ArrowLeftRight, Search } from 'lucide-react';

export default function ComparePage() {
  const [playerA, setPlayerA] = useState('');
  const [playerB, setPlayerB] = useState('');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CompareResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleCompare = (e: React.FormEvent) => {
    e.preventDefault();
    if (!playerA.trim() || !playerB.trim()) return;
    setLoading(true);
    setError(null);
    api.comparePlayers(playerA.trim(), playerB.trim())
      .then(setResult)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-2xl font-bold text-text mb-2 flex items-center gap-2">
        <ArrowLeftRight size={24} className="text-accent" />
        Compare Players
      </h1>
      <p className="text-sm text-muted mb-6">Side-by-side comparison of two players</p>

      <form onSubmit={handleCompare} className="glass-panel p-4 mb-6">
        <div className="flex flex-col md:flex-row items-center gap-3">
          <div className="relative flex-1 w-full">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-dim" />
            <input
              type="text"
              placeholder="Player A Steam ID..."
              value={playerA}
              onChange={(e) => setPlayerA(e.target.value)}
              className="input pl-9 w-full"
            />
          </div>
          <div className="text-muted text-sm font-bold">vs</div>
          <div className="relative flex-1 w-full">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-dim" />
            <input
              type="text"
              placeholder="Player B Steam ID..."
              value={playerB}
              onChange={(e) => setPlayerB(e.target.value)}
              className="input pl-9 w-full"
            />
          </div>
          <button type="submit" className="btn btn-primary flex-shrink-0">
            Compare
          </button>
        </div>
      </form>

      {loading && (
        <div className="flex justify-center py-20">
          <Loader2 size={32} className="text-accent animate-spin" />
        </div>
      )}
      {error && (
        <div className="glass-panel p-8 text-center"><p className="text-danger text-sm">{error}</p></div>
      )}

      {result && !loading && (
        <div className="grid lg:grid-cols-2 gap-6">
          {/* Player A */}
          <PlayerSummary player={result.playerA} side="A" />
          {/* Player B */}
          <PlayerSummary player={result.playerB} side="B" />
        </div>
      )}
    </div>
  );
}

function PlayerSummary({ player, side }: { player: any; side: string }) {
  if (!player?.ratings) return null;

  const radarData = [
    { category: 'Skating', value: player.ratings.skating, fullMark: 100 },
    { category: 'Shooting', value: player.ratings.shooting, fullMark: 100 },
    { category: 'Stickhandling', value: player.ratings.stickhandling, fullMark: 100 },
    { category: 'Passing', value: player.ratings.passing, fullMark: 100 },
    { category: 'Inputs', value: player.ratings.inputs, fullMark: 100 },
    { category: 'G.Sense', value: player.ratings.gameSense, fullMark: 100 },
  ];

  return (
    <div className="glass-panel p-5 space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-bold text-text">{player.username || side}</h3>
          <p className="text-xs text-muted">{player.archetype}</p>
        </div>
        <div className="text-3xl font-extrabold text-accent">{player.ratings.overall}</div>
      </div>
      <div className="flex justify-center">
        <RadarChart data={radarData} size={200} />
      </div>
      <div className="space-y-1.5">
        {Object.entries(player.ratings)
          .filter(([k]) => !['archetype', 'stickMotion'].includes(k as string))
          .slice(0, 6)
          .map(([key, value]) => (
            <div key={key} className="flex items-center gap-2">
              <span className="text-xs text-muted w-24">{key.replace(/([A-Z])/g, ' $1')}</span>
              <div className="flex-1 progress-bar"><div className="fill bg-accent" style={{width: `${value}%`}} /></div>
              <span className="text-xs font-mono w-8">{value as number}</span>
            </div>
          ))}
      </div>
    </div>
  );
}
