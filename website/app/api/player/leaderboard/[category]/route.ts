import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function GET(req: NextRequest, { params }: { params: { category: string } }) {
  try {
    await initTables();
    const category = params.category || 'Overall';
    const col = `"${category}"`;
    const result = await query(
      `SELECT r."SteamId", r.${col} as rating, p."Username", p."TotalMatches", p."Archetype"
       FROM "PlayerRatings" r JOIN "Players" p ON r."SteamId" = p."SteamId"
       ORDER BY rating DESC NULLS LAST LIMIT 50`
    );
    const entries = result.rows.map((r: any, i: number) => ({
      rank: i + 1,
      steamId: r.steamid ?? r.SteamId ?? '',
      username: r.username ?? r.Username ?? r.steamid?.substring(0, 8) ?? 'Unknown',
      rating: r.rating ?? 50,
      matchesPlayed: r.totalmatches ?? r.TotalMatches ?? 0,
      archetype: r.archetype ?? r.Archetype ?? 'Unknown',
    }));
    return NextResponse.json(entries);
  } catch (e: any) {
    console.error('Leaderboard error:', e?.message);
    return NextResponse.json([]);
  }
}
