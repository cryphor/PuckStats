'use client';
import { useRef, useEffect } from 'react';

interface Props { data: { cells: { x: number; y: number; intensity: number }[]; maxIntensity: number; type: string }; width?: number; height?: number; }

export function Heatmap({ data, width = 200, height = 100 }: Props) {
  const ref = useRef<HTMLCanvasElement>(null);
  useEffect(() => {
    const c = ref.current; if (!c) return;
    const ctx = c.getContext('2d'); if (!ctx) return;
    ctx.clearRect(0, 0, width, height);
    ctx.fillStyle = '#050505'; ctx.fillRect(0, 0, width, height);
    ctx.strokeStyle = '#1d1d1d'; ctx.lineWidth = 1; ctx.strokeRect(0, 0, width, height);
    ctx.beginPath(); ctx.moveTo(width / 2, 0); ctx.lineTo(width / 2, height); ctx.strokeStyle = '#1d1d1d'; ctx.stroke();
    ctx.beginPath(); ctx.arc(width / 2, height / 2, 25, 0, Math.PI * 2); ctx.stroke();

    const cellW = width / 100, cellH = height / 50;
    for (const cell of data.cells) {
      const a = Math.min(1, cell.intensity);
      ctx.fillStyle = `rgba(74,222,128,${a * 0.5})`;
      ctx.fillRect(cell.x * cellW, cell.y * cellH, cellW + 1, cellH + 1);
    }
  }, [data, width, height]);
  return (
    <div className="panel rounded">
      <div className="panel-header capitalize">{data.type} Heatmap</div>
      <canvas ref={ref} width={width} height={height} className="w-full" style={{ maxWidth: width }} />
    </div>
  );
}
