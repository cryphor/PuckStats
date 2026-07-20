using System.Collections.Generic;
using UnityEngine;

namespace PuckStats
{
    /// <summary>
    /// Collects and aggregates match telemetry data into structured analytics.
    /// Processes per-tick movement data, input data, and game events.
    /// </summary>
    public class TelemetryCollector
    {
        public TelemetryTick LastTick { get; private set; }
        public PlayerBody LocalBody { get; set; }
        public Player LocalPlayer { get; private set; }

        private List<TelemetryTick> _allTicks = new(72000);
        private float _distanceTraveled;
        private float _maxSpeed;
        private float _totalSpeed;
        private int _sampleCount;
    private int _puckTouches;
    private float _possessionTime;
    private int _directionChanges;
        private Vector3 _lastDirection = Vector3.zero;
        private const float DirectionChangeThreshold = 45f;

        private int _wPressedCount, _aPressedCount, _sPressedCount, _dPressedCount;
        private bool _wasWPressed, _wasAPressed, _wasSPressed, _wasDPressed;
        private int _jumpCount, _sprintToggleCount, _slideToggleCount, _stopCount;
        private bool _wasSprinting, _wasSliding, _wasStopped, _wasSPressed_init;

        private GamePhase _currentPhase;
        private int _currentPeriod, _blueScore, _redScore;

        private List<string> _goalScorers = new();
        private List<string> _assisters = new();

        public void Reset()
        {
            _allTicks.Clear();
            _distanceTraveled = 0f; _maxSpeed = 0f; _totalSpeed = 0f; _sampleCount = 0;
            _puckTouches = 0; _possessionTime = 0f;
            _directionChanges = 0; _lastDirection = Vector3.zero;
            _wPressedCount = _aPressedCount = _sPressedCount = _dPressedCount = 0;
            _jumpCount = _sprintToggleCount = _slideToggleCount = _stopCount = 0;
            _wasWPressed = _wasAPressed = _wasSPressed = _wasDPressed = false;
            _wasSprinting = _wasSliding = _wasStopped = _wasSPressed_init = false;
            _goalScorers.Clear(); _assisters.Clear();
            LastTick = null;
        }

