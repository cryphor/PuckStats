using System.Collections.Generic;
using UnityEngine;

namespace PuckStats
{
    /// <summary>
    /// Client-side behaviour that drives the PuckStats data collection and UI.
    /// Prints in-game chat messages so players see the mod working.
    /// </summary>
    public class PuckStatsBehaviour : MonoBehaviour
    {
        private float _telemetryInterval = 1f / 20f;
        private float _telemetryTimer;
        private float _uiUpdateTimer;
        private bool _initialized;
        private bool _inMatch;
        private string _currentMatchId;
        private float _matchStartTime;
        private TelemetryCollector _collector = new();
        private List<TelemetryTick> _tickBuffer = new(300);
        private System.Diagnostics.Stopwatch _sessionTimer = new();
        private int _lastKnownGoals;
        private int _lastKnownAssists;
        private bool _welcomeShown;

        private static readonly Color AccentColor = new Color(0.26f, 1f, 0.56f);

        void Start()
        {
            StartCoroutine(InitDelayed());
            _sessionTimer.Start();
        }

        private System.Collections.IEnumerator InitDelayed()
        {
            yield return null;
            yield return null;
            yield return null;
            try
            {
                PuckStatsUI.Initialize();
                SubscribeToEvents();
                _initialized = true;
                Plugin.Log("PuckStats client initialized");
            }
            catch (System.Exception e) { Plugin.LogError($"Init failed: {e}"); }
        }

        void Update()
        {
            if (!_initialized) return;

            // Show welcome message once
            if (!_welcomeShown && UIManager.Instance?.UIDocument != null)
            {
                _welcomeShown = true;
                LogMsg("[PuckStats] <color=#42ff8f>v" + Plugin.MOD_VERSION + "</color> loaded. Tracking your gameplay.");
                LogMsg("[PuckStats] Press <color=#42ff8f>Ctrl+P</color> to toggle the overlay.");
                LogMsg("[PuckStats] Data upload: <color=#42ff8f>" + (Plugin.Config.AutoUpload ? "ON" : "OFF") + "</color>");
            }

            _uiUpdateTimer += Time.deltaTime;
            _telemetryTimer += Time.deltaTime;

            if (_uiUpdateTimer >= 0.1f)
            {
                _uiUpdateTimer = 0f;
                PuckStatsUI.Tick();
                UpdateMatchState();
                TrackPersonalStats();
            }

            if (_inMatch && _telemetryTimer >= _telemetryInterval)
            {
                _telemetryTimer = 0f;
                CollectTelemetry();
                FlushBufferIfNeeded();
            }
        }

        /// <summary>
        /// Log a message to the game console (visible in Developer Console / output_log.txt).
        /// </summary>
        private void LogMsg(string msg) => UnityEngine.Debug.Log(msg);

        private void SubscribeToEvents()
        {
            try
            {
                EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", OnGameStateChanged);
                EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", OnPlayerSpawned);
                EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", OnPlayerBodySpawned);
                EventManager.AddEventListener("Event_Everyone_OnPlayerGoalsChanged", OnPlayerGoalsChanged);
                EventManager.AddEventListener("Event_Everyone_OnPlayerAssistsChanged", OnPlayerAssistsChanged);
            }
            catch (System.Exception e) { Plugin.LogWarning($"Event subscribe failed: {e}"); }
        }

        /// <summary>
        /// Track personal goal/assist changes to fire chat notifications.
        /// </summary>
        private void TrackPersonalStats()
        {
            try
            {
                var local = GetLocalPlayer();
                if (local == null) return;

                int goals = local.Goals.Value;
                int assists = local.Assists.Value;

                if (goals > _lastKnownGoals)
                {
                    int scored = goals - _lastKnownGoals;
                    _collector.RecordGoalEvent(local);
                    LogMsg($"[PuckStats] <color=#42ff8f>GOAL!</color> You scored!  <color=#42ff8f>(+{scored})</color>");
                    
                    var gs = GameManager.Instance?.GameState.Value;
                    if (gs.HasValue)
                        LogMsg($"[PuckStats] Score: <color=#3b82f6>{gs.Value.BlueScore}</color> - <color=#d13333>{gs.Value.RedScore}</color>");
                }

                if (assists > _lastKnownAssists)
                {
                    int assisted = assists - _lastKnownAssists;
                    LogMsg($"[PuckStats] <color=#fbbf24>ASSIST!</color> You helped set up a goal!  <color=#fbbf24>(+{assisted})</color>");
                }

                _lastKnownGoals = goals;
                _lastKnownAssists = assists;
            }
            catch { }
        }

