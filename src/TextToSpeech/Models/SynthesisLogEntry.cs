using System.Text.Json.Serialization;

namespace TextToSpeech.Models;

public sealed class SynthesisLogEntry
{
    [JsonPropertyName("lineId")]
    public string? LineId { get; set; }

    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    [JsonPropertyName("emotion")]
    public string? Emotion { get; set; }

    [JsonPropertyName("voice_name")]
    public string VoiceName { get; set; } = string.Empty;

    [JsonPropertyName("style")]
    public string? Style { get; set; }

    [JsonPropertyName("rate")]
    public string Rate { get; set; } = "0%";

    [JsonPropertyName("pitch")]
    public string Pitch { get; set; } = "0%";

    [JsonPropertyName("volume")]
    public string Volume { get; set; } = "0%";

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("pause_ms")]
    public int PauseMs { get; set; }

    [JsonPropertyName("wav_path")]
    public string WavPath { get; set; } = string.Empty;
}
