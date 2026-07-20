using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PuckStats
{
    /// <summary>
    /// Parses .puckreplay files into structured replay data.
    /// Binary format: "PCKR" magic → version → header JSON → frame stream.
    /// Uses Unity's JsonUtility for JSON parsing.
    /// </summary>
    public class ReplayFileReader : IDisposable
    {
        private readonly Stream _stream;
        private ReplayHeader _header;
        private uint _frameCount;
        private long _frameStartPosition;
        private bool _disposed;

        public ReplayHeader Header => _header;

        private ReplayFileReader(Stream stream)
        {
            _stream = stream;
        }

        public static ReplayFileReader Open(string filePath)
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var parser = new ReplayFileReader(stream);
            parser.ParseHeader();
            return parser;
        }

        public static ReplayFileReader FromBytes(byte[] data)
        {
            var stream = new MemoryStream(data);
            var parser = new ReplayFileReader(stream);
            parser.ParseHeader();
            return parser;
        }

        private void ParseHeader()
        {
            var buf = new byte[4];

            // Magic
            _stream.Read(buf, 0, 4);
            if (Encoding.UTF8.GetString(buf) != "PCKR")
                throw new InvalidDataException("Invalid .puckreplay: wrong magic");

            // Version
            _stream.Read(buf, 0, 4);
            var version = BitConverter.ToUInt32(buf, 0);
            if (version > 2) throw new InvalidDataException($"Unsupported replay version: {version}");

            // Header JSON
            _stream.Read(buf, 0, 4);
            var headerLen = BitConverter.ToUInt32(buf, 0);
            var headerBytes = new byte[headerLen];
            _stream.Read(headerBytes, 0, (int)headerLen);
            _header = JsonUtility.FromJson<ReplayHeader>(Encoding.UTF8.GetString(headerBytes))
                       ?? new ReplayHeader { Version = version.ToString() };
            _header.Version = version.ToString();

            // Frame count
            _stream.Read(buf, 0, 4);
            _frameCount = BitConverter.ToUInt32(buf, 0);
            _frameStartPosition = _stream.Position;
        }

        public IEnumerable<ReplayFrame> ReadAllFrames()
        {
            _stream.Seek(_frameStartPosition, SeekOrigin.Begin);
            for (uint i = 0; i < _frameCount; i++)
            {
                if (_stream.Position >= _stream.Length) yield break;
                var tsBuf = new byte[4];
                if (_stream.Read(tsBuf, 0, 4) < 4) yield break;
                var timestamp = BitConverter.ToSingle(tsBuf, 0);
                var typeLen = _stream.ReadByte();
                var typeBuf = new byte[typeLen];
                _stream.Read(typeBuf, 0, typeLen);
                var typeName = Encoding.UTF8.GetString(typeBuf);
                var lenBuf = new byte[4];
                _stream.Read(lenBuf, 0, 4);
                var dataLen = BitConverter.ToUInt32(lenBuf, 0);
                var data = new byte[dataLen];
                _stream.Read(data, 0, (int)dataLen);

                yield return new ReplayFrame { FrameIndex = i, Timestamp = timestamp, EventType = typeName, RawData = data };
            }
        }

        public ParsedReplay ParseFullReplay()
        {
            var parsed = new ParsedReplay { Header = _header };
            foreach (var p in _header.Players)
                parsed.Players[p.ClientId] = new ParsedReplayPlayer { ClientId = p.ClientId, SteamId = p.SteamId, Username = p.Username, Team = p.Team, Role = p.Role, Number = p.Number };

            foreach (var frame in ReadAllFrames())
            {
                try { HandleFrame(frame, parsed); } catch { }
            }

            foreach (var (_, p) in parsed.Players)
                ComputePlayerStats(p);

            parsed.SkatingHeatmap = GenerateHeatmap(parsed, "Skating");
            return parsed;
        }

        private void HandleFrame(ReplayFrame frame, ParsedReplay parsed)
        {
            var json = Encoding.UTF8.GetString(frame.RawData);
            switch (frame.EventType)
            {
                case "PlayerBodySpawned":
                case "PlayerBodyMove":
                    try
                    {
                        var d = JsonUtility.FromJson<ReplayBodyMove>(json);
                        if (parsed.Players.TryGetValue(d.OwnerClientId, out var p))
                        {
                            p.BodyPositions.Add((frame.Timestamp, d.PosX, d.PosY, d.PosZ, d.VelX, d.VelY, d.VelZ));
                            float spd = Mathf.Sqrt(d.VelX * d.VelX + d.VelY * d.VelY + d.VelZ * d.VelZ);
                            if (spd > p.TopSpeed) p.TopSpeed = spd;
                        }
                    }
                    catch { }
                    break;
                case "PuckMove":
                    try
                    {
                        var d = JsonUtility.FromJson<ReplayPuckMove>(json);
                        parsed.PuckPositions.Add((frame.Timestamp, d.PosX, d.PosY, d.PosZ, d.VelX, d.VelY, d.VelZ));
                    }
                    catch { }
                    break;
                case "Goal":
                    parsed.Events.Add(new ParsedReplayEvent { Timestamp = frame.Timestamp, Type = "Goal", Data = json });
                    break;
            }
        }

        private void ComputePlayerStats(ParsedReplayPlayer player)
        {
            var pos = player.BodyPositions;
            float totalDist = 0;
            for (int i = 1; i < pos.Count; i++)
            {
                float dx = pos[i].PosX - pos[i - 1].PosX;
                float dy = pos[i].PosY - pos[i - 1].PosY;
                float dz = pos[i].PosZ - pos[i - 1].PosZ;
                totalDist += Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            player.TotalDistance = totalDist;

            // Detect shifts (gaps > 5s)
            player.Shifts.Clear();
            if (pos.Count > 1)
            {
                float shiftStart = pos[0].Time, shiftDist = 0;
                for (int i = 1; i < pos.Count; i++)
                {
                    float gap = pos[i].Time - pos[i - 1].Time;
                    if (gap > 5f && shiftDist > 1f)
                    {
                        player.Shifts.Add(new ShiftData { StartTime = shiftStart, EndTime = pos[i - 1].Time, DistanceTraveled = shiftDist, AverageSpeed = shiftDist / Math.Max(0.01f, pos[i - 1].Time - shiftStart) });
                        shiftStart = pos[i].Time; shiftDist = 0;
                    }
                    else if (i > 1)
                    {
                        float dx = pos[i].PosX - pos[i - 1].PosX, dy = pos[i].PosY - pos[i - 1].PosY, dz = pos[i].PosZ - pos[i - 1].PosZ;
                        shiftDist += Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
                    }
                }
                if (shiftDist > 1f)
                    player.Shifts.Add(new ShiftData { StartTime = shiftStart, EndTime = pos[pos.Count - 1].Time, DistanceTraveled = shiftDist, AverageSpeed = shiftDist / Math.Max(0.01f, pos[pos.Count - 1].Time - shiftStart) });
            }
        }

        public static HeatmapData GenerateHeatmap(ParsedReplay parsed, string type)
        {
            const int w = 100, h = 50;
            const float halfX = 40f, halfZ = 20f;
            var grid = new float[w, h];
            float max = 0;

            foreach (var (_, player) in parsed.Players)
                foreach (var bp in player.BodyPositions)
                {
                    int cx = (int)((bp.PosX + halfX) / (2 * halfX) * w);
                    int cy = (int)((bp.PosZ + halfZ) / (2 * halfZ) * h);
                    if (cx >= 0 && cx < w && cy >= 0 && cy < h) { grid[cx, cy]++; if (grid[cx, cy] > max) max = grid[cx, cy]; }
                }

            var cells = new List<HeatmapCell>();
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (grid[x, y] > 0) cells.Add(new HeatmapCell { X = x, Y = y, Intensity = grid[x, y] / Math.Max(1, max) });

            return new HeatmapData { Type = type, RinkWidth = w, RinkHeight = h, Cells = cells, MaxIntensity = max };
        }

        public void Dispose()
        {
            if (!_disposed) { _stream.Dispose(); _disposed = true; }
        }
    }

    // === REPLAY TYPES ===

    [Serializable]
    public class ReplayHeader
    {
        public string Version = "1.0";
        public string MatchId = "", ServerName = "", Map = "";
        public string StartTime = "";
        public float DurationSeconds;
        public int BlueScore, RedScore;
        public List<ReplayPlayerInfo> Players = new();
    }

    [Serializable]
    public class ReplayPlayerInfo
    {
        public ulong ClientId;
        public string SteamId = "", Username = "";
        public string Team = "", Role = "";
        public int Number;
    }

    public class ReplayFrame
    {
        public uint FrameIndex;
        public float Timestamp;
        public string EventType = "";
        public byte[] RawData = Array.Empty<byte>();
    }

    [Serializable]
    public class ReplayBodyMove
    {
        public ulong OwnerClientId;
        public float PosX, PosY, PosZ, VelX, VelY, VelZ;
    }

    [Serializable]
    public class ReplayPuckMove
    {
        public float PosX, PosY, PosZ, VelX, VelY, VelZ;
    }

    public class ParsedReplay
    {
        public ReplayHeader Header = new();
        public Dictionary<ulong, ParsedReplayPlayer> Players = new();
        public List<ParsedReplayEvent> Events = new();
        public List<(float Time, float PosX, float PosY, float PosZ, float VelX, float VelY, float VelZ)> PuckPositions = new();
        public HeatmapData SkatingHeatmap = new();
    }

    public class ParsedReplayPlayer
    {
        public ulong ClientId;
        public string SteamId = "", Username = "";
        public string Team = "", Role = "";
        public int Number;
        public List<(float Time, float PosX, float PosY, float PosZ, float VelX, float VelY, float VelZ)> BodyPositions = new();
        public float TotalDistance, TopSpeed;
        public List<ShiftData> Shifts = new();
    }

    public class ParsedReplayEvent
    {
        public float Timestamp;
        public string Type = "", Data = "";
    }
}
