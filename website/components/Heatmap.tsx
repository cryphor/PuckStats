'use client';

import { useRef, useEffect } from 'react';
import type { HeatmapData } from '@/lib/api';

interface Props {
  data: HeatmapData;
  width?: number;
  height?: number;
}

export function Heatmap({ data, width = 600, height = 300 }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const cellW = width / data.rinkWidth;
    const cellH = height / data.rinkHeight;

    ctx.clearRect(0, 0, width, height);

    // Draw rink background
    ctx.fillStyle = '#0d0d0d';
    ctx.fillRect(0, 0, width, height);

    // Rink borders
    ctx.strokeStyle = '#1a1a1a';
    ctx.lineWidth = 2;
    ctx.strokeRect(0, 0, width, height);

    // Center line
    ctx.beginPath();
    ctx.moveTo(width / 2, 0);
    ctx.lineTo(width / 2, height);
    ctx.strokeStyle = '#1a1a1a';
    ctx.lineWidth = 1;
    ctx.stroke();

    // Center circle
    ctx.beginPath();
    ctx.arc(width / 2, height / 2, 50, 0, Math.PI * 2);
    ctx.stroke();

    // Draw heatmap cells
    for (const cell of data.cells) {
      const alpha = Math.min(1, cell.intensity);
      const r = Math.round(66 * alpha);
      const g = Math.round(255 * alpha);
      const b = Math.round(143 * alpha);
      ctx.fillStyle = `rgba(${r},${g},${b},${alpha * 0.7})`;
      ctx.fillRect(cell.x * cellW, cell.y * cellH, cellW, cellH);
    }
  }, [data, width, height]);

  return (
    <div className="glass-panel p-4">
      <h3 className="text-sm font-semibold text-text mb-3 capitalize">{data.type} Heatmap</h3>
      <canvas
        ref={canvasRef}
        width={width}
        height={height}
        className="w-full rounded-lg"
        style={{ maxWidth: width }}
      />
      <div className="flex items-center gap-3 mt-2">
        <span className="text-xs text-muted">Low</span>
        <div className="flex-1 h-2 rounded-full bg-gradient-to-r from-[#0a0a0a] via-[#42ff8f]/30 to-[#42ff8f]" />
        <span className="text-xs text-muted">High</span>
      </div>
    </div>
  );
}
