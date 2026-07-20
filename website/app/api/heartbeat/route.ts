import { NextRequest, NextResponse } from 'next/server';
import { query, initTables } from '@/lib/db';

export async function POST(req: NextRequest) {
  try {
    await initTables();
    const body = await req.json();
    const { SteamId, MatchId, Timestamp } = body;
    // Just acknowledge the heartbeat - no storage needed yet
    return NextResponse.json({ ok: true, time: Timestamp });
  } catch {
    return NextResponse.json({ ok: true });
  }
}
