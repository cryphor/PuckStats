interface Props { rating: number; label: string; size?: 'sm' | 'md'; }

export function RatingBadge({ rating, label, size = 'sm' }: Props) {
  const sz = size === 'sm' ? 'w-7 h-7 text-[10px]' : 'w-9 h-9 text-xs';
  const c = rating >= 75 ? 'border-[#4ade80]/50 text-[#4ade80]' : rating >= 50 ? 'border-[#fbbf24]/40 text-[#fbbf24]' : 'border-[#f87171]/40 text-[#f87171]';
  return (
    <div className="flex flex-col items-center gap-0.5">
      <div className={`${sz} rounded-full border flex items-center justify-center font-bold ${c}`}>{rating}</div>
      <span className="text-[9px] text-[#555]">{label}</span>
    </div>
  );
}
