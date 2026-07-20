using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace PuckStats
{
    /// <summary>
    /// PuckStats mod entry point. Collects telemetry data and sends to analytics backend.
    /// Implements IPuckPlugin for the Puck mod loading framework.
    /// </summary>
    public class Plugin : IPuckPlugin
    {
        public const string MOD_ID = "com.puckstats.mod";
        public const string MOD_VERSION = "1.0.0";
        private static readonly Harmony harmony = new Harmony(MOD_ID);
        private static GameObject _gameObject;
        internal static ModConfig Config;

        public bool OnEnable()
        {
            try
            {
                Config = ModConfig.Load();

                if (!IsDedicatedServer())
                {
                    harmony.PatchAll();
                    _gameObject = new GameObject("[PuckStats]");
                    Object.DontDestroyOnLoad(_gameObject);
                    _gameObject.AddComponent<PuckStatsBehaviour>();
                    Log($"v{MOD_VERSION} enabled");
                }
                else
                {
                    Log($"v{MOD_VERSION} running on dedicated server — data collection only");
                    harmony.PatchAll();
                    _gameObject = new GameObject("[PuckStats-Server]");
                    Object.DontDestroyOnLoad(_gameObject);
                    _gameObject.AddComponent<ServerTelemetryBehaviour>();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"OnEnable failed: {ex}");
                return false;
            }
        }

        public bool OnDisable()
        {
            try
            {
                NetworkSender.FlushAndDisconnect();
                if (_gameObject != null) Object.Destroy(_gameObject);
                harmony?.UnpatchSelf();
                PuckStatsUI.Destroy();
                return true;
            }
            catch { return false; }
        }

        public static bool IsDedicatedServer()
            => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        public static void Log(string m) => Debug.Log($"[PuckStats] {m}");
        public static void LogError(string m) => Debug.LogError($"[PuckStats] {m}");
        public static void LogWarning(string m) => Debug.LogWarning($"[PuckStats] {m}");
    }
}
