import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function GET(req: NextRequest) {
  const a = req.nextUrl.searchParams.get('playerA') || '';
  const b = req.nextUrl.searchParams.get('playerB') || '';
  try {
    await initTables();
    const [aRes, bRes] = await Promise.all([
      query('SELECT * FROM "PlayerRatings" WHERE "SteamId" = $1', [a]),
      query('SELECT * FROM "PlayerRatings" WHERE "SteamId" = $1', [b]),
    ]);
    const va = aRes.rows[0] as any || {};
    const vb = bRes.rows[0] as any || {};
    const s = (r: any) => r.overall ?? r.Overall ?? 50;
    const make = (r: any, id: string) => ({
      steamId: id, username: id.substring(0, 12),
      ratings: { skating: r.skating ?? r.Skating ?? 50, shooting: r.shooting ?? r.Shooting ?? 50, stickhandling: r.stickhandling ?? r.Stickhandling ?? 50, passing: r.passing ?? r.Passing ?? 50, inputs: r.inputs ?? r.Inputs ?? 50, gameSense: r.gamesense ?? r.GameSense ?? 50, overall: s(r) },
    });
    return NextResponse.json({
      playerA: make(va, a), playerB: make(vb, b),
      differences: {
        skating: (va.skating ?? va.Skating ?? 50) - (vb.skating ?? vb.Skating ?? 50),
        shooting: (va.shooting ?? va.Shooting ?? 50) - (vb.shooting ?? vb.Shooting ?? 50),
        stickhandling: (va.stickhandling ?? va.Stickhandling ?? 50) - (vb.stickhandling ?? vb.Stickhandling ?? 50),
        passing: (va.passing ?? va.Passing ?? 50) - (vb.passing ?? vb.Passing ?? 50),
        inputs: (va.inputs ?? va.Inputs ?? 50) - (vb.inputs ?? vb.Inputs ?? 50),
        gameSense: (va.gamesense ?? va.GameSense ?? 50) - (vb.gamesense ?? vb.GameSense ?? 50),
        overall: s(va) - s(vb),
      }
    });
  } catch (e: any) {
    console.error('Compare error:', e?.message);
    return NextResponse.json({ error: 'Compare error' }, { status: 500 });
  }
}
