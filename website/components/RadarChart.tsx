'use client';

import { RadarChart as ReRadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar, ResponsiveContainer } from 'recharts';

interface Props {
  data: { category: string; value: number; fullMark: number }[];
  size?: number;
}

export function RadarChart({ data, size = 280 }: Props) {
  return (
    <ResponsiveContainer width={size} height={size}>
      <ReRadarChart data={data} cx="50%" cy="50%" outerRadius="75%">
        <PolarGrid stroke="#222" strokeWidth={0.5} />
        <PolarAngleAxis
          dataKey="category"
          tick={{ fill: '#a0a0a0', fontSize: 10, fontWeight: 500 }}
          tickLine={false}
        />
        <PolarRadiusAxis
          angle={90}
          domain={[0, 100]}
          tick={false}
          axisLine={false}
        />
        <Radar
          name="Rating"
          dataKey="value"
          stroke="#42ff8f"
          strokeWidth={2}
          fill="#42ff8f"
          fillOpacity={0.15}
        />
      </ReRadarChart>
    </ResponsiveContainer>
  );
}
