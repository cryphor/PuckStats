interface Props {
  rating: number;
  label: string;
  size?: 'sm' | 'md' | 'lg';
  showPercentile?: boolean;
  percentile?: number;
}

export function RatingBadge({ rating, label, size = 'md', showPercentile, percentile }: Props) {
  const sizes = { sm: 'text-xs w-12 h-12', md: 'text-sm w-16 h-16', lg: 'text-lg w-20 h-20' };
  const fontSizes = { sm: 'text-sm', md: 'text-lg', lg: 'text-2xl' };

  const getColor = (val: number) => {
    if (val >= 90) return 'border-emerald-400 text-emerald-400';
    if (val >= 75) return 'border-green-400 text-green-400';
    if (val >= 60) return 'border-yellow-400 text-yellow-400';
    if (val >= 40) return 'border-orange-400 text-orange-400';
    return 'border-red-400 text-red-400';
  };

  return (
    <div className="flex flex-col items-center gap-1 group">
      <div
        className={`${sizes[size]} rounded-full border-2 flex items-center justify-center
                    ${getColor(rating)} bg-panel transition-all duration-300
                    group-hover:border-glow`}
      >
        <span className={`${fontSizes[size]} font-bold tabular-nums`}>{rating}</span>
      </div>
      <span className="text-muted text-xs text-center">{label}</span>
      {showPercentile && percentile !== undefined && (
        <span className="text-muted-dim text-[10px]">Top {100 - percentile}%</span>
      )}
    </div>
  );
}
