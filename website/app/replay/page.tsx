'use client';

import { useState } from 'react';
import { api } from '@/lib/api';

export default function ReplayPage() {
  const [uploading, setUploading] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [sid, setSid] = useState('');

  const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]; if (!file) return;
    if (!file.name.endsWith('.puckreplay')) { setError('Only .puckreplay files'); return; }
    setUploading(true); setError(null);
    try { const r = await api.uploadReplay(file, sid || undefined); setResult(r); } catch (e: any) { setError(e.message); }
    setUploading(false);
  };

  return (
    <div className="max-w-[800px] mx-auto px-2 py-3">
      <h1 className="text-sm font-bold text-[#e8e8e8] mb-3">Replay Upload</h1>

      <div className="panel rounded p-3">
        <div className="text-[11px] text-[#555] mb-2">Upload a .puckreplay file for analysis.</div>
        {uploading ? (
          <div className="text-xs text-[#555]">Uploading...</div>
        ) : result ? (
          <div>
            <div className="text-xs text-[#4ade80] mb-2">Uploaded! Processing...</div>
            <button onClick={() => setResult(null)} className="btn btn-secondary text-[11px]">Upload Another</button>
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <input type="file" accept=".puckreplay" onChange={handleFile} className="text-xs text-[#555] file:mr-2 file:py-1 file:px-2 file:rounded file:border file:border-[#1d1d1d] file:bg-[#0b0b0b] file:text-xs file:text-[#e8e8e8]" />
            <input type="text" placeholder="Steam ID (optional)" value={sid} onChange={e => setSid(e.target.value)} className="input w-32 text-[11px]" />
          </div>
        )}
        {error && <div className="text-[11px] text-[#f87171] mt-2">{error}</div>}
      </div>
    </div>
  );
}
