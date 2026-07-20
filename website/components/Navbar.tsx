'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useState } from 'react';

const links = [
  { href: '/', label: 'Home' },
  { href: '/leaderboard', label: 'Leaderboards' },
  { href: '/analytics', label: 'Analytics' },
  { href: '/compare', label: 'Compare' },
  { href: '/replay', label: 'Replays' },
];

export function Navbar() {
  const pathname = usePathname();
  const [q, setQ] = useState('');

  const isActive = (href: string) =>
    href === '/' ? pathname === '/' : pathname.startsWith(href);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (q.trim()) window.location.href = `/player/${q.trim()}`;
  };

  return (
    <div className="border-b border-[#1a1a1a] bg-[#050505] sticky top-0 z-50">
      <div className="flex items-center h-9 px-3 gap-1 max-w-[1400px] mx-auto">
        <Link href="/" className="flex items-center gap-1.5 flex-shrink-0 mr-2">
          <div className="w-[18px] h-[18px] rounded bg-[#4a7cff] flex items-center justify-center">
            <span className="text-[9px] font-bold text-white">P</span>
          </div>
          <span className="text-sm font-semibold text-[#e8e8e8]">PuckStats</span>
        </Link>

        <div className="flex items-center gap-px">
          {links.map(link => (
            <Link
              key={link.href}
              href={link.href}
              className={`nav-a ${isActive(link.href) ? 'active' : ''}`}
            >
              {link.label}
            </Link>
          ))}
        </div>

        <div className="flex-1" />

        <form onSubmit={handleSearch} className="flex">
          <input
            type="text"
            placeholder="Steam ID..."
            value={q}
            onChange={e => setQ(e.target.value)}
            className="input w-32 text-[11px]"
          />
        </form>
      </div>
    </div>
  );
}
