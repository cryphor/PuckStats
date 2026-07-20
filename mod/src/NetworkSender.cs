using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace PuckStats
{
    public static class NetworkSender
    {
        private static readonly Queue<byte[]> _sendQueue = new();
        private static string ApiUrl => Plugin.Config.ApiEndpoint;
        private static bool _isSending;
        private static string _steamId;
        private static string _sessionId;

        public static void Initialize(string steamId)
        {
            _steamId = steamId;
            _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public static void SendTelemetryBatch(string matchId, List<TelemetryTick> ticks)
        {
            if (string.IsNullOrEmpty(_steamId)) return;
            var batch = new TelemetryBatch { MatchId = matchId, SteamId = _steamId, SessionId = _sessionId, Ticks = ticks.ToArray() };
            EnqueueRequest("/api/telemetry", JsonUtility.ToJson(batch));
        }

        public static void SendMatch(MatchInfo match)
        {
            if (string.IsNullOrEmpty(_steamId)) return;
            EnqueueRequest("/api/match", JsonUtility.ToJson(match));
        }

        public static void SendHeartbeat(string matchId)
        {
            var hb = new HeartbeatData { SteamId = _steamId, MatchId = matchId, Timestamp = Time.time };
            EnqueueRequest("/api/heartbeat", JsonUtility.ToJson(hb));
        }

        private static void EnqueueRequest(string endpoint, string json)
        {
            if (string.IsNullOrEmpty(ApiUrl)) return;
            _sendQueue.Enqueue(Encoding.UTF8.GetBytes(json));
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
                var data = _sendQueue.Dequeue();
                using var uwr = new UnityWebRequest(ApiUrl + "/api/telemetry", "POST")
                {
                    uploadHandler = new UploadHandlerRaw(data),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("X-Steam-Id", _steamId);
                uwr.SetRequestHeader("X-Mod-Version", Plugin.MOD_VERSION);
                uwr.timeout = Plugin.Config.RequestTimeout;
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Plugin.LogWarning($"Network send failed: {uwr.error}");
                    if (Plugin.Config.RetryOnFailure && _sendQueue.Count < 50)
                        _sendQueue.Enqueue(data);
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
    }
}
