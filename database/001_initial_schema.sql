-- PuckStats Database Migration
-- PostgreSQL schema for the PuckStats analytics platform

-- Enable extensions
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================
-- PLAYERS
-- ============================================================
CREATE TABLE IF NOT EXISTS "Players" (
    "SteamId"       VARCHAR(32) PRIMARY KEY,
    "Username"      VARCHAR(128) NOT NULL,
    "AvatarUrl"     TEXT DEFAULT '',
    "Team"          VARCHAR(16) DEFAULT 'None',
    "Role"          VARCHAR(16) DEFAULT 'Attacker',
    "Handedness"    VARCHAR(8) DEFAULT 'Right',

    -- Aggregate stats
    "TotalMatches"          INT DEFAULT 0,
    "TotalGoals"            INT DEFAULT 0,
    "TotalAssists"          INT DEFAULT 0,
    "TotalSaves"            INT DEFAULT 0,
    "TotalShots"            INT DEFAULT 0,
    "TotalWins"             INT DEFAULT 0,
    "TotalLosses"           INT DEFAULT 0,
    "TotalDistanceTraveled" REAL DEFAULT 0,
    "TotalPossessionTime"   REAL DEFAULT 0,
    "TotalPlayTimeSeconds"  DOUBLE PRECISION DEFAULT 0,

    -- Profile
    "Archetype"         VARCHAR(32) DEFAULT 'Unknown',
    "OverallRating"     INT DEFAULT 50,
    "ScoutingReportJson" TEXT DEFAULT '{}',

    "FirstSeen"   TIMESTAMPTZ DEFAULT NOW(),
    "LastSeen"    TIMESTAMPTZ DEFAULT NOW(),
    "LastUpdated" TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX "IX_Players_Username" ON "Players" ("Username");
CREATE INDEX "IX_Players_OverallRating" ON "Players" ("OverallRating" DESC);
CREATE INDEX "IX_Players_Archetype" ON "Players" ("Archetype");

-- ============================================================
-- MATCHES
-- ============================================================
CREATE TABLE IF NOT EXISTS "Matches" (
    "MatchId"        VARCHAR(32) PRIMARY KEY,
    "Type"           VARCHAR(64) DEFAULT 'Public',
    "ServerName"     VARCHAR(128) DEFAULT '',
    "Map"            VARCHAR(64) DEFAULT '',
    "StartTime"      TIMESTAMPTZ NOT NULL,
    "EndTime"        TIMESTAMPTZ NOT NULL,
    "DurationSeconds" REAL DEFAULT 0,
    "BlueScore"      INT DEFAULT 0,
    "RedScore"       INT DEFAULT 0,
    "Period"         INT DEFAULT 1,
    "IsOvertime"     BOOLEAN DEFAULT FALSE
);

CREATE INDEX "IX_Matches_StartTime" ON "Matches" ("StartTime" DESC);
CREATE INDEX "IX_Matches_Type" ON "Matches" ("Type");

-- ============================================================
-- MATCH PLAYERS (join table)
-- ============================================================
CREATE TABLE IF NOT EXISTS "MatchPlayers" (
    "Id"            BIGSERIAL PRIMARY KEY,
    "MatchId"       VARCHAR(32) NOT NULL REFERENCES "Matches"("MatchId"),
    "SteamId"       VARCHAR(32) NOT NULL REFERENCES "Players"("SteamId"),
    "Team"          VARCHAR(16) DEFAULT '',
    "Role"          VARCHAR(16) DEFAULT '',

    -- Per-match stats
    "Goals"                 INT DEFAULT 0,
    "Assists"               INT DEFAULT 0,
    "Saves"                 INT DEFAULT 0,
    "Shots"                 INT DEFAULT 0,
    "Passes"                INT DEFAULT 0,
    "PassesCompleted"       INT DEFAULT 0,
    "Hits"                  INT DEFAULT 0,
    "Interceptions"         INT DEFAULT 0,
    "Blocks"                INT DEFAULT 0,
    "FaceoffsWon"           INT DEFAULT 0,
    "FaceoffsTaken"         INT DEFAULT 0,
    "Turnovers"             INT DEFAULT 0,
    "PenaltyMinutes"        INT DEFAULT 0,
    "PossessionTimeSeconds" REAL DEFAULT 0,
    "DistanceTraveled"      REAL DEFAULT 0,
    "AverageSpeed"          REAL DEFAULT 0,
    "TopSpeed"              REAL DEFAULT 0,
    "PuckTouches"           INT DEFAULT 0,
    "MatchRating"           REAL DEFAULT 50
);

CREATE UNIQUE INDEX "IX_MatchPlayers_Match_Player" ON "MatchPlayers" ("MatchId", "SteamId");
CREATE INDEX "IX_MatchPlayers_SteamId" ON "MatchPlayers" ("SteamId");

-- ============================================================
-- REPLAYS
-- ============================================================
CREATE TABLE IF NOT EXISTS "Replays" (
    "Id"                  BIGINT PRIMARY KEY,
    "MatchId"             VARCHAR(32) DEFAULT '',
    "UploadedBySteamId"   VARCHAR(32) DEFAULT '',
    "FilePath"            TEXT DEFAULT '',
    "FileSizeBytes"       BIGINT DEFAULT 0,
    "ParsedDataJson"      JSONB DEFAULT '{}',
    "AiAnalysisJson"      JSONB DEFAULT '{}',
    "UploadedAt"          TIMESTAMPTZ DEFAULT NOW(),
    "IsProcessed"         BOOLEAN DEFAULT FALSE
);

CREATE INDEX "IX_Replays_MatchId" ON "Replays" ("MatchId");
CREATE INDEX "IX_Replays_UploadedBy" ON "Replays" ("UploadedBySteamId");
CREATE INDEX "IX_Replays_UploadedAt" ON "Replays" ("UploadedAt" DESC);

-- ============================================================
-- MATCH EVENTS
-- ============================================================
CREATE TABLE IF NOT EXISTS "MatchEvents" (
    "Id"                      BIGSERIAL PRIMARY KEY,
    "MatchId"                 VARCHAR(32) NOT NULL,
    "Type"                    VARCHAR(32) NOT NULL,
    "Timestamp"               REAL DEFAULT 0,
    "Period"                  INT DEFAULT 1,
    "PlayerSteamId"           VARCHAR(32) DEFAULT '',
    "SecondaryPlayerSteamId"  VARCHAR(32) DEFAULT '',
    "Team"                    VARCHAR(16) DEFAULT '',
    "EventDataJson"           JSONB DEFAULT '{}'
);

CREATE INDEX "IX_MatchEvents_MatchId" ON "MatchEvents" ("MatchId");
CREATE INDEX "IX_MatchEvents_Type" ON "MatchEvents" ("Type");
CREATE INDEX "IX_MatchEvents_Player" ON "MatchEvents" ("PlayerSteamId");

-- ============================================================
-- PLAYER RATINGS
-- ============================================================
CREATE TABLE IF NOT EXISTS "PlayerRatings" (
    "SteamId"           VARCHAR(32) PRIMARY KEY REFERENCES "Players"("SteamId"),
    "Skating"           INT DEFAULT 50,
    "Shooting"          INT DEFAULT 50,
    "Stickhandling"     INT DEFAULT 50,
    "Passing"           INT DEFAULT 50,
    "Inputs"            INT DEFAULT 50,
    "StickMotion"       INT DEFAULT 50,
    "OffensivePlay"     INT DEFAULT 50,
    "DefensivePlay"     INT DEFAULT 50,
    "Positioning"       INT DEFAULT 50,
    "GameSense"         INT DEFAULT 50,
    "Overall"           INT DEFAULT 50,

    -- Percentiles
    "SkatingPercentile"       INT DEFAULT 50,
    "ShootingPercentile"      INT DEFAULT 50,
    "StickhandlingPercentile" INT DEFAULT 50,
    "PassingPercentile"       INT DEFAULT 50,
    "InputsPercentile"        INT DEFAULT 50,
    "StickMotionPercentile"   INT DEFAULT 50,
    "OffensivePlayPercentile" INT DEFAULT 50,
    "DefensivePlayPercentile" INT DEFAULT 50,
    "PositioningPercentile"   INT DEFAULT 50,
    "GameSensePercentile"     INT DEFAULT 50,
    "OverallPercentile"       INT DEFAULT 50,

    "ComputedAt" TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================
-- RATING HISTORY (time series)
-- ============================================================
CREATE TABLE IF NOT EXISTS "RatingHistory" (
    "Id"            BIGSERIAL PRIMARY KEY,
    "SteamId"       VARCHAR(32) NOT NULL,
    "OverallRating" INT DEFAULT 50,
    "RatingsJson"   JSONB DEFAULT '{}',
    "RecordedAt"    TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX "IX_RatingHistory_SteamId_Date" ON "RatingHistory" ("SteamId", "RecordedAt" DESC);

-- ============================================================
-- HEATMAPS
-- ============================================================
CREATE TABLE IF NOT EXISTS "Heatmaps" (
    "Id"      BIGSERIAL PRIMARY KEY,
    "MatchId" VARCHAR(32) NOT NULL,
    "Type"    VARCHAR(32) NOT NULL,
    "Data"    JSONB DEFAULT '{}'
);

CREATE INDEX "IX_Heatmaps_Match_Type" ON "Heatmaps" ("MatchId", "Type");

-- ============================================================
-- SHIFTS
-- ============================================================
CREATE TABLE IF NOT EXISTS "Shifts" (
    "Id"                BIGSERIAL PRIMARY KEY,
    "MatchId"           VARCHAR(32) NOT NULL,
    "SteamId"           VARCHAR(32) NOT NULL,
    "StartTime"         REAL DEFAULT 0,
    "EndTime"           REAL DEFAULT 0,
    "DistanceTraveled"  REAL DEFAULT 0,
    "AverageSpeed"      REAL DEFAULT 0,
    "PossessionPercent" REAL DEFAULT 0,
    "OffensiveImpact"   REAL DEFAULT 0,
    "DefensiveImpact"   REAL DEFAULT 0,
    "ShiftScore"        REAL DEFAULT 0
);

CREATE INDEX "IX_Shifts_Match_Player" ON "Shifts" ("MatchId", "SteamId");

-- ============================================================
-- TELEMETRY TICKS
-- ============================================================
CREATE TABLE IF NOT EXISTS "TelemetryTicks" (
    "Id"            BIGSERIAL PRIMARY KEY,
    "MatchId"       VARCHAR(32) NOT NULL,
    "SteamId"       VARCHAR(32) NOT NULL,
    "Timestamp"     REAL DEFAULT 0,
    "TickNumber"    INT DEFAULT 0,
    "PositionData"  JSONB DEFAULT '{}',
    "Speed"         REAL DEFAULT 0,
    "InputDataJson" TEXT DEFAULT '{}',
    "MoveStateJson" TEXT DEFAULT '{}'
);

CREATE INDEX "IX_TelemetryTicks_Match_Player_Tick" ON "TelemetryTicks" ("MatchId", "SteamId", "TickNumber");

-- ============================================================
-- HELPER FUNCTIONS
-- ============================================================

-- Function to clean up old telemetry (retention: 90 days)
CREATE OR REPLACE FUNCTION cleanup_old_telemetry()
RETURNS void AS $$
BEGIN
    DELETE FROM "TelemetryTicks"
    WHERE "Id" IN (
        SELECT t."Id" FROM "TelemetryTicks" t
        JOIN "Matches" m ON t."MatchId" = m."MatchId"
        WHERE m."StartTime" < NOW() - INTERVAL '90 days'
        LIMIT 10000
    );
END;
$$ LANGUAGE plpgsql;

-- Function to recompute player aggregate stats
CREATE OR REPLACE FUNCTION recompute_player_stats(player_id VARCHAR)
RETURNS void AS $$
DECLARE
    v_matches INT;
    v_goals INT;
    v_assists INT;
    v_saves INT;
    v_wins INT;
    v_losses INT;
BEGIN
    SELECT
        COUNT(*),
        COALESCE(SUM("Goals"), 0),
        COALESCE(SUM("Assists"), 0),
        COALESCE(SUM("Saves"), 0),
        COALESCE(SUM(CASE WHEN "Team" = 'Blue' AND EXISTS(SELECT 1 FROM "Matches" m WHERE m."MatchId" = "MatchPlayers"."MatchId" AND m."BlueScore" > m."RedScore") THEN 1 ELSE 0 END), 0),
        COALESCE(SUM(CASE WHEN "Team" = 'Blue' AND EXISTS(SELECT 1 FROM "Matches" m WHERE m."MatchId" = "MatchPlayers"."MatchId" AND m."BlueScore" < m."RedScore") THEN 1 ELSE 0 END), 0)
    INTO v_matches, v_goals, v_assists, v_saves, v_wins, v_losses
    FROM "MatchPlayers"
    WHERE "SteamId" = player_id;

    UPDATE "Players"
    SET "TotalMatches" = v_matches,
        "TotalGoals" = v_goals,
        "TotalAssists" = v_assists,
        "TotalSaves" = v_saves,
        "TotalWins" = v_wins,
        "TotalLosses" = v_losses,
        "LastUpdated" = NOW()
    WHERE "SteamId" = player_id;
END;
$$ LANGUAGE plpgsql;