        private void UpdateMatchState()
        {
            try
            {
                bool wasInMatch = _inMatch;
                _inMatch = GlobalStateManager.UIState.Phase == UIPhase.Playing;

                if (_inMatch && !wasInMatch)
                {
                    // Match started
                    _currentMatchId = System.Guid.NewGuid().ToString("N").Substring(0, 12);
                    _matchStartTime = Time.time;
                    _collector.Reset();
                    _tickBuffer.Clear();
                    _lastKnownGoals = 0;
                    _lastKnownAssists = 0;
                    PuckStatsUI.ShowInMatch();
                    Plugin.Log($"Match started: {_currentMatchId}");
                    LogMsg("[PuckStats] Match tracking started.");
                }
                else if (!_inMatch && wasInMatch)
                {
                    // Match ended — show stats summary
                    FlushBuffer();
                    var matchData = _collector.FinalizeMatch(
                        _currentMatchId, _matchStartTime, Time.time);
                    NetworkSender.SendMatch(matchData);
                    PuckStatsUI.ShowOutOfMatch();

                    float matchLen = Time.time - _matchStartTime;
                    var movement = _collector.ComputeMovementAnalytics();
                    var input = _collector.ComputeInputAnalytics();

                    LogMsg($"[PuckStats] <color=#42ff8f>Match over!</color> Duration: {matchLen / 60:F0}m {matchLen % 60:F0}s");
                    LogMsg($"[PuckStats] Distance: <color=#42ff8f>{movement.DistanceTraveled:F1}m</color>  |  Top speed: <color=#42ff8f>{movement.TopSpeed:F1}m/s</color>");
                    LogMsg($"[PuckStats] Goals: <color=#42ff8f>{matchData.Goals}</color>  |  Input actions: <color=#42ff8f>{input.TotalInputEvents}</color>  |  Ticks: <color=#42ff8f>{_collector.TickCount}</color>");

                    if (Plugin.Config.AutoUpload)
                        LogMsg("[PuckStats] <color=#42ff8f>Data uploaded.</color> View your stats at https://puck-stats-praise.vercel.app");
                    else
                        LogMsg("[PuckStats] Data not uploaded (auto-upload is OFF).");
                }

                if (_inMatch)
                    PuckStatsUI.UpdateMatchTime(Time.time - _matchStartTime);
            }
            catch (System.Exception) { }
        }

