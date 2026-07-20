'use client';
import { RadarChart as RC, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar, ResponsiveContainer } from 'recharts';

interface Props {
  data: { category: string; value: number; fullMark: number }[];
  size?: number;
}

export function RadarChart({ data, size = 160 }: Props) {
  return (
    <ResponsiveContainer width={size} height={size}>
      <RC data={data} cx="50%" cy="50%" outerRadius="70%">
        <PolarGrid stroke="#1d1d1d" strokeWidth={0.5} />
        <PolarAngleAxis dataKey="category" tick={{ fill: '#8a8a8a', fontSize: 9 }} tickLine={false} />
        <PolarRadiusAxis angle={90} domain={[0, 100]} tick={false} axisLine={false} />
        <Radar name="Rating" dataKey="value" stroke="#4ade80" strokeWidth={1.5} fill="#4ade80" fillOpacity={0.1} />
      </RC>
    </ResponsiveContainer>
  );
}
