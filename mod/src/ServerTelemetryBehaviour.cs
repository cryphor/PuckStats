using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PuckStats
{
    /// <summary>
    /// Server-side telemetry behaviour for dedicated servers.
    /// Collects server-authoritative data about all players.
    /// </summary>
    public class ServerTelemetryBehaviour : MonoBehaviour
    {
        private float _tickTimer;
        private float _tickInterval = 0.5f; // Server collects at 2Hz for all players
        private bool _isTracking;

        void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Destroy(this);
                return;
            }
            StartCoroutine(InitDelayed());
        }

        private System.Collections.IEnumerator InitDelayed()
        {
            yield return null;
            yield return null;
            try
            {
                EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", OnGameStateChanged);
                Plugin.Log("Server telemetry initialized");
            }
            catch (System.Exception e) { Plugin.LogError($"Server init: {e}"); }
        }

        void Update()
        {
            if (!_isTracking || !NetworkManager.Singleton.IsServer) return;

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer = 0f;
                CollectServerTelemetry();
            }
        }

        private void CollectServerTelemetry()
        {
            try
            {
                var players = PlayerManager.Instance?.GetPlayers();
                if (players == null) return;

                foreach (var player in players)
                {
                    if (player == null || player.PlayerBody == null) continue;

                    // Collect aggregate telemetry per player (server-side)
                    // This is lighter weight - we don't need per-tick data server-side
                    var body = player.PlayerBody;
                    float speed = body.Rigidbody?.linearVelocity.magnitude ?? 0f;

                    // Could batch and relay to analytics backend from server
                }
            }
            catch { }
        }

        private void OnGameStateChanged(Dictionary<string, object> message)
        {
            try
            {
                var newState = (GameState)message["newGameState"];
                _isTracking = newState.Phase == GamePhase.Play;
            }
            catch { }
        }

        void OnDestroy()
        {
            try
            {
                EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", OnGameStateChanged);
            }
            catch { }
        }
    }
}
