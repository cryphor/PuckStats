'use client';

import { useState } from 'react';
import { api, type CompareResult } from '@/lib/api';
import { RadarChart } from '@/components/RadarChart';

export default function ComparePage() {
  const [a, setA] = useState('');
  const [b, setB] = useState('');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CompareResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleCompare = (e: React.FormEvent) => {
    e.preventDefault();
    if (!a.trim() || !b.trim()) return;
    setLoading(true); setError(null);
    api.comparePlayers(a.trim(), b.trim()).then(setResult).catch(e => setError(e.message)).finally(() => setLoading(false));
  };

  const makeRadar = (r: any) => [
    { category: 'SKT', value: r?.skating || 50, fullMark: 100 },
    { category: 'SHT', value: r?.shooting || 50, fullMark: 100 },
    { category: 'STK', value: r?.stickhandling || 50, fullMark: 100 },
    { category: 'PAS', value: r?.passing || 50, fullMark: 100 },
    { category: 'INP', value: r?.inputs || 50, fullMark: 100 },
    { category: 'GSN', value: r?.gameSense || 50, fullMark: 100 },
  ];

  return (
    <div className="max-w-[1600px] mx-auto px-2 py-3">
      <div className="flex items-center gap-2 mb-3">
        <h1 className="text-sm font-bold text-[#e8e8e8]">Compare</h1>
        <form onSubmit={handleCompare} className="flex items-center gap-2">
          <input type="text" placeholder="Player A" value={a} onChange={e => setA(e.target.value)} className="input w-28 text-[11px]" />
          <span className="text-[#555] text-xs">vs</span>
          <input type="text" placeholder="Player B" value={b} onChange={e => setB(e.target.value)} className="input w-28 text-[11px]" />
          <button type="submit" className="btn btn-primary text-[11px]">Compare</button>
        </form>
      </div>

      {loading && <div className="text-xs text-[#555] p-4">Loading...</div>}
      {error && <div className="panel rounded p-3 text-[11px] text-[#f87171]">{error}</div>}

      {result && !loading && (
        <div className="grid grid-cols-2 gap-3">
          {[result.playerA, result.playerB].map((pl, idx) => (
            <div key={idx} className="panel rounded">
              <div className="panel-header">{pl?.username || (idx === 0 ? 'Player A' : 'Player B')}</div>
              <div className="grid grid-cols-[1fr_140px] gap-2 p-2">
                <div className="space-y-1">
                  {['skating', 'shooting', 'stickhandling', 'passing', 'inputs', 'gameSense', 'overall'].map(k => (
                    <div key={k} className="flex items-center gap-2">
                      <span className="text-[10px] text-[#8a8a8a] w-16 capitalize">{k.replace(/([A-Z])/g, ' $1')}</span>
                      <div className="flex-1 progress-bar">
                        <div className="progress-fill bg-[#4ade80]/40" style={{ width: `${pl?.ratings?.[k as keyof typeof pl.ratings] || 50}%` }} />
                      </div>
                      <span className="text-[10px] font-mono text-[#e8e8e8] w-6 text-right">{pl?.ratings?.[k as keyof typeof pl.ratings] || 50}</span>
                    </div>
                  ))}
                </div>
                <div className="flex justify-center"><RadarChart data={makeRadar(pl?.ratings)} size={130} /></div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
