let pool: any = null;

export async function query(sqlQuery: string, params?: any[]) {
  if (!pool) {
    const { Pool } = require('pg');
    const connStr = process.env.POSTGRES_URL_NON_POOLING || process.env.POSTGRES_URL || process.env.DATABASE_URL;
    if (!connStr) throw new Error('No database URL found');
    const cleanUrl = connStr.replace(/[\?&]sslmode=[^&]*/g, '');
    pool = new Pool({ connectionString: cleanUrl, ssl: { rejectUnauthorized: false }, max: 1, idleTimeoutMillis: 5000, connectionTimeoutMillis: 10000 });
  }
  const client = await pool.connect();
  try { return await client.query(sqlQuery, params); } finally { client.release(); }
}

export async function initTables() {
  try {
    await query(`
      CREATE TABLE IF NOT EXISTS "Players" (
        "SteamId" VARCHAR(32) PRIMARY KEY,
        "Username" VARCHAR(128) NOT NULL DEFAULT '',
        "AvatarUrl" TEXT DEFAULT '',
        "Team" VARCHAR(16) DEFAULT 'None',
        "Archetype" VARCHAR(32) DEFAULT 'Unknown',
        "Role" VARCHAR(16) DEFAULT 'Attacker',
        "Handedness" VARCHAR(8) DEFAULT 'Right',
        -- Basic
        "TotalMatches" INT DEFAULT 0,
        "TotalWins" INT DEFAULT 0,
        "TotalLosses" INT DEFAULT 0,
        "TotalOtl" INT DEFAULT 0,
        -- Scoring
        "TotalGoals" INT DEFAULT 0,
        "TotalAssists" INT DEFAULT 0,
        "TotalPoints" INT DEFAULT 0,
        "PlusMinus" INT DEFAULT 0,
        "TotalPim" INT DEFAULT 0,
        "TotalTimeOnIce" DOUBLE PRECISION DEFAULT 0,
        "TotalShots" INT DEFAULT 0,
        "TotalShotAttempts" INT DEFAULT 0,
        "TotalHits" INT DEFAULT 0,
        -- Shooting detail
        "HighDangerGoals" INT DEFAULT 0,
        "LowDangerGoals" INT DEFAULT 0,
        "PowerplayGoals" INT DEFAULT 0,
        "ShorthandedGoals" INT DEFAULT 0,
        "GameWinningGoals" INT DEFAULT 0,
        -- Passing
        "PrimaryAssists" INT DEFAULT 0,
        "SecondaryAssists" INT DEFAULT 0,
        "TotalPasses" INT DEFAULT 0,
        "PassesCompleted" INT DEFAULT 0,
        "DangerousPasses" INT DEFAULT 0,
        -- Faceoffs
        "FaceoffsTaken" INT DEFAULT 0,
        "FaceoffsWon" INT DEFAULT 0,
        -- Possession
        "TotalPossessionTime" REAL DEFAULT 0,
        "TotalPuckTouches" INT DEFAULT 0,
        "Turnovers" INT DEFAULT 0,
        "Recoveries" INT DEFAULT 0,
        -- Defense
        "TotalBlocks" INT DEFAULT 0,
        "TotalInterceptions" INT DEFAULT 0,
        "Takeaways" INT DEFAULT 0,
        "StickChecks" INT DEFAULT 0,
        "PokeChecks" INT DEFAULT 0,
        -- Special teams
        "PowerplayAssists" INT DEFAULT 0,
        "ShorthandedAssists" INT DEFAULT 0,
        -- On-ice
        "OnIceGoalsFor" INT DEFAULT 0,
        "OnIceGoalsAgainst" INT DEFAULT 0,
        -- Distance
        "TotalDistanceTraveled" REAL DEFAULT 0,
        "TotalPlayTimeSeconds" DOUBLE PRECISION DEFAULT 0,
        "OverallRating" INT DEFAULT 50,
        "FirstSeen" TIMESTAMPTZ DEFAULT NOW(),
        "LastSeen" TIMESTAMPTZ DEFAULT NOW(),
        "LastUpdated" TIMESTAMPTZ DEFAULT NOW()
      )
    `);

    // Add missing columns for existing tables
    const migrations = [
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Team" VARCHAR(16) DEFAULT \'None\'',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Role" VARCHAR(16) DEFAULT \'Attacker\'',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Handedness" VARCHAR(8) DEFAULT \'Right\'',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalLosses" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalOtl" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalPoints" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PlusMinus" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalPim" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalTimeOnIce" DOUBLE PRECISION DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalShots" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalShotAttempts" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalHits" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "HighDangerGoals" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "LowDangerGoals" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PowerplayGoals" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "ShorthandedGoals" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "GameWinningGoals" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PrimaryAssists" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "SecondaryAssists" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalPasses" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PassesCompleted" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "DangerousPasses" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "FaceoffsTaken" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "FaceoffsWon" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalPossessionTime" REAL DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalPuckTouches" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Turnovers" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Recoveries" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalBlocks" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "TotalInterceptions" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "Takeaways" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "StickChecks" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PokeChecks" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "PowerplayAssists" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "ShorthandedAssists" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "OnIceGoalsFor" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "OnIceGoalsAgainst" INT DEFAULT 0',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "FirstSeen" TIMESTAMPTZ DEFAULT NOW()',
      'ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "LastSeen" TIMESTAMPTZ DEFAULT NOW()',
    ];
    for (const sql of migrations) { try { await query(sql); } catch {} }

    await query(`
      CREATE TABLE IF NOT EXISTS "PlayerRatings" (
        "SteamId" VARCHAR(32) PRIMARY KEY,
        "Skating" INT DEFAULT 50, "Shooting" INT DEFAULT 50,
        "Stickhandling" INT DEFAULT 50, "Passing" INT DEFAULT 50,
        "Inputs" INT DEFAULT 50, "StickMotion" INT DEFAULT 50,
        "OffensivePlay" INT DEFAULT 50, "DefensivePlay" INT DEFAULT 50,
        "Positioning" INT DEFAULT 50, "GameSense" INT DEFAULT 50,
        "Overall" INT DEFAULT 50, "OverallPercentile" INT DEFAULT 50,
        "ComputedAt" TIMESTAMPTZ DEFAULT NOW()
      )
    `);

    await query(`
      CREATE TABLE IF NOT EXISTS "RatingHistory" (
        "Id" BIGSERIAL PRIMARY KEY,
        "SteamId" VARCHAR(32) NOT NULL,
        "OverallRating" INT DEFAULT 50,
        "RecordedAt" TIMESTAMPTZ DEFAULT NOW()
      )
    `);
  } catch (err: any) { console.error('initTables error:', err?.message); }
}
