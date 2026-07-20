'use client';

import { useParams } from 'next/navigation';
import { useState, useEffect } from 'react';
import { api } from '@/lib/api';

export default function ReplayViewerPage() {
  const params = useParams();
  const replayId = Number(params.id);
  const [replay, setReplay] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!replayId) return;
    api.getReplay(replayId).then(setReplay).catch(() => {}).finally(() => setLoading(false));
  }, [replayId]);

  if (loading) return <div className="text-xs text-[#555] p-4">Loading replay...</div>;
  if (!replay) return <div className="text-xs text-[#8a8a8a] p-4">Replay not found</div>;

  const h = replay.header || {};
  const events = replay.events || [];

  return (
    <div className="max-w-[1600px] mx-auto px-2 py-3">
      <div className="flex items-center gap-3 mb-3 pb-3 border-b border-[#1d1d1d]">
        <h1 className="text-sm font-bold text-[#e8e8e8]">Replay</h1>
        <span className="text-[11px] text-[#555]">{h.serverName || 'Unknown'} · {h.blueScore}-{h.redScore}</span>
        <span className="text-[11px] text-[#555]">{h.durationSeconds ? `${Math.floor(h.durationSeconds / 60)}m ${Math.floor(h.durationSeconds % 60)}s` : ''}</span>
      </div>

      <div className="grid grid-cols-[1fr_280px] gap-3">
        {/* Rink area (placeholder) */}
        <div className="panel rounded aspect-video flex items-center justify-center">
          <div className="text-xs text-[#555]">Replay viewer — rink rendering area</div>
        </div>

        {/* Side panel */}
        <div className="space-y-2">
          <div className="panel rounded">
            <div className="panel-header">Events ({events.length})</div>
            <div className="max-h-80 overflow-y-auto">
              {events.map((e: any, i: number) => (
                <div key={i} className="stat-row">
                  <span className="text-[10px] font-mono text-[#555] w-10">{Math.floor(e.timestamp / 60)}:{String(Math.floor(e.timestamp % 60)).padStart(2, '0')}</span>
                  <span className={`text-[10px] ${e.type === 'Goal' ? 'text-[#4ade80]' : e.type === 'Save' ? 'text-[#60a5fa]' : 'text-[#8a8a8a]'}`}>{e.type}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
