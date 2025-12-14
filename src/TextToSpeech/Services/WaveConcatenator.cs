using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using TextToSpeech.Models;

namespace TextToSpeech.Services;

internal static class WaveConcatenator
{
    public static void CreateConcatenatedWave(IReadOnlyList<RenderedLine> renderedLines, string targetPath)
    {
        using var firstReader = new WaveFileReader(renderedLines[0].WavPath);
        var format = firstReader.WaveFormat;

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? Directory.GetCurrentDirectory());
        using var writer = new WaveFileWriter(targetPath, format);

        AppendAudio(writer, firstReader, renderedLines[0].PauseMs);

        for (var i = 1; i < renderedLines.Count; i++)
        {
            using var reader = new WaveFileReader(renderedLines[i].WavPath);
            if (!reader.WaveFormat.Equals(format))
            {
                throw new InvalidOperationException("All WAV files must share the same format when concatenating.");
            }

            AppendAudio(writer, reader, renderedLines[i].PauseMs);
        }
    }

    private static void AppendAudio(WaveFileWriter writer, WaveFileReader reader, int pauseMs)
    {
        reader.CopyTo(writer);
        if (pauseMs <= 0)
        {
            return;
        }

        var bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000.0;
        var bytesToWrite = (int)Math.Round(bytesPerMillisecond * pauseMs, MidpointRounding.AwayFromZero);
        var blockAlign = Math.Max(1, reader.WaveFormat.BlockAlign);
        bytesToWrite -= bytesToWrite % blockAlign;
        if (bytesToWrite > 0)
        {
            writer.Write(new byte[bytesToWrite], 0, bytesToWrite);
        }
    }
}
