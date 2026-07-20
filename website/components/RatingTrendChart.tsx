'use client';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface Props { trends: { category: string; points: { date: string; rating: number }[] }[]; height?: number; }

export function RatingTrendChart({ trends, height = 120 }: Props) {
  const colors: Record<string, string> = { Skating: '#4ade80', Shooting: '#f472b6', Stickhandling: '#60a5fa', Passing: '#fbbf24', GameSense: '#a78bfa', Overall: '#e8e8e8' };
  const merged: any[] = [];
  for (const t of trends) for (const p of t.points) {
    let e = merged.find((m: any) => m.date === p.date.split('T')[0]);
    if (!e) { e = { date: p.date.split('T')[0] }; merged.push(e); }
    e[t.category] = p.rating;
  }
  merged.sort((a, b) => a.date.localeCompare(b.date));

  return (
    <div className="panel rounded p-2">
      <div className="text-[10px] text-[#555] mb-1 uppercase tracking-wider">Rating History</div>
      <ResponsiveContainer width="100%" height={height}>
        <LineChart data={merged.slice(-30)}>
          <CartesianGrid stroke="#1d1d1d" strokeDasharray="2 2" />
          <XAxis dataKey="date" tick={{ fill: '#555', fontSize: 9 }} tickFormatter={(d: string) => d.slice(5)} tickLine={false} axisLine={false} />
          <YAxis domain={[0, 100]} tick={{ fill: '#555', fontSize: 9 }} tickLine={false} axisLine={false} width={20} />
          <Tooltip contentStyle={{ background: '#0b0b0b', border: '1px solid #1d1d1d', borderRadius: 0, fontSize: '11px', color: '#e8e8e8' }} />
          {['Overall', 'Skating', 'Shooting'].map(cat => (
            <Line key={cat} type="monotone" dataKey={cat} stroke={colors[cat] || '#555'} strokeWidth={cat === 'Overall' ? 1.5 : 1} dot={false} connectNulls />
          ))}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
