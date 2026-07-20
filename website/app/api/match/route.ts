import { NextRequest, NextResponse } from 'next/server';
import { upsertPlayer, initTables } from '@/lib/db';

export async function GET() {
  return NextResponse.json({ ok: true, message: 'Match API — send POST' });
}

export async function POST(req: NextRequest) {
  try {
    await initTables();
    const body = await req.json();
    const { MatchId, SteamId } = body;
    if (!SteamId || !MatchId) {
      return NextResponse.json({ error: 'Missing SteamId or MatchId' }, { status: 400 });
    }
    const result = await upsertPlayer(body);
    if (result.error) {
      return NextResponse.json({ error: result.error }, { status: 500 });
    }
    return NextResponse.json({ ok: true, message: 'Match recorded' });
  } catch (e: any) {
    console.error('Match error:', e?.message);
    return NextResponse.json({ error: e?.message || 'Match error' }, { status: 500 });
  }
}