        private void CollectTelemetry()
        {
            try
            {
                var local = GetLocalPlayer();
                if (local == null || local.PlayerBody == null) return;

                var body = local.PlayerBody;
                var input = local.PlayerInput;

                var tick = new TelemetryTick
                {
                    Timestamp = Time.time - _matchStartTime,
                    TickNumber = (int)((Time.time - _matchStartTime) / _telemetryInterval),
                    Position = new Vec3 { X = body.transform.position.x, Y = body.transform.position.y, Z = body.transform.position.z },
                    Velocity = new Vec3 { X = body.Rigidbody.linearVelocity.x, Y = body.Rigidbody.linearVelocity.y, Z = body.Rigidbody.linearVelocity.z },
                    Direction = new Vec3 { X = body.transform.forward.x, Y = body.transform.forward.y, Z = body.transform.forward.z }
                };

                if (input != null)
                {
                    tick.Input = new InputSnapshot
                    {
                        MoveInput = new Vec2 { X = input.MoveInput.ServerValue.x, Y = input.MoveInput.ServerValue.y },
                        LookAngleInput = new Vec2 { X = input.LookAngleInput.ServerValue.x, Y = input.LookAngleInput.ServerValue.y },
                        StickRaycastAngleInput = new Vec2 { X = input.StickRaycastOriginAngleInput.ServerValue.x, Y = input.StickRaycastOriginAngleInput.ServerValue.y },
                        BladeAngle = input.BladeAngleInput.ServerValue,
                        Sprinting = input.SprintInput.ServerValue,
                        Sliding = input.SlideInput.ServerValue,
                        Jumping = input.JumpInput.ServerValue > 0,
                        Stopping = input.StopInput.ServerValue,
                        Tracking = input.TrackInput.ServerValue
                    };
                }

                tick.MoveState = new MovementState
                {
                    IsMoving = body.Rigidbody.linearVelocity.magnitude > 0.5f,
                    IsSprinting = input?.SprintInput.ServerValue ?? false,
                    IsSliding = input?.SlideInput.ServerValue ?? false,
                    IsStopped = body.Rigidbody.linearVelocity.magnitude < 0.1f,
                    HasFallen = body.HasFallen.Value
                };

                if (_collector.LastTick != null)
                {
                    float dt = tick.Timestamp - _collector.LastTick.Timestamp;
                    if (dt > 0.001f)
                    {
                        tick.Acceleration = new Vec3
                        {
                            X = (tick.Velocity.X - _collector.LastTick.Velocity.X) / dt,
                            Y = (tick.Velocity.Y - _collector.LastTick.Velocity.Y) / dt,
                            Z = (tick.Velocity.Z - _collector.LastTick.Velocity.Z) / dt
                        };
                    }
                }

                _collector.AddTick(tick);
                _tickBuffer.Add(tick);
            }
            catch { }
        }

        private Player GetLocalPlayer()
        {
            try { return PlayerManager.Instance?.GetLocalPlayer(); }
            catch { return null; }
        }

        private void FlushBufferIfNeeded()
        {
            if (_tickBuffer.Count >= 300)
                FlushBuffer();
        }

        private void FlushBuffer()
        {
            if (_tickBuffer.Count == 0) return;
            var copy = new List<TelemetryTick>(_tickBuffer);
            _tickBuffer.Clear();
            NetworkSender.SendTelemetryBatch(_currentMatchId, copy);
        }

        private void OnGameStateChanged(Dictionary<string, object> message)
        {
            try
            {
                var newState = (GameState)message["newGameState"];
                _collector.RecordGameState(newState);
            }
            catch { }
        }

        private void OnPlayerSpawned(Dictionary<string, object> message)
        {
            try
            {
                var player = (Player)message["player"];
                if (player.IsLocalPlayer)
                {
                    _collector.SetLocalPlayer(player);
                }
            }
            catch { }
        }

        private void OnPlayerBodySpawned(Dictionary<string, object> message)
        {
            try
            {
                var body = (PlayerBody)message["playerBody"];
                if (body.Player?.IsLocalPlayer == true)
                    _collector.LocalBody = body;
            }
            catch { }
        }

        private void OnPlayerGoalsChanged(Dictionary<string, object> message)
        {
            try
            {
                var player = (Player)message["player"];
                if (player.IsLocalPlayer)
                    _collector.RecordGoalEvent(player);
            }
            catch { }
        }

        private void OnPlayerAssistsChanged(Dictionary<string, object> message)
        {
            try { /* tracked via TrackPersonalStats polling */ }
            catch { }
        }

        void OnDestroy()
        {
            try
            {
                EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", OnGameStateChanged);
                EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", OnPlayerSpawned);
                EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", OnPlayerBodySpawned);
                EventManager.RemoveEventListener("Event_Everyone_OnPlayerGoalsChanged", OnPlayerGoalsChanged);
                EventManager.RemoveEventListener("Event_Everyone_OnPlayerAssistsChanged", OnPlayerAssistsChanged);
            }
            catch { }
            FlushBuffer();
        }
    }
}
