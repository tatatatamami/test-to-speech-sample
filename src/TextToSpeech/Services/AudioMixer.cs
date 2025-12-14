using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TextToSpeech.Models;

namespace TextToSpeech.Services;

public sealed class AudioMixer
{
    public IReadOnlyList<SynthesisLogEntry> BuildFinalTrack(IReadOnlyList<RenderedLine> renderedLines, string finalPath, string exportFormat)
    {
        if (renderedLines.Count == 0)
        {
            throw new InvalidOperationException("No rendered lines provided to AudioMixer.");
        }

        var tempMixPath = Path.Combine(
            Path.GetDirectoryName(finalPath) ?? Directory.GetCurrentDirectory(),
            Path.GetFileNameWithoutExtension(finalPath) + "__mix.wav");

        WaveConcatenator.CreateConcatenatedWave(renderedLines, tempMixPath);

        AudioExporter.Export(tempMixPath, finalPath, exportFormat);
        File.Delete(tempMixPath);

        var entries = new List<SynthesisLogEntry>(renderedLines.Count);
        foreach (var rendered in renderedLines)
        {
            entries.Add(new SynthesisLogEntry
            {
                LineId = rendered.Line.LineId,
                Speaker = rendered.Line.Speaker,
                Emotion = rendered.Line.Emotion,
                VoiceName = rendered.Profile.VoiceName,
                Style = rendered.Line.Emotion ?? rendered.Profile.Style,
                Rate = rendered.Profile.Rate,
                Pitch = rendered.Profile.Pitch,
                Volume = rendered.Profile.Volume,
                DurationMs = (int)Math.Round(rendered.Duration.TotalMilliseconds, MidpointRounding.AwayFromZero),
                PauseMs = rendered.PauseMs,
                WavPath = rendered.WavPath.Replace('\\', '/'),
            });
        }

        return entries;
    }

    public static void AppendLogEntries(string logFilePath, IReadOnlyList<SynthesisLogEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? Directory.GetCurrentDirectory());
        using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);
        foreach (var entry in entries)
        {
            var payload = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = false });
            writer.WriteLine(payload);
        }
    }
}
