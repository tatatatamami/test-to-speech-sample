using System.Text.Json.Serialization;

namespace TextToSpeech.Models;

public sealed class DialogueLine
{
    [JsonPropertyName("lineId")]
    public string? LineId { get; set; }

    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    [JsonPropertyName("emotion")]
    public string? Emotion { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("pauseMs")]
    public int? PauseMs { get; set; }
}
