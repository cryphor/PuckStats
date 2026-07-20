// PuckStats Data Models — all types shared across the mod.
// Enums that exist in the game (PlayerTeam, PlayerRole, PlayerHandedness, GamePhase) are NOT redefined here.
// Unity-serializable: uses fields (not properties) for JsonUtility compatibility.

using System;
using System.Collections.Generic;

namespace PuckStats
{
    // === PUCKSTATS-ONLY ENUMS ===
    public enum MatchType { Public, Pickup, League, Practice, Scrim, Replay }
    public enum Archetype { Sniper, Playmaker, Grinder, TwoWayForward, OffensiveDefenseman, DefensiveDefenseman, PuckPossessionSpecialist, Hybrid, Unknown }

    // === PRIMITIVES (Unity serializable) ===
    [Serializable]
    public class Vec3
    {
        public float X, Y, Z;
    }

    [Serializable]
    public class Vec2
    {
        public float X, Y;
    }

    // === TELEMETRY (Unity serializable — used by JsonUtility) ===
    [Serializable]
    public class TelemetryTick
    {
        public float Timestamp;
        public int TickNumber;
        public Vec3 Position = new();
        public Vec3 Velocity = new();
        public Vec3 Acceleration = new();
        public Vec3 Direction = new();
        public float StickAngle;
        public InputSnapshot Input = new();
        public MovementState MoveState = new();
    }

    [Serializable]
    public class InputSnapshot
    {
        public Vec2 MoveInput = new();
        public Vec2 LookAngleInput = new();
        public Vec2 StickRaycastAngleInput = new();
        public float BladeAngle;
        public bool Sprinting;
        public bool Sliding;
        public bool Jumping;
        public bool Stopping;
        public bool Tracking;
    }

    [Serializable]
    public class MovementState
    {
        public bool IsMoving;
        public bool IsSprinting;
        public bool IsSliding;
        public bool IsJumping;
        public bool IsStopped;
        public bool HasFallen;
    }

    // === NETWORK PAYLOADS (Unity serializable) ===
    [Serializable]
    public class TelemetryBatch
    {
        public string MatchId;
        public string SteamId;
        public string SessionId;
        public TelemetryTick[] Ticks;
    }

    [Serializable]
    public class HeartbeatData
    {
        public string SteamId;
        public string MatchId;
        public float Timestamp;
    }

    [Serializable]
    public class MatchInfo
    {
        public string MatchId;
        public string ServerName;
        public float MatchLengthSeconds;
        public int BlueScore;
        public int RedScore;
        public string SteamId;
        public string Username;
        public string Team;
        public float DistanceTraveled;
        public float AverageSpeed;
        public float TopSpeed;
        public int Goals;
        public int Assists;
        public int PuckTouches;
        public float PossessionTimeSeconds;
    }

    // === ANALYTICS TYPES ===
    public class PlayerRatings
    {
        public int Skating, Shooting, Stickhandling, Passing, Inputs, StickMotion,
                   OffensivePlay, DefensivePlay, Positioning, GameSense, Overall;
        public Archetype Archetype;
    }

    public class Percentiles
    {
        public int SkatingPercentile, ShootingPercentile, StickhandlingPercentile,
                   PassingPercentile, InputsPercentile, StickMotionPercentile,
                   OffensivePlayPercentile, DefensivePlayPercentile, PositioningPercentile,
                   GameSensePercentile, OverallPercentile;
    }

    public class ScoutingReport
    {
        public List<string> Strengths = new();
        public List<string> Weaknesses = new();
        public int OverallRating;
        public Archetype Archetype;
        public string ArchetypeDescription = "";
        public string Summary = "";
        public List<KeyMoment> KeyMoments = new();
        public List<string> ImprovementSuggestions = new();
    }

    public class KeyMoment
    {
        public float Timestamp;
        public string Type = "", Description = "", Severity = "";
    }

    // === HEATMAPS ===
    public class HeatmapData
    {
        public string Type = "";
        public int RinkWidth = 200, RinkHeight = 100;
        public List<HeatmapCell> Cells = new();
        public float MaxIntensity;
    }

    public class HeatmapCell
    {
        public int X, Y;
        public float Intensity;
    }

    // === PASSING NETWORK ===
    public class PassingNetwork
    {
        public List<PassingNode> Nodes = new();
        public List<PassingEdge> Edges = new();
    }

    public class PassingNode
    {
        public string SteamId = "", Username = "";
        public Vec2 Position = new();
        public float Influence;
    }

    public class PassingEdge
    {
        public string SourceSteamId = "", TargetSteamId = "";
        public int PassCount, CompletedCount;
        public float SuccessRate;
    }

    // === SHIFT ANALYTICS ===
    public class ShiftData
    {
        public float StartTime, EndTime, DistanceTraveled, AverageSpeed,
                     PossessionPercent, OffensiveImpact, DefensiveImpact, ShiftScore;
    }

    // === MOVEMENT / INPUT ANALYTICS ===
    public class MovementAnalytics
    {
        public float DistanceTraveled, AverageSpeed, TopSpeed, CruiseSpeed,
                     BurstSpeed, DirectionChangesPerMinute;
        public int JumpCount, SprintToggleCount, SlideToggleCount, StopCount;
    }

    public class InputAnalytics
    {
        public int WPressedCount, APressedCount, SPressedCount, DPressedCount,
                   TotalInputEvents, JumpCount, SprintToggleCount, SlideToggleCount, StopCount;
    }
}
