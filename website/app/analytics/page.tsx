'use client';

import { useState, useEffect } from 'react';
import { api, type HeatmapData, type PassingNetwork } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';
import { Heatmap } from '@/components/Heatmap';
import { RatingTrendChart } from '@/components/RatingTrendChart';
import { BarChart3, Loader2, Search } from 'lucide-react';

export default function AnalyticsPage() {
  const [steamId, setSteamId] = useState('');
  const [searched, setSearched] = useState('');
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (!steamId.trim()) return;
    setLoading(true);
    setError(null);
    setSearched(steamId.trim());

    Promise.all([
      api.getPlayerAnalytics(steamId.trim()),
      api.getRatingHistory(steamId.trim(), 90),
    ])
      .then(([analytics, history]) => {
        setData({ ...analytics, ratingHistory: history });
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-text mb-6 flex items-center gap-2">
        <BarChart3 size={24} className="text-accent" />
        Analytics
      </h1>

      {/* Search */}
      <form onSubmit={handleSearch} className="mb-8">
        <div className="flex gap-3 max-w-md">
          <div className="relative flex-1">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-dim" />
            <input
              type="text"
              placeholder="Enter Steam ID..."
              value={steamId}
              onChange={(e) => setSteamId(e.target.value)}
              className="input pl-9 w-full"
            />
          </div>
          <button type="submit" className="btn btn-primary">Analyze</button>
        </div>
      </form>

      {loading && (
        <div className="flex items-center justify-center py-20">
          <Loader2 size={32} className="text-accent animate-spin" />
        </div>
      )}

      {error && !loading && (
        <div className="glass-panel p-8 text-center">
          <p className="text-danger text-sm">{error}</p>
          <p className="text-muted text-xs mt-1">Try searching for a valid Steam ID</p>
        </div>
      )}

      {data && !loading && (
        <div className="space-y-6 animate-fade-in">
          {/* Radar + Ratings */}
          {data.ratings && (
            <div className="grid md:grid-cols-2 gap-6">
              <div className="glass-panel p-4 flex justify-center">
                <RadarChart data={[
                  { category: 'Skating', value: data.ratings.skating, fullMark: 100 },
                  { category: 'Shooting', value: data.ratings.shooting, fullMark: 100 },
                  { category: 'Stickhandling', value: data.ratings.stickhandling, fullMark: 100 },
                  { category: 'Passing', value: data.ratings.passing, fullMark: 100 },
                  { category: 'Inputs', value: data.ratings.inputs, fullMark: 100 },
                  { category: 'G.Sense', value: data.ratings.gameSense, fullMark: 100 },
                ]} size={260} />
              </div>
              <div className="glass-panel p-4">
                <h3 className="text-sm font-semibold text-text mb-3">Performance Summary</h3>
                <div className="space-y-1.5">
                  {Object.entries(data.ratings)
                    .filter(([k]) => typeof k === 'string' && !['archetype', 'stickMotion'].includes(k as string))
                    .map(([key, value]) => (
                      <div key={key} className="flex items-center gap-3">
                        <span className="text-xs text-muted w-24">{key.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase())}</span>
                        <div className="flex-1 progress-bar">
                          <div className="fill bg-accent" style={{ width: `${value}%` }} />
                        </div>
                        <span className="text-xs font-mono w-8 text-right">{value as number}</span>
                      </div>
                    ))}
                </div>
              </div>
            </div>
          )}

          {/* Rating Trends */}
          {data.ratingHistory && (
            <RatingTrendChart trends={data.ratingHistory} height={250} />
          )}

          {/* Heatmaps */}
          <div className="grid lg:grid-cols-3 gap-4">
            {data.skatingHeatmap && <Heatmap data={data.skatingHeatmap} width={400} height={200} />}
            {data.shotHeatmap && <Heatmap data={data.shotHeatmap} width={400} height={200} />}
            {data.possessionHeatmap && <Heatmap data={data.possessionHeatmap} width={400} height={200} />}
          </div>

          {/* Passing Network */}
          {data.passingNetwork?.nodes?.length > 0 && (
            <div className="glass-panel p-4">
              <h3 className="text-sm font-semibold text-text mb-3">Passing Network</h3>
              <div className="flex flex-wrap gap-3">
                {data.passingNetwork.nodes.map((node: any) => (
                  <div key={node.steamId} className="flex items-center gap-2 px-3 py-2 rounded-lg bg-[#0a0a0a] border border-border">
                    <div className="w-2 h-2 rounded-full bg-accent" style={{ opacity: node.influence / 10 }} />
                    <span className="text-xs text-text">{node.username || node.steamId.substring(0, 8)}</span>
                    <span className="text-[10px] text-muted">{node.influence.toFixed(1)}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Scouting Report */}
          {data.scoutingReport && (
            <div className="glass-panel p-5">
              <h3 className="text-sm font-semibold text-text mb-3">Scouting Report</h3>
              <p className="text-sm text-muted mb-3">{data.scoutingReport.summary}</p>
              <div className="grid md:grid-cols-2 gap-4">
                <div>
                  <h4 className="text-xs font-semibold text-accent mb-2">Strengths</h4>
                  {data.scoutingReport.strengths?.map((s: string, i: number) => (
                    <p key={i} className="text-xs text-text mb-1">+ {s}</p>
                  ))}
                </div>
                <div>
                  <h4 className="text-xs font-semibold text-warning mb-2">Improvements</h4>
                  {data.scoutingReport.weaknesses?.map((w: string, i: number) => (
                    <p key={i} className="text-xs text-muted mb-1">- {w}</p>
                  ))}
                  {data.scoutingReport.improvementSuggestions?.map((s: string, i: number) => (
                    <p key={i} className="text-xs text-blue mb-1">• {s}</p>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {!data && !loading && !error && (
        <div className="glass-panel p-12 text-center">
          <BarChart3 size={48} className="text-muted-dim mx-auto mb-4" />
          <p className="text-muted text-sm">Enter a Steam ID to view player analytics</p>
        </div>
      )}
    </div>
  );
}
