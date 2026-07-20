import Link from 'next/link';

export default function HomePage() {
  return (
    <div className="max-w-[1600px] mx-auto px-2 py-3">
      <div className="grid grid-cols-[240px_1fr] gap-3">
        <div className="space-y-2">
          <div className="panel rounded">
            <div className="panel-header">Quick Links</div>
            <div className="p-2 space-y-1">
              <Link href="/leaderboard" className="block text-xs text-[#8a8a8a] hover:text-[#e8e8e8] py-1">Global Leaderboard</Link>
              <Link href="/compare" className="block text-xs text-[#8a8a8a] hover:text-[#e8e8e8] py-1">Compare Players</Link>
              <Link href="/analytics" className="block text-xs text-[#8a8a8a] hover:text-[#e8e8e8] py-1">Analytics Search</Link>
              <Link href="/replay" className="block text-xs text-[#8a8a8a] hover:text-[#e8e8e8] py-1">Replay Upload</Link>
            </div>
          </div>
        </div>

        <div className="panel rounded p-4">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-7 h-7 rounded bg-[#4ade80] flex items-center justify-center"><span className="text-xs font-bold text-black">P</span></div>
            <span className="text-base font-bold text-[#e8e8e8]">PuckStats</span>
          </div>
          <div className="text-[12px] text-[#555] leading-relaxed">
            Search for a player by Steam ID to view their analytics, or browse the leaderboard.
          </div>
          <div className="flex items-center gap-2 mt-2">
            <Link href="/leaderboard" className="btn btn-primary">Leaderboard</Link>
            <Link href="/replay" className="btn btn-secondary">Upload Replay</Link>
          </div>
        </div>
      </div>
    </div>
  );
}
