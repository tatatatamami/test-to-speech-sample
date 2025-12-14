namespace TextToSpeech.Models;

public sealed class VoiceProfile
{
    public string VoiceName { get; init; } = string.Empty;

    public string? Style { get; init; }

    public double? StyleDegree { get; init; }

    public string Rate { get; init; } = "0%";

    public string Pitch { get; init; } = "0%";

    public string Volume { get; init; } = "0%";

    public string FileTag { get; init; } = "voice";

    public bool EnableStyle { get; init; } = true;

    public string? SpeakerProfileId { get; init; }
}