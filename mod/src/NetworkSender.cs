using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PuckStats
{
    public static class NetworkSender
    {
        private static readonly Queue<QueuedRequest> _sendQueue = new();
        private static string ApiUrl => Plugin.Config.ApiEndpoint;
        private static bool _isSending;
        private static string _steamId;
        private static string _sessionId;

        public static void Initialize(string steamId)
        {
            _steamId = steamId;
            _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            Plugin.Log($"NetworkSender initialized for {steamId}");
        }

        public static string GetSteamId() => _steamId ?? "";

        public static void SendTelemetryBatch(string matchId, List<TelemetryTick> ticks)
        {
            if (string.IsNullOrEmpty(_steamId) || ticks == null || ticks.Count == 0) return;
            var batch = new TelemetryBatch { MatchId = matchId, SteamId = _steamId, SessionId = _sessionId, Ticks = ticks.ToArray() };
            EnqueueRequest("/api/match", JsonUtility.ToJson(batch));
        }

        public static void SendMatch(MatchInfo match)
        {
            if (string.IsNullOrEmpty(_steamId)) return;
            var json = JsonUtility.ToJson(match);
            Plugin.Log($"Sending match: {match.MatchId}, goals={match.Goals}, dist={match.DistanceTraveled:F0}m");
            EnqueueRequest("/api/match", json);
        }

        public static void SendHeartbeat(string matchId)
        {
            if (string.IsNullOrEmpty(_steamId)) return;
            var hb = new HeartbeatData { SteamId = _steamId, MatchId = matchId, Timestamp = Time.time };
            EnqueueRequest("/api/heartbeat", JsonUtility.ToJson(hb));
        }

        private static void EnqueueRequest(string endpoint, string json)
        {
            if (string.IsNullOrEmpty(ApiUrl)) return;
            var data = Encoding.UTF8.GetBytes(json);
            _sendQueue.Enqueue(new QueuedRequest { Url = ApiUrl + endpoint, Data = data });
            if (!_isSending)
            {
                var go = GameObject.Find("[PuckStats]");
                if (go != null)
                {
                    var mb = go.GetComponent<MonoBehaviour>();
                    if (mb != null) mb.StartCoroutine(ProcessQueue());
                }
            }
        }

        private static System.Collections.IEnumerator ProcessQueue()
        {
            _isSending = true;
            while (_sendQueue.Count > 0)
            {
                var req = _sendQueue.Dequeue();
                using var uwr = new UnityWebRequest(req.Url, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(req.Data),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("X-Steam-Id", _steamId);
                uwr.SetRequestHeader("X-Mod-Version", Plugin.MOD_VERSION);
                uwr.timeout = Plugin.Config.RequestTimeout;
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Plugin.Log($"Sent to {req.Url} — {uwr.downloadHandler.text}");
                }
                else
                {
                    Plugin.LogWarning($"Failed to send to {req.Url}: {uwr.error}");
                    if (Plugin.Config.RetryOnFailure && _sendQueue.Count < 50)
                        _sendQueue.Enqueue(req);
                }
            }
            _isSending = false;
        }

        public static void FlushAndDisconnect()
        {
            int retries = 20;
            while (_isSending && retries-- > 0)
                System.Threading.Thread.Sleep(100);
        }

        private class QueuedRequest
        {
            public string Url;
            public byte[] Data;
        }
    }
}
