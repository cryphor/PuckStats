import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function POST(req: NextRequest) {
  try {
    await initTables();
    const body = await req.json();
    const { MatchId, SteamId, Ticks } = body;

    if (!SteamId || !MatchId) {
      return NextResponse.json({ error: 'Missing SteamId or MatchId' }, { status: 400 });
    }

    // Store telemetry summary (not individual ticks - too many)
    if (Ticks && Array.isArray(Ticks) && Ticks.length > 0) {
      const firstTick = Ticks[0];
      const lastTick = Ticks[Ticks.length - 1];

      // Compute basic stats from the batch
      let topSpeed = 0;
      let totalDist = 0;
      for (let i = 1; i < Ticks.length; i++) {
        const t = Ticks[i];
        const prev = Ticks[i - 1];
        if (t.Velocity) {
          const spd = Math.sqrt(t.Velocity.X * t.Velocity.X + t.Velocity.Y * t.Velocity.Y + t.Velocity.Z * t.Velocity.Z);
          if (spd > topSpeed) topSpeed = spd;
        }
        if (t.Position && prev.Position) {
          totalDist += Math.sqrt(
            Math.pow(t.Position.X - prev.Position.X, 2) +
            Math.pow(t.Position.Y - prev.Position.Y, 2) +
            Math.pow(t.Position.Z - prev.Position.Z, 2)
          );
        }
      }

      // Update player with telemetry-derived stats
      await query(`
        INSERT INTO "Players" ("SteamId", "TotalDistanceTraveled", "TotalPlayTimeSeconds", "LastUpdated")
        VALUES ($1, $2, $3, NOW())
        ON CONFLICT ("SteamId") DO UPDATE SET
          "TotalDistanceTraveled" = "Players"."TotalDistanceTraveled" + $2,
          "TotalPlayTimeSeconds" = "Players"."TotalPlayTimeSeconds" + $3,
          "LastSeen" = NOW(),
          "LastUpdated" = NOW()
      `, [SteamId, totalDist, Ticks.length * 0.05]); // 0.05s per tick at 20Hz

      // Update skating rating from telemetry
      const avgSpeed = totalDist / Math.max(1, (Ticks.length * 0.05));
      const skatingScore = Math.min(99, Math.max(1, Math.round(avgSpeed * 3 + topSpeed * 2)));

      await query(`
        INSERT INTO "PlayerRatings" ("SteamId", "Skating", "ComputedAt")
        VALUES ($1, $2, NOW())
        ON CONFLICT ("SteamId") DO UPDATE SET
          "Skating" = GREATEST("PlayerRatings"."Skating", $2),
          "ComputedAt" = NOW()
      `, [SteamId, skatingScore]);
    }

    return NextResponse.json({ ok: true, ticksProcessed: Ticks?.length || 0 });
  } catch (e: any) {
    console.error('Telemetry error:', e?.message);
    return NextResponse.json({ error: e?.message || 'Telemetry error' }, { status: 500 });
  }
}
