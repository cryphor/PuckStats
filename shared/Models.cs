using System.Text.Json.Serialization;

namespace PuckStats.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlayerTeam { None, Blue, Red, Spectator }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlayerRole { Attacker, Goalie }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GamePhase { Warmup, PreGame, FaceOff, Play, Intermission, Replay, PostGame, GameOver }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchType { Public, Pickup, League, Practice, Scrim, Replay }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Archetype { Sniper, Playmaker, Grinder, TwoWayForward, OffensiveDefenseman, DefensiveDefenseman, PuckPossessionSpecialist, Hybrid, Unknown }

public class MatchInfo
{
    public string MatchId { get; set; } = "";
    public MatchType Type { get; set; }
    public string ServerName { get; set; } = "";
    public string Map { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float MatchLengthSeconds { get; set; }
    public int BlueScore { get; set; }
    public int RedScore { get; set; }
    public int Period { get; set; }
    public bool IsOvertime { get; set; }
    public List<PlayerMatchData> Players { get; set; } = new();
    public List<GoalEvent> Goals { get; set; } = new();
    public List<SaveEvent> Saves { get; set; } = new();
    public List<FaceoffEvent> Faceoffs { get; set; } = new();
}

public class PlayerMatchData
{
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public PlayerTeam Team { get; set; }
    public PlayerRole Role { get; set; }
    public int Number { get; set; }
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
    public float PossessionTimeSeconds { get; set; }
    public float DistanceTraveled { get; set; }
    public float AverageSpeed { get; set; }
    public float TopSpeed { get; set; }
    public int PuckTouches { get; set; }
}

public class GoalEvent
{
    public float Timestamp { get; set; }
    public int Period { get; set; }
    public string ScorerSteamId { get; set; } = "";
    public string AssisterSteamId { get; set; } = "";
    public PlayerTeam ScorerTeam { get; set; }
    public float ShotSpeed { get; set; }
    public float ExpectedGoal { get; set; }
}

public class SaveEvent
{
    public float Timestamp { get; set; }
    public string GoalieSteamId { get; set; } = "";
    public string ShooterSteamId { get; set; } = "";
    public float ShotSpeed { get; set; }
}

public class FaceoffEvent
{
    public float Timestamp { get; set; }
    public string WinnerSteamId { get; set; } = "";
    public string LoserSteamId { get; set; } = "";
    public PlayerTeam WinnerTeam { get; set; }
}

public class PlayerRatings
{
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
    public Archetype Archetype { get; set; }
}

public class Percentiles
{
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
}

public class ScoutingReport
{
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public int OverallRating { get; set; }
    public Archetype Archetype { get; set; }
    public string ArchetypeDescription { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<string> ImprovementSuggestions { get; set; } = new();
}

public class HeatmapData
{
    public string Type { get; set; } = "";
    public int RinkWidth { get; set; } = 200;
    public int RinkHeight { get; set; } = 100;
    public List<HeatmapCell> Cells { get; set; } = new();
    public float MaxIntensity { get; set; }
}

public class HeatmapCell
{
    public int X { get; set; }
    public int Y { get; set; }
    public float Intensity { get; set; }
}

public class PassingNetwork
{
    public List<PassingNode> Nodes { get; set; } = new();
    public List<PassingEdge> Edges { get; set; } = new();
}

public class PassingNode
{
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public float Influence { get; set; }
}

public class PassingEdge
{
    public string SourceSteamId { get; set; } = "";
    public string TargetSteamId { get; set; } = "";
    public int PassCount { get; set; }
    public int CompletedCount { get; set; }
    public float SuccessRate { get; set; }
}

public class ShiftData
{
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public float DistanceTraveled { get; set; }
    public float AverageSpeed { get; set; }
    public float PossessionPercent { get; set; }
    public float OffensiveImpact { get; set; }
    public float DefensiveImpact { get; set; }
    public float ShiftScore { get; set; }
}

public class ReplayHeader
{
    public string Version { get; set; } = "1.0";
    public string MatchId { get; set; } = "";
    public string ServerName { get; set; } = "";
    public string Map { get; set; } = "";
    public DateTime StartTime { get; set; }
    public float DurationSeconds { get; set; }
    public int BlueScore { get; set; }
    public int RedScore { get; set; }
    public List<ReplayPlayerInfo> Players { get; set; } = new();
}

public class ReplayPlayerInfo
{
    public ulong ClientId { get; set; }
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public PlayerTeam Team { get; set; }
    public PlayerRole Role { get; set; }
    public int Number { get; set; }
}

public class PlayerTelemetryAggregate
{
    public float AvgTopSpeed, AvgSpeed, AvgBurstSpeed, AvgDirectionChanges, DistancePerMinute, SprintEfficiency;
    public float GoalConversionRate, ShotAccuracy, AvgShotSpeed, GoalsPerGame, ShotDangerScore;
    public float AvgPossessionTime, PossessionEfficiency, TurnoverRate, PuckTouchesPerGame, StickActivity;
    public float PassCompletionRate, PassesPerGame, DangerousPassesPerGame, AssistsPerGame, PassDirectionDiversity;
    public float InputConsistency, InputComplexity, AvgReactionMs, ActionsPerMinute, StickInputEfficiency, WasdBalance;
    public float AvgStickSpeed, StickAnglePrecision, StickEfficiency, StickAngleRange;
    public float OffensiveZoneTime, ShotCreationRate, ChanceCreationRate, OffensivePositioning;
    public float InterceptionsPerGame, BlocksPerGame, DefensivePositioning, PuckRecoveriesPerGame, ZoneCoverage, FaceoffWinRate;
    public float ZoneOccupancyConsistency, AvgSpacingToTeammates, PositionVariance, TeamSupportTime, TransitionSpeed;
    public float WinRateAboveTeam, DecisionQuality, AnticipationScore, TeamSynergy, ClutchScore;

