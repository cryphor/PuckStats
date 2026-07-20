using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuckStats.Api.Data;

// === ENTITY MODELS ===

public class PlayerEntity
{
    [Key]
    public string SteamId { get; set; } = "";
    
    [MaxLength(128)]
    public string Username { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string Team { get; set; } = "None";
    public string Role { get; set; } = "Attacker";
    public string Handedness { get; set; } = "Right";

    // Aggregate stats
    public int TotalMatches { get; set; }
    public int TotalGoals { get; set; }
    public int TotalAssists { get; set; }
    public int TotalSaves { get; set; }
    public int TotalShots { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public float TotalDistanceTraveled { get; set; }
    public float TotalPossessionTime { get; set; }
    public double TotalPlayTimeSeconds { get; set; }

    // Profile
    public string Archetype { get; set; } = "Unknown";
    public int OverallRating { get; set; }
    public string ScoutingReportJson { get; set; } = "{}";

    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class MatchEntity
{
    [Key]
    public string MatchId { get; set; } = "";
    
    [MaxLength(64)]
    public string Type { get; set; } = "Public";
    
    [MaxLength(128)]
    public string ServerName { get; set; } = "";
    
    [MaxLength(64)]
    public string Map { get; set; } = "";

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float DurationSeconds { get; set; }
    public int BlueScore { get; set; }
    public int RedScore { get; set; }
    public int Period { get; set; }
    public bool IsOvertime { get; set; }

    public List<MatchPlayerEntity> Players { get; set; } = new();
}

public class MatchPlayerEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public string SteamId { get; set; } = "";
    public string Team { get; set; } = "";
    public string Role { get; set; } = "";

    // Per-match stats
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Saves { get; set; }
    public int Shots { get; set; }
    public int Passes { get; set; }
    public int PassesCompleted { get; set; }
    public int Hits { get; set; }
    public int Interceptions { get; set; }
    public int Blocks { get; set; }
    public int FaceoffsWon { get; set; }
    public int FaceoffsTaken { get; set; }
    public int Turnovers { get; set; }
    public int PenaltyMinutes { get; set; }
    public float PossessionTimeSeconds { get; set; }
    public float DistanceTraveled { get; set; }
    public float AverageSpeed { get; set; }
    public float TopSpeed { get; set; }
    public int PuckTouches { get; set; }

    // Performance
    public float MatchRating { get; set; }
}

public class ReplayEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public string UploadedBySteamId { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string ParsedDataJson { get; set; } = "{}";
    public string AiAnalysisJson { get; set; } = "{}";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; }
}

public class MatchEventEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    
    [MaxLength(32)]
    public string Type { get; set; } = ""; // Goal, Save, Faceoff, Hit, Penalty
    
    public float Timestamp { get; set; }
    public int Period { get; set; }
    public string PlayerSteamId { get; set; } = "";
    public string SecondaryPlayerSteamId { get; set; } = "";
    public string Team { get; set; } = "";
    public string EventDataJson { get; set; } = "{}"; // Additional event-specific data
}

public class PlayerRatingEntity
{
    [Key]
    public string SteamId { get; set; } = "";

    public int Skating { get; set; }
    public int Shooting { get; set; }
    public int Stickhandling { get; set; }
    public int Passing { get; set; }
    public int Inputs { get; set; }
    public int StickMotion { get; set; }
    public int OffensivePlay { get; set; }
    public int DefensivePlay { get; set; }
    public int Positioning { get; set; }
    public int GameSense { get; set; }
    public int Overall { get; set; }

    // Percentiles
    public int SkatingPercentile { get; set; }
    public int ShootingPercentile { get; set; }
    public int StickhandlingPercentile { get; set; }
    public int PassingPercentile { get; set; }
    public int InputsPercentile { get; set; }
    public int StickMotionPercentile { get; set; }
    public int OffensivePlayPercentile { get; set; }
    public int DefensivePlayPercentile { get; set; }
    public int PositioningPercentile { get; set; }
    public int GameSensePercentile { get; set; }
    public int OverallPercentile { get; set; }

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
}

public class RatingHistoryEntity
{
    public long Id { get; set; }
    public string SteamId { get; set; } = "";
    public int OverallRating { get; set; }
    public string RatingsJson { get; set; } = "{}"; // Full ratings snapshot
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class HeatmapEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public string Type { get; set; } = ""; // Skating, Possession, Shot
    
    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}"; // HeatmapData serialized
}

public class ShiftEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public string SteamId { get; set; } = "";
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public float DistanceTraveled { get; set; }
    public float AverageSpeed { get; set; }
    public float PossessionPercent { get; set; }
    public float OffensiveImpact { get; set; }
    public float DefensiveImpact { get; set; }
    public float ShiftScore { get; set; }
}

public class TelemetryTickEntity
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public string SteamId { get; set; } = "";
    public float Timestamp { get; set; }
    public int TickNumber { get; set; }

    [Column(TypeName = "jsonb")]
    public string PositionData { get; set; } = "{}"; // {x, y, z, vx, vy, vz}
    
    public float Speed { get; set; }
    public string InputDataJson { get; set; } = "{}";
    public string MoveStateJson { get; set; } = "{}";
}
