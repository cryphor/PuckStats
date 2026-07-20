using System.IO;
using UnityEngine;

namespace PuckStats
{
    /// <summary>
    /// Mod configuration persisted to disk. 
    /// Uses JsonUtility for compatibility with Unity's runtime.
    /// </summary>
    [System.Serializable]
    public class ModConfig
    {
        public bool Enabled = true;
        public bool AutoUpload = true;
        public bool ShowOverlay = true;
        public string ApiEndpoint = "https://puckstats-api.onrender.com";
        public int RequestTimeout = 10;
        public int TelemetryRateHz = 20;
        public bool RetryOnFailure = true;
        public bool CollectReplays = true;
        public string ReplayUploadPath = "";
        public bool AnonymousMode = false;

        private static string ConfigPath =>
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PuckStats", "config.json");

        public static ModConfig Load()
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonUtility.FromJson<ModConfig>(json);
                    return cfg ?? new ModConfig();
                }
                catch { /* corrupt -> defaults */ }
            }

            var defaults = new ModConfig();
            defaults.Save();
            return defaults;
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(ConfigPath, JsonUtility.ToJson(this, true));
            }
            catch (System.Exception e)
            {
                Plugin.LogError($"Failed to save config: {e.Message}");
            }
        }
    }
}
