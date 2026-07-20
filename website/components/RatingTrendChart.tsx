'use client';

import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { RatingTrend } from '@/lib/api';
import { format, parseISO } from 'date-fns';

interface Props {
  trends: RatingTrend[];
  height?: number;
}

export function RatingTrendChart({ trends, height = 300 }: Props) {
  const colors: Record<string, string> = {
    Skating: '#42ff8f',
    Shooting: '#f472b6',
    Stickhandling: '#60a5fa',
    Passing: '#fbbf24',
    GameSense: '#a78bfa',
    Overall: '#ffffff',
  };

  // Merge all trend points into unified timeline
  const dateMap = new Map<string, any>();
  for (const trend of trends) {
    for (const point of trend.points) {
      const date = typeof point.date === 'string' ? point.date.split('T')[0] : point.date;
      if (!dateMap.has(date)) dateMap.set(date, { date });
      dateMap.get(date)![trend.category] = point.rating;
    }
  }

  const mergedData = Array.from(dateMap.values())
    .sort((a, b) => a.date.localeCompare(b.date));

  const displayTrends = ['Overall', 'Skating', 'Shooting', 'GameSense'];

  return (
    <div className="glass-panel p-4">
      <h3 className="text-sm font-semibold text-text mb-3">Rating Progression</h3>
      <ResponsiveContainer width="100%" height={height}>
        <LineChart data={mergedData}>
          <CartesianGrid stroke="#1a1a1a" strokeDasharray="3 3" />
          <XAxis
            dataKey="date"
            tick={{ fill: '#666', fontSize: 10 }}
            tickFormatter={(d) => format(parseISO(d), 'MMM d')}
            tickLine={false}
            axisLine={false}
          />
          <YAxis
            domain={[0, 100]}
            tick={{ fill: '#666', fontSize: 10 }}
            tickLine={false}
            axisLine={false}
          />
          <Tooltip
            contentStyle={{
              background: '#111',
              border: '1px solid #222',
              borderRadius: '8px',
              fontSize: '12px',
              color: '#fff',
            }}
          />
          {displayTrends.map((cat) => (
            <Line
              key={cat}
              type="monotone"
              dataKey={cat}
              stroke={colors[cat] || '#888'}
              strokeWidth={cat === 'Overall' ? 2.5 : 1.5}
              dot={false}
              connectNulls
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
      <div className="flex items-center gap-4 mt-2 flex-wrap">
        {displayTrends.map((cat) => (
          <div key={cat} className="flex items-center gap-1.5">
            <div className="w-2.5 h-2.5 rounded-full" style={{ background: colors[cat] }} />
            <span className="text-xs text-muted">{cat}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
