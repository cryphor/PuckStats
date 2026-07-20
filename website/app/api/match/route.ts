import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function POST(req: NextRequest) {
  try {
    await initTables();
    const body = await req.json();
    const { MatchId, SteamId, Username, Team, Goals, DistanceTraveled, AverageSpeed, TopSpeed, PuckTouches, PossessionTimeSeconds, MatchLengthSeconds, BlueScore, RedScore, ServerName } = body;

    if (!SteamId || !MatchId) {
      return NextResponse.json({ error: 'Missing SteamId or MatchId' }, { status: 400 });
    }

    const sid = SteamId;
    const uname = (Username || '').substring(0, 128);
    const team = Team || 'None';
    const goals = Math.max(0, Math.floor(Goals || 0));
    const dist = parseFloat(DistanceTraveled) || 0;
    const playTime = parseFloat(MatchLengthSeconds) || 0;

    // Upsert player using simple escaped values
    await query(`
      INSERT INTO "Players" ("SteamId", "Username", "Team", "TotalMatches", "TotalGoals", "TotalDistanceTraveled", "TotalPlayTimeSeconds", "LastUpdated")
      VALUES ('${sid}', '${uname.replace(/'/g, "''")}', '${team.replace(/'/g, "''")}', 1, ${goals}, ${dist}, ${playTime}, NOW())
      ON CONFLICT ("SteamId") DO UPDATE SET
        "Username" = CASE WHEN '${uname.replace(/'/g, "''")}' = '' THEN "Players"."Username" ELSE '${uname.replace(/'/g, "''")}' END,
        "Team" = '${team.replace(/'/g, "''")}',
        "TotalMatches" = "Players"."TotalMatches" + 1,
        "TotalGoals" = "Players"."TotalGoals" + ${goals},
        "TotalDistanceTraveled" = "Players"."TotalDistanceTraveled" + ${dist},
        "TotalPlayTimeSeconds" = "Players"."TotalPlayTimeSeconds" + ${playTime},
        "LastSeen" = NOW(),
        "LastUpdated" = NOW()
    `);

    return NextResponse.json({ ok: true, message: 'Match recorded' });
  } catch (e: any) {
    console.error('Match error:', e?.message);
    return NextResponse.json({ error: e?.message || 'Match error' }, { status: 500 });
  }
}