        public void AddTick(TelemetryTick tick)
        {
            if (_allTicks.Count > 0)
            {
                var prev = _allTicks[_allTicks.Count - 1];
                float dx = tick.Position.X - prev.Position.X;
                float dy = tick.Position.Y - prev.Position.Y;
                float dz = tick.Position.Z - prev.Position.Z;
                _distanceTraveled += Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            float speed = Mathf.Sqrt(
                tick.Velocity.X * tick.Velocity.X +
                tick.Velocity.Y * tick.Velocity.Y +
                tick.Velocity.Z * tick.Velocity.Z);

            _totalSpeed += speed; _sampleCount++;
            if (speed > _maxSpeed) _maxSpeed = speed;

            if (_lastDirection != Vector3.zero)
            {
                var dir = new Vector3(tick.Direction.X, tick.Direction.Y, tick.Direction.Z);
                if (dir.magnitude > 0.01f && Vector3.Angle(_lastDirection, dir) > DirectionChangeThreshold)
                    _directionChanges++;
                _lastDirection = dir;
            }
            else _lastDirection = new Vector3(tick.Direction.X, tick.Direction.Y, tick.Direction.Z);

            if (tick.Input.MoveInput.Y > 0.5f && !_wasWPressed) { _wPressedCount++; _wasWPressed = true; }
            if (tick.Input.MoveInput.Y <= 0.5f) _wasWPressed = false;
            if (tick.Input.MoveInput.X < -0.5f && !_wasAPressed) { _aPressedCount++; _wasAPressed = true; }
            if (tick.Input.MoveInput.X >= -0.5f) _wasAPressed = false;
            if (tick.Input.MoveInput.Y < -0.5f && !_wasSPressed) { _sPressedCount++; _wasSPressed = true; }
            if (tick.Input.MoveInput.Y >= -0.5f) _wasSPressed = false;
            if (tick.Input.MoveInput.X > 0.5f && !_wasDPressed) { _dPressedCount++; _wasDPressed = true; }
            if (tick.Input.MoveInput.X <= 0.5f) _wasDPressed = false;

            if (tick.Input.Jumping && !_wasSPressed_init) _jumpCount++;
            _wasSPressed_init = tick.Input.Jumping;
            if (tick.Input.Sprinting && !_wasSprinting) { _sprintToggleCount++; _wasSprinting = true; }
            if (!tick.Input.Sprinting) _wasSprinting = false;
            if (tick.Input.Sliding && !_wasSliding) { _slideToggleCount++; _wasSliding = true; }
            if (!tick.Input.Sliding) _wasSliding = false;
            if (tick.Input.Stopping && !_wasStopped) { _stopCount++; _wasStopped = true; }
            if (!tick.Input.Stopping) _wasStopped = false;

            _allTicks.Add(tick);
            LastTick = tick;
        }

        public void SetLocalPlayer(Player player) => LocalPlayer = player;

        public void RecordGameState(GameState state)
        {
            _currentPhase = state.Phase;
            _currentPeriod = state.Period;
            _blueScore = state.BlueScore;
            _redScore = state.RedScore;
        }

        public void RecordGoalEvent(Player player)
        {
            _goalScorers.Add(player.SteamId.Value.ToString());
        }

        public void RecordAssistEvent(Player player)
        {
            _assisters.Add(player.SteamId.Value.ToString());
        }

        public MatchInfo FinalizeMatch(string matchId, float startTime, float endTime)
        {
            float avgSpeed = _sampleCount > 0 ? _totalSpeed / _sampleCount : 0f;
            var match = new MatchInfo
            {
                MatchId = matchId,
                MatchLengthSeconds = endTime - startTime,
                BlueScore = _blueScore,
                RedScore = _redScore
            };

            var sm = ServerManager.Instance;
            if (sm != null)
            {
                match.ServerName = sm.Server.Value.Name.ToString();
            }

            // Always set SteamId — fall back to NetworkSender's cached value
            if (LocalPlayer != null)
            {
                match.SteamId = LocalPlayer.SteamId.Value.ToString();
                match.Username = LocalPlayer.Username.Value.ToString();
                match.Team = LocalPlayer.Team.ToString();
            }
            
            // If LocalPlayer is null, use the session cached SteamId from NetworkSender
            if (string.IsNullOrEmpty(match.SteamId))
            {
                match.SteamId = NetworkSender.GetSteamId();
                match.Username = match.SteamId.Length > 10 ? match.SteamId.Substring(0, 10) : match.SteamId;
            }
            match.DistanceTraveled = _distanceTraveled;
            match.AverageSpeed = avgSpeed;
            match.TopSpeed = _maxSpeed;
            match.Goals = _goalScorers.Count;
            match.Assists = _assisters.Count;
            match.PuckTouches = _puckTouches;
            match.PossessionTimeSeconds = _possessionTime;

            return match;
        }

        public MovementAnalytics ComputeMovementAnalytics()
        {
            if (_allTicks.Count == 0) return new MovementAnalytics();
            float avgSpeed = _totalSpeed / _sampleCount;

            var speeds = new List<float>();
            foreach (var t in _allTicks)
                speeds.Add(Mathf.Sqrt(t.Velocity.X * t.Velocity.X + t.Velocity.Y * t.Velocity.Y + t.Velocity.Z * t.Velocity.Z));
            speeds.Sort();

            float burstSpeed = speeds.Count >= 100 ? speeds[Mathf.FloorToInt(speeds.Count * 0.95f)] : avgSpeed;
            float minutes = (_allTicks.Count * 0.05f) / 60f;
            float agilityScore = minutes > 0 ? _directionChanges / minutes : 0;

            return new MovementAnalytics
            {
                DistanceTraveled = _distanceTraveled, AverageSpeed = avgSpeed,
                TopSpeed = _maxSpeed, CruiseSpeed = avgSpeed, BurstSpeed = burstSpeed,
                DirectionChangesPerMinute = agilityScore,
                JumpCount = _jumpCount, SprintToggleCount = _sprintToggleCount,
                SlideToggleCount = _slideToggleCount, StopCount = _stopCount
            };
        }

        public InputAnalytics ComputeInputAnalytics()
        {
            return new InputAnalytics
            {
                WPressedCount = _wPressedCount, APressedCount = _aPressedCount,
                SPressedCount = _sPressedCount, DPressedCount = _dPressedCount,
                TotalInputEvents = _wPressedCount + _aPressedCount + _sPressedCount + _dPressedCount + _jumpCount + _sprintToggleCount + _slideToggleCount + _stopCount,
                JumpCount = _jumpCount, SprintToggleCount = _sprintToggleCount,
                SlideToggleCount = _slideToggleCount, StopCount = _stopCount
            };
        }

        public List<TelemetryTick> GetAllTicks() => new(_allTicks);
        public int TickCount => _allTicks.Count;
    }
}
