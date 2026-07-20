const SB = process.env.SUPABASE_URL || '';
const KEY = process.env.SUPABASE_SERVICE_ROLE_KEY || process.env.SUPABASE_ANON_KEY || '';
const H = { 'apikey': KEY, 'Authorization': `Bearer ${KEY}`, 'Content-Type': 'application/json' };

export async function query(sql: string, params?: any[]) {
  try {
    const { Pool } = require('pg');
    const p = new Pool({ connectionString: process.env.POSTGRES_URL, ssl: { rejectUnauthorized: false }, max: 1 });
    const c = await p.connect(); const r = await c.query(sql, params);
    c.release(); await p.end(); return r;
  } catch { return { rows: [] }; }
}

export async function initTables() {
  try {
    const r = await fetch(`${SB}/rest/v1/Players?select=SteamId&limit=1`, { headers: H });
    if (r.ok) return;
  } catch {}
  try {
    const { Pool } = require('pg');
    const p = new Pool({ connectionString: process.env.POSTGRES_URL, ssl: { rejectUnauthorized: false }, max: 1 });
    const c = await p.connect();
    await c.query(`CREATE TABLE IF NOT EXISTS "Players" ("SteamId" VARCHAR(32) PRIMARY KEY, "Username" VARCHAR(128) DEFAULT '', "Team" VARCHAR(16) DEFAULT 'None', "Archetype" VARCHAR(32) DEFAULT 'Unknown', "TotalMatches" INT DEFAULT 0, "TotalWins" INT DEFAULT 0, "TotalGoals" INT DEFAULT 0, "TotalAssists" INT DEFAULT 0, "TotalPoints" INT DEFAULT 0, "PlusMinus" INT DEFAULT 0, "TotalDistanceTraveled" REAL DEFAULT 0, "TotalPlayTimeSeconds" DOUBLE PRECISION DEFAULT 0, "TotalPuckTouches" INT DEFAULT 0, "TotalPossessionTime" REAL DEFAULT 0, "AverageSpeed" REAL DEFAULT 0, "TopSpeed" REAL DEFAULT 0, "LastUpdated" TIMESTAMPTZ DEFAULT NOW())`);
    c.release(); await p.end();
  } catch {}
}

export async function getPlayer(steamId: string) {
  const res = await fetch(`${SB}/rest/v1/Players?SteamId=eq.${encodeURIComponent(steamId)}&select=*`, { headers: H });
  if (!res.ok) return null;
  const rows = await res.json();
  return rows?.[0] || null;
}

export async function upsertPlayer(d: any) {
  const sid = d.SteamId;
  if (!sid) return { error: 'No SteamId' };

  // Get current values
  let prev: any = {};
  try { const r = await getPlayer(sid); if (r) prev = r; } catch {}

  const g = Math.max(0, Math.floor(d.Goals || 0));
  const a = Math.max(0, Math.floor(d.Assists || 0));
  const pm = (d.BlueScore || 0) - (d.RedScore || 0);
  const w = (d.BlueScore || 0) > (d.RedScore || 0) ? 1 : 0;
  const _ = (v: any) => v ?? 0;

  const row = {
    SteamId: sid, Username: (d.Username || _(prev.Username) || '').substring(0, 128),
    Team: d.Team || _(prev.Team) || 'None',
    TotalMatches: _(prev.TotalMatches || prev.totalmatches) + 1,
    TotalWins: _(prev.TotalWins || prev.totalwins) + w,
    TotalGoals: _(prev.TotalGoals || prev.totalgoals) + g,
    TotalAssists: _(prev.TotalAssists || prev.totalassists) + a,
    TotalPoints: _(prev.TotalPoints || prev.totalpoints) + g + a,
    PlusMinus: _(prev.PlusMinus || prev.plusminus) + pm,
    TotalDistanceTraveled: _(prev.TotalDistanceTraveled || prev.totaldistancetraveled) + parseFloat(d.DistanceTraveled || 0),
    TotalPlayTimeSeconds: _(prev.TotalPlayTimeSeconds || prev.totalplaytimeseconds) + parseFloat(d.MatchLengthSeconds || 0),
    TotalPuckTouches: _(prev.TotalPuckTouches || prev.totalpucktouches) + Math.floor(d.PuckTouches || 0),
    TotalPossessionTime: _(prev.TotalPossessionTime || prev.totalpossessiontime) + parseFloat(d.PossessionTimeSeconds || 0),
    AverageSpeed: parseFloat(d.AverageSpeed) || _(prev.AverageSpeed || prev.averagespeed),
    TopSpeed: Math.max(parseFloat(d.TopSpeed) || 0, _(prev.TopSpeed || prev.topspeed)),
    LastUpdated: new Date().toISOString(),
  };

  try {
    const res = await fetch(`${SB}/rest/v1/Players?on_conflict=SteamId`, {
      method: 'POST',
      headers: { ...H, 'Prefer': 'resolution=merge-duplicates' },
      body: JSON.stringify(row),
    });
    const txt = await res.text();
    if (!res.ok) return { error: `SUPABASE ${res.status}: ${txt.substring(0,200)}` };
    return { ok: true, debug: `${res.status} ${txt.substring(0,50)}` };
  } catch (e: any) { return { error: e.message }; }
}
