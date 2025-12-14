using System;
using System.IO;
using NAudio.MediaFoundation;
using NAudio.Wave;

namespace TextToSpeech.Services;

internal static class AudioExporter
{
    public static void Export(string sourceWavePath, string destinationPath, string exportFormat)
    {
        if (!File.Exists(sourceWavePath))
        {
            throw new FileNotFoundException("Source WAV file not found for export", sourceWavePath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? Directory.GetCurrentDirectory());

        switch (exportFormat.Trim().ToLowerInvariant())
        {
            case "wav":
                File.Copy(sourceWavePath, destinationPath, overwrite: true);
                break;
            case "mp3":
                MediaFoundationApi.Startup();
                try
                {
                    using var reader = new AudioFileReader(sourceWavePath);
                    MediaFoundationEncoder.EncodeToMp3(reader, destinationPath);
                }
                finally
                {
                    MediaFoundationApi.Shutdown();
                }
                break;
            default:
                throw new NotSupportedException($"Export format '{exportFormat}' is not supported. Use wav or mp3.");
        }
    }
}
