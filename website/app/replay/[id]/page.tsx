'use client';

import { useParams } from 'next/navigation';
import { useState, useEffect } from 'react';
import { api } from '@/lib/api';
import { Heatmap } from '@/components/Heatmap';
import { Play, Pause, SkipBack, SkipForward, Loader2, Film, Goal, Zap, Clock } from 'lucide-react';

export default function ReplayViewerPage() {
  const params = useParams();
  const replayId = Number(params.id);
  const [replay, setReplay] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [playing, setPlaying] = useState(false);
  const [timestamp, setTimestamp] = useState(0);
  const [speed, setSpeed] = useState(1);
  const [activeTab, setActiveTab] = useState<'events' | 'heatmaps' | 'goals' | 'ai'>('events');

  useEffect(() => {
    if (!replayId) return;
    setLoading(true);
    api.getReplay(replayId)
      .then(setReplay)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [replayId]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[70vh]">
        <Loader2 size={32} className="text-accent animate-spin" />
      </div>
    );
  }

  if (!replay) {
    return (
      <div className="flex items-center justify-center min-h-[70vh]">
        <div className="glass-panel p-8 text-center">
          <Film size={48} className="text-muted-dim mx-auto mb-3" />
          <p className="text-muted text-sm">Replay not found or still processing</p>
        </div>
      </div>
    );
  }

  const header = replay.header;
  const players = replay.players || {};
  const events = replay.events || [];
  const duration = header?.durationSeconds || 0;

  return (
    <div className="max-w-7xl mx-auto px-4 py-6 animate-fade-in">
      {/* Header */}
      <div className="glass-panel p-4 mb-4">
        <div className="flex items-center justify-between flex-wrap gap-3">
          <div>
            <h1 className="text-lg font-bold text-text">
              {header?.serverName || 'Replay'} — {header?.map || 'Unknown Map'}
            </h1>
            <p className="text-xs text-muted">
              {new Date(header?.startTime).toLocaleDateString()} · {formatTime(duration)}
              {' · '}{header?.blueScore}-{header?.redScore}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted">{Object.keys(players).length} players</span>
            <span className="text-xs text-muted">·</span>
            <span className="text-xs text-muted">{events.length} events</span>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex items-center gap-0 border-b border-border mb-4">
        {(['events', 'heatmaps', 'goals', 'ai'] as const).map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`tab capitalize ${activeTab === tab ? 'active' : ''}`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      {activeTab === 'events' && (
        <div className="glass-panel p-4">
          <h3 className="text-sm font-semibold text-text mb-3">Match Events ({events.length})</h3>
          <div className="max-h-96 overflow-y-auto space-y-1">
            {events.map((event: any, i: number) => (
              <div key={i} className="flex items-center gap-3 py-1.5 border-b border-border/30 last:border-0">
                <Clock size={12} className="text-muted-dim flex-shrink-0" />
                <span className="text-xs font-mono text-muted w-12">{formatTime(event.timestamp)}</span>
                <span className={`px-2 py-0.5 rounded text-[10px] font-medium ${
                  event.type === 'Goal' ? 'bg-accent/10 text-accent' :
                  event.type === 'Save' ? 'bg-blue/10 text-blue' :
                  'bg-muted/10 text-muted'
                }`}>
                  {event.type}
                </span>
                <span className="text-xs text-muted truncate">{event.data?.substring(0, 60)}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'heatmaps' && (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
          {replay.skatingHeatmap && <Heatmap data={replay.skatingHeatmap} width={400} height={200} />}
          {replay.shotHeatmap && <Heatmap data={replay.shotHeatmap} width={400} height={200} />}
          {replay.possessionHeatmap && <Heatmap data={replay.possessionHeatmap} width={400} height={200} />}
        </div>
      )}

      {activeTab === 'goals' && (
        <div className="glass-panel p-4">
          <h3 className="text-sm font-semibold text-text mb-3 flex items-center gap-2">
            <Goal size={16} className="text-accent" />
            Goals
          </h3>
          {events.filter((e: any) => e.type === 'Goal').length === 0 ? (
            <p className="text-xs text-muted">No goals in this replay</p>
          ) : (
            <div className="space-y-3">
              {events.filter((e: any) => e.type === 'Goal').map((goal: any, i: number) => (
                <div key={i} className="flex items-center gap-3 p-3 rounded-lg bg-[#0a0a0a] border border-border">
                  <div className="w-8 h-8 rounded-full bg-accent/10 flex items-center justify-center">
                    <Goal size={14} className="text-accent" />
                  </div>
                  <div>
                    <p className="text-xs font-medium text-text">
                      {formatTime(goal.timestamp)} — Period {goal.data?.period || '?'}
                    </p>
                    <p className="text-[10px] text-muted">
                      Expected Goal Value: {(goal.data?.xG || 0.15).toFixed(2)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'ai' && (
        <div className="glass-panel p-4 text-center">
          <Zap size={40} className="text-accent mx-auto mb-3" />
          <p className="text-sm text-text font-medium mb-2">AI Analysis Available</p>
          <p className="text-xs text-muted mb-4">Click below to generate a detailed AI analysis of this match</p>
          <button
            className="btn btn-primary"
            onClick={() => api.analyzeReplay(replayId)}
          >
            Analyze Replay
          </button>
        </div>
      )}
    </div>
  );
}

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return `${m}:${s.toString().padStart(2, '0')}`;
}
