import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function GET(req: NextRequest, { params }: { params: { steamId: string } }) {
  try {
    await initTables();
    const { steamId } = params;
    const result = await query(
      'SELECT "OverallRating", "RecordedAt" FROM "RatingHistory" WHERE "SteamId" = $1 ORDER BY "RecordedAt" DESC LIMIT 90',
      [steamId]
    );
    const points = result.rows.map((r: any) => ({
      date: r.RecordedAt?.toISOString?.() || new Date().toISOString(),
      rating: r.OverallRating || 50,
    })).reverse();
    return NextResponse.json([{ category: 'Overall', points }]);
  } catch { return NextResponse.json([]); }
}