    public static PlayerTelemetryAggregate Neutral() => new()
    {
        AvgTopSpeed = 12.5f, AvgSpeed = 6.8f, AvgBurstSpeed = 14.2f, AvgDirectionChanges = 45f,
        DistancePerMinute = 280f, SprintEfficiency = 0.65f, GoalConversionRate = 0.12f, ShotAccuracy = 0.55f,
        AvgShotSpeed = 75f, GoalsPerGame = 0.8f, ShotDangerScore = 0.35f, AvgPossessionTime = 120f,
        PossessionEfficiency = 0.7f, TurnoverRate = 3.5f, PuckTouchesPerGame = 45f, StickActivity = 60f,
        PassCompletionRate = 0.72f, PassesPerGame = 25f, DangerousPassesPerGame = 2.5f, AssistsPerGame = 0.6f,
        PassDirectionDiversity = 0.6f, InputConsistency = 0.7f, InputComplexity = 0.55f, AvgReactionMs = 250f,
        ActionsPerMinute = 80f, StickInputEfficiency = 0.6f, WasdBalance = 0.7f, AvgStickSpeed = 120f,
        StickAnglePrecision = 0.65f, StickEfficiency = 0.55f, StickAngleRange = 0.6f, OffensiveZoneTime = 0.3f,
        ShotCreationRate = 0.25f, ChanceCreationRate = 0.12f, OffensivePositioning = 0.4f,
        InterceptionsPerGame = 4f, BlocksPerGame = 2f, DefensivePositioning = 0.45f, PuckRecoveriesPerGame = 5f,
        ZoneCoverage = 0.6f, FaceoffWinRate = 0.48f, ZoneOccupancyConsistency = 0.55f, AvgSpacingToTeammates = 0.4f,
        PositionVariance = 2.5f, TeamSupportTime = 0.5f, TransitionSpeed = 0.6f, WinRateAboveTeam = 0.02f,
        DecisionQuality = 0.55f, AnticipationScore = 0.5f, TeamSynergy = 0.45f, ClutchScore = 0.5f
    };
}

// === API RESPONSE MODELS ===
public class PlayerProfile
{
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public PlayerRatings Ratings { get; set; } = new();
    public Percentiles Percentiles { get; set; } = new();
    public Archetype Archetype { get; set; }
    public ScoutingReport ScoutingReport { get; set; } = new();
    public int TotalMatches { get; set; }
    public int TotalGoals { get; set; }
    public int TotalAssists { get; set; }
    public int TotalSaves { get; set; }
    public float WinRate { get; set; }
    public RecentMatch[] RecentMatches { get; set; } = Array.Empty<RecentMatch>();
    public DateTime LastUpdated { get; set; }
}

public class RecentMatch
{
    public string MatchId { get; set; } = "";
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";
    public string Team { get; set; } = "";
    public int TeamScore { get; set; }
    public int OpponentScore { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public float Rating { get; set; }
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public int Rating { get; set; }
    public int MatchesPlayed { get; set; }
    public Archetype Archetype { get; set; }
}

public class CompareResult
{
    public PlayerProfile? PlayerA { get; set; }
    public PlayerProfile? PlayerB { get; set; }
    public Dictionary<string, int> Differences { get; set; } = new();
}

public class DashboardData
{
    public int OverallRating { get; set; }
    public PlayerRatings Ratings { get; set; } = new();
    public Percentiles Percentiles { get; set; } = new();
    public List<RatingTrend> Trends { get; set; } = new();
    public List<RecentMatch> RecentGames { get; set; } = new();
    public int SessionsThisWeek { get; set; }
    public float HoursPlayed { get; set; }
}

public class RatingTrend
{
    public string Category { get; set; } = "";
    public List<TrendPoint> Points { get; set; } = new();
}

public class TrendPoint
{
    public DateTime Date { get; set; }
    public int Rating { get; set; }
}
