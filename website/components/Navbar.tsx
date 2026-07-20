'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useState } from 'react';

export function Navbar() {
  const pathname = usePathname();
  const [q, setQ] = useState('');

  const links = [
    { href: '/', label: 'Home' },
    { href: '/leaderboard', label: 'Leaderboard' },
    { href: '/analytics', label: 'Analytics' },
    { href: '/compare', label: 'Compare' },
    { href: '/replay', label: 'Replays' },
  ];

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (q.trim()) window.location.href = `/player/${q.trim()}`;
  };

  return (
    <div className="border-b border-[#1d1d1d] bg-[#050505] sticky top-0 z-50">
      <div className="flex items-center h-9 px-4 gap-4 max-w-[1600px] mx-auto">
        <Link href="/" className="flex items-center gap-1.5 flex-shrink-0">
          <div className="w-5 h-5 rounded bg-[#4ade80] flex items-center justify-center">
            <span className="text-[10px] font-bold text-black">P</span>
          </div>
          <span className="text-sm font-bold text-[#e8e8e8]">PuckStats</span>
        </Link>

        <div className="flex items-center gap-0.5">
          {links.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={`nav-item ${link.href === '/' ? pathname === '/' ? 'active' : '' : pathname.startsWith(link.href) ? 'active' : ''}`}
            >
              {link.label}
            </Link>
          ))}
        </div>

        <div className="flex-1" />

        <form onSubmit={handleSearch} className="flex items-center">
          <input
            type="text"
            placeholder="Steam ID..."
            value={q}
            onChange={(e) => setQ(e.target.value)}
            className="input w-36 text-[11px]"
          />
        </form>
      </div>
    </div>
  );
}
