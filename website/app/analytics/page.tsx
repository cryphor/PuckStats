'use client';

import { useState } from 'react';
import { api } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';
import { Heatmap } from '@/components/Heatmap';
import { RatingTrendChart } from '@/components/RatingTrendChart';

export default function AnalyticsPage() {
  const [sid, setSid] = useState('');
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (!sid.trim()) return;
    setLoading(true); setError(null);
    Promise.all([api.getPlayerAnalytics(sid.trim()), api.getRatingHistory(sid.trim(), 90)])
      .then(([analytics, history]) => setData({ ...analytics, ratingHistory: history }))
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  return (
    <div className="max-w-[1600px] mx-auto px-2 py-3">
      <div className="flex items-center gap-2 mb-3">
        <h1 className="text-sm font-bold text-[#e8e8e8]">Analytics</h1>
        <form onSubmit={handleSearch} className="flex items-center gap-2">
          <input type="text" placeholder="Steam ID..." value={sid} onChange={e => setSid(e.target.value)} className="input w-40 text-[11px]" />
          <button type="submit" className="btn btn-primary text-[11px]">Search</button>
        </form>
      </div>

      {loading && <div className="text-xs text-[#555] p-4">Loading...</div>}
      {error && !loading && <div className="panel rounded p-3"><div className="text-[11px] text-[#f87171]">{error}</div></div>}

      {data && !loading && (
        <div className="grid grid-cols-[200px_1fr_200px] gap-3">
          <div className="space-y-2">
            <div className="panel rounded p-2"><RadarChart data={[
              { category: 'SKT', value: data.ratings?.skating || 50, fullMark: 100 },
              { category: 'SHT', value: data.ratings?.shooting || 50, fullMark: 100 },
              { category: 'STK', value: data.ratings?.stickhandling || 50, fullMark: 100 },
              { category: 'PAS', value: data.ratings?.passing || 50, fullMark: 100 },
              { category: 'INP', value: data.ratings?.inputs || 50, fullMark: 100 },
              { category: 'GSN', value: data.ratings?.gameSense || 50, fullMark: 100 },
            ]} size={150} /></div>
            <div className="panel rounded">
              <div className="panel-header">Ratings</div>
              {['Skating', 'Shooting', 'Stickhandling', 'Passing', 'Inputs', 'GameSense'].map(k => (
                <div key={k} className="stat-row">
                  <span className="stat-label">{k}</span>
                  <span className="stat-value">{data.ratings?.[k.charAt(0).toLowerCase() + k.slice(1)] ?? data.ratings?.[k] ?? 50}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="space-y-2">
            <div className="panel rounded p-2">
              <div className="text-[10px] text-[#555] mb-1 uppercase tracking-wider">Performance</div>
              <div className="grid grid-cols-2 gap-3">
                {data.skatingHeatmap && <Heatmap data={data.skatingHeatmap} width={180} height={90} />}
                {data.shotHeatmap && <Heatmap data={data.shotHeatmap} width={180} height={90} />}
              </div>
            </div>
            {data.ratingHistory && <RatingTrendChart trends={data.ratingHistory} height={100} />}
          </div>

          <div className="space-y-2">
            <div className="panel rounded">
              <div className="panel-header">Scouting</div>
              <div className="p-2">
                <div className="text-[11px] text-[#8a8a8a] mb-1">{data.scoutingReport?.summary || 'No data'}</div>
                {data.scoutingReport?.strengths?.slice(0, 3).map((s: string, i: number) => (
                  <div key={i} className="text-[10px] text-[#4ade80] leading-relaxed">+ {s}</div>
                ))}
                {data.scoutingReport?.improvementSuggestions?.slice(0, 2).map((s: string, i: number) => (
                  <div key={i} className="text-[10px] text-[#fbbf24] leading-relaxed">• {s}</div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      {!data && !loading && !error && (
        <div className="panel rounded p-4 text-center">
          <div className="text-xs text-[#555]">Enter a Steam ID to view player analytics</div>
        </div>
      )}
    </div>
  );
}
