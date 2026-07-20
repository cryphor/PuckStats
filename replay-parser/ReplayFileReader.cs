using System.Text;
using System.Text.Json;
using PuckStats.Shared;

namespace PuckStats.ReplayParser;

public class ReplayFileReader : IDisposable
{
    private readonly Stream _stream;
    private bool _disposed;

    private ReplayFileReader(Stream stream) => _stream = stream;

    public static ReplayFileReader Open(string filePath) => new(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

    public ReplayHeader ParseHeader()
    {
        var buf = new byte[4];
        _stream.ReadExactly(buf, 0, 4);
        if (Encoding.UTF8.GetString(buf) != "PCKR") throw new InvalidDataException("Invalid .puckreplay");
        _stream.ReadExactly(buf, 0, 4);
        _stream.ReadExactly(buf, 0, 4);
        var hLen = BitConverter.ToUInt32(buf, 0);
        var hBytes = new byte[hLen];
        _stream.ReadExactly(hBytes, 0, (int)hLen);
        var header = JsonSerializer.Deserialize<ReplayHeader>(hBytes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _stream.ReadExactly(buf, 0, 4);
        return header ?? new ReplayHeader();
    }

    public IEnumerable<ReplayFrame> ReadAllFrames()
    {
        while (_stream.Position < _stream.Length)
        {
            var tsBuf = new byte[4];
            if (_stream.Read(tsBuf, 0, 4) < 4) yield break;
            var timestamp = BitConverter.ToSingle(tsBuf, 0);
            var typeLen = _stream.ReadByte();
            var typeBuf = new byte[typeLen];
            _stream.ReadExactly(typeBuf, 0, typeLen);
            var typeName = Encoding.UTF8.GetString(typeBuf);
            var lenBuf = new byte[4];
            _stream.ReadExactly(lenBuf, 0, 4);
            var dataLen = BitConverter.ToUInt32(lenBuf, 0);
            var data = new byte[dataLen];
            _stream.ReadExactly(data, 0, (int)dataLen);
            yield return new ReplayFrame { Timestamp = timestamp, EventType = typeName, RawData = data };
        }
    }

    public ParsedReplay ParseFullReplay()
    {
        var header = ParseHeader();
        var parsed = new ParsedReplay { Header = header };
        foreach (var frame in ReadAllFrames())
        {
            try
            {
                var json = Encoding.UTF8.GetString(frame.RawData);
                switch (frame.EventType)
                {
                    case "PlayerBodySpawned":
                    case "PlayerBodyMove":
                        parsed.Players.Add(new ParsedReplayPlayer { BodyPositions = new() });
                        break;
                    case "PuckMove":
                        break;
                    case "Goal":
                        parsed.Events.Add(new ParsedReplayEvent { Timestamp = frame.Timestamp, Type = "Goal", Data = json });
                        break;
                }
            }
            catch { }
        }
        return parsed;
    }

    public void Dispose() { if (!_disposed) { _stream.Dispose(); _disposed = true; } }
}

public class ReplayFrame
{
    public float Timestamp;
    public string EventType = "";
    public byte[] RawData = Array.Empty<byte>();
}

public class ParsedReplay
{
    public ReplayHeader Header { get; set; } = new();
    public List<ParsedReplayPlayer> Players { get; set; } = new();
    public List<ParsedReplayEvent> Events { get; set; } = new();
    public HeatmapData? SkatingHeatmap { get; set; }
    public HeatmapData? ShotHeatmap { get; set; }
    public HeatmapData? PossessionHeatmap { get; set; }
}

public class ParsedReplayPlayer
{
    public ulong ClientId { get; set; }
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public List<(float Time, float X, float Y, float Z)> BodyPositions { get; set; } = new();
}

public class ParsedReplayEvent
{
    public float Timestamp;
    public string Type = "";
    public string Data = "";
}
