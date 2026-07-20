'use client';

import { useState } from 'react';
import { api } from '@/lib/api';
import { Film, Upload, Loader2, Play, BarChart3 } from 'lucide-react';
import Link from 'next/link';

export default function ReplayPage() {
  const [uploading, setUploading] = useState(false);
  const [dragActive, setDragActive] = useState(false);
  const [uploadResult, setUploadResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [steamId, setSteamId] = useState('');

  const handleUpload = async (file: File) => {
    if (!file.name.endsWith('.puckreplay')) {
      setError('Only .puckreplay files are supported');
      return;
    }
    setUploading(true);
    setError(null);
    try {
      const result = await api.uploadReplay(file, steamId || undefined);
      setUploadResult(result);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setUploading(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragActive(false);
    const file = e.dataTransfer.files[0];
    if (file) handleUpload(file);
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleUpload(file);
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-8 animate-fade-in">
      <h1 className="text-2xl font-bold text-text mb-2 flex items-center gap-2">
        <Film size={24} className="text-accent" />
        Replay Center
      </h1>
      <p className="text-sm text-muted mb-8">Upload .puckreplay files for deep analysis — heatmaps, passing networks, AI coaching</p>

      {/* Upload zone */}
      <div
        onDragOver={(e) => { e.preventDefault(); setDragActive(true); }}
        onDragLeave={() => setDragActive(false)}
        onDrop={handleDrop}
        className={`glass-panel p-12 text-center border-2 border-dashed transition-all ${
          dragActive ? 'border-accent bg-accent/5' : 'border-border'
        }`}
      >
        {uploading ? (
          <div>
            <Loader2 size={40} className="text-accent animate-spin mx-auto mb-3" />
            <p className="text-sm text-muted">Uploading and processing replay...</p>
          </div>
        ) : uploadResult ? (
          <div>
            <div className="w-12 h-12 rounded-full bg-accent/10 mx-auto mb-3 flex items-center justify-center">
              <Play size={24} className="text-accent" />
            </div>
            <p className="text-sm text-text font-medium">Replay uploaded!</p>
            <p className="text-xs text-muted mt-1 mb-4">Processing for analysis...</p>
            <div className="flex items-center justify-center gap-3">
              <Link href={`/replay/${uploadResult.replayId}`} className="btn btn-primary">
                View Replay
              </Link>
              <button onClick={() => setUploadResult(null)} className="btn btn-secondary">
                Upload Another
              </button>
            </div>
          </div>
        ) : (
          <div>
            <div className="w-16 h-16 rounded-full bg-accent/10 mx-auto mb-4 flex items-center justify-center">
              <Upload size={28} className="text-accent" />
            </div>
            <p className="text-sm text-text font-medium mb-1">Drag & drop a .puckreplay file</p>
            <p className="text-xs text-muted mb-4">or click to browse</p>
            <label className="btn btn-primary cursor-pointer inline-flex items-center">
              <Upload size={14} className="mr-1.5" />
              Choose File
              <input type="file" accept=".puckreplay" className="hidden" onChange={handleFileInput} />
            </label>
            <div className="mt-4 max-w-xs mx-auto">
              <input
                type="text"
                placeholder="Your Steam ID (optional)"
                value={steamId}
                onChange={(e) => setSteamId(e.target.value)}
                className="input w-full text-xs"
              />
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className="glass-panel p-4 mt-4 border-danger/30">
          <p className="text-danger text-sm">{error}</p>
        </div>
      )}

      {/* Info */}
      <div className="glass-panel p-5 mt-6">
        <h3 className="text-sm font-semibold text-text mb-3 flex items-center gap-2">
          <BarChart3 size={16} className="text-accent" />
          What You'll Get
        </h3>
        <div className="grid md:grid-cols-2 gap-3">
          {[
            'Full replay parsing with frame-level detail',
            'Skating, shot, and possession heatmaps',
            'Team passing network visualization',
            'Shift-by-shift analytics',
            'Goal breakdowns with xG values',
            'AI-powered match analysis & suggestions',
          ].map((item, i) => (
            <div key={i} className="flex items-center gap-2">
              <div className="w-1.5 h-1.5 rounded-full bg-accent flex-shrink-0" />
              <span className="text-xs text-muted">{item}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
