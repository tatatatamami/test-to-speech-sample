using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using TextToSpeech.Configuration;
using TextToSpeech.Models;
using TextToSpeech.Services;

namespace TextToSpeech;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Load environment variables (Azure Speech key and endpoint)
            LoadDotEnv();

            // Parse CLI arguments and prepare the output directory
            var options = CliOptions.Parse(args);
            options.EnsureDirectories();

            // Load voice configuration and dialogue script
            var voiceConfiguration = VoiceConfiguration.Load(options.VoiceConfigPath);
            var dialogueLines = LoadScript(options.ScriptPath);
            if (dialogueLines.Count == 0)
            {
                Console.Error.WriteLine("Dialogue script is empty.");
                return 1;
            }

            // Synthesize each line and assemble the final track
            var ssmlBuilder = new SsmlBuilder();
            var audioMixer = new AudioMixer();
            var renderedLines = new List<RenderedLine>(dialogueLines.Count);

            // Create a per-scene working directory
            var perLineDirectory = Path.Combine(options.SegmentDirectory.FullName, options.SceneId);
            Directory.CreateDirectory(perLineDirectory);

            // Initialize the speech synthesis service
            using var synthesizer = new SpeechSynthesizerService(voiceConfiguration.Defaults);

            for (var index = 0; index < dialogueLines.Count; index++)
            {
                // Validate each line (speaker name and text)
                var line = dialogueLines[index];
                if (string.IsNullOrWhiteSpace(line.Speaker))
                {
                    throw new InvalidOperationException($"Line {index + 1} is missing a speaker name.");
                }

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    throw new InvalidOperationException($"Line {index + 1} is missing text content.");
                }

                // Build SSML and synthesize the line
                var profile = voiceConfiguration.ResolveProfile(line.Speaker);
                var ssml = ssmlBuilder.Build(line.Text!, profile, line.Emotion);

                // Synthesize audio and save the file
                var lineId = ResolveLineId(line.LineId, index + 1);
                var fileName = $"{lineId}_{profile.FileTag}.wav";
                var wavPath = Path.Combine(perLineDirectory, fileName);

                // Execute speech synthesis
                var duration = await synthesizer.SynthesizeAsync(ssml, wavPath).ConfigureAwait(false);
                var pauseMs = voiceConfiguration.ResolvePause(line.PauseMs);

                // Capture synthesis metadata
                renderedLines.Add(new RenderedLine(line, profile, wavPath, pauseMs, ssml, duration));
            }

            // Render the final track and append logs
            var logEntries = audioMixer.BuildFinalTrack(renderedLines, options.OutputPath.FullName, options.ExportFormat);
            AudioMixer.AppendLogEntries(options.GetLogFilePath(), logEntries);

            Console.WriteLine($"Synthesis complete. Output: {options.OutputPath.FullName}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void LoadDotEnv()
    {
        try
        {
            Env.TraversePath().Load();
        }
        catch (FileNotFoundException)
        {
            // Optional:
        }
    }

    private static List<DialogueLine> LoadScript(FileInfo scriptPath)
    {
        if (!scriptPath.Exists)
        {
            throw new FileNotFoundException("Dialogue script not found", scriptPath.FullName);
        }

        var raw = File.ReadAllText(scriptPath.FullName);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
        var lines = JsonSerializer.Deserialize<List<DialogueLine>>(raw, options) ?? new List<DialogueLine>();
        return lines;
    }

    private static string ResolveLineId(string? lineId, int index)
    {
        if (!string.IsNullOrWhiteSpace(lineId) && int.TryParse(lineId, out var parsed))
        {
            return parsed.ToString("D3");
        }

        if (!string.IsNullOrWhiteSpace(lineId))
        {
            return lineId;
        }

        return index.ToString("D3");
    }
}
