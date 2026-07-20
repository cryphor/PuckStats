import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function GET(req: NextRequest, { params }: { params: { steamId: string } }) {
  try {
    await initTables();
    const { steamId } = params;
    const result = await query('SELECT * FROM "PlayerRatings" WHERE "SteamId" = $1', [steamId]);
    const r = result.rows[0] || {};
    return NextResponse.json({
      steamId,
      ratings: {
        skating: r.skating ?? r.Skating ?? 50,
        shooting: r.shooting ?? r.Shooting ?? 50,
        stickhandling: r.stickhandling ?? r.Stickhandling ?? 50,
        passing: r.passing ?? r.Passing ?? 50,
        inputs: r.inputs ?? r.Inputs ?? 50,
        stickMotion: r.stickmotion ?? r.StickMotion ?? 50,
        offensivePlay: r.offensiveplay ?? r.OffensivePlay ?? 50,
        defensivePlay: r.defensiveplay ?? r.DefensivePlay ?? 50,
        positioning: r.positioning ?? r.Positioning ?? 50,
        gameSense: r.gamesense ?? r.GameSense ?? 50,
        overall: r.overall ?? r.Overall ?? 50,
      },
      scoutingReport: { strengths: [], weaknesses: [], overallRating: r.overall ?? r.Overall ?? 50 },
      skatingHeatmap: { type: 'Skating', rinkWidth: 100, rinkHeight: 50, cells: [], maxIntensity: 0 },
      shotHeatmap: { type: 'Shot', rinkWidth: 100, rinkHeight: 50, cells: [], maxIntensity: 0 },
      possessionHeatmap: { type: 'Possession', rinkWidth: 100, rinkHeight: 50, cells: [], maxIntensity: 0 },
    });
  } catch (e: any) {
    console.error('Analytics error:', e?.message);
    return NextResponse.json({ error: e?.message || 'Analytics error' }, { status: 500 });
  }
}
