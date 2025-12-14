using System;

namespace TextToSpeech.Models;

public sealed class RenderedLine
{
    public RenderedLine(DialogueLine line, VoiceProfile profile, string wavPath, int pauseMs, string ssml, TimeSpan duration)
    {
        Line = line;
        Profile = profile;
        WavPath = wavPath;
        PauseMs = pauseMs;
        Ssml = ssml;
        Duration = duration;
    }

    public DialogueLine Line { get; }

    public VoiceProfile Profile { get; }

    public string WavPath { get; }

    public int PauseMs { get; }

    public string Ssml { get; }

    public TimeSpan Duration { get; }
}
