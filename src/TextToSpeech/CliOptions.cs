using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextToSpeech;

public sealed class CliOptions
{
    private CliOptions(
        FileInfo scriptPath,
        FileInfo voiceConfigPath,
        FileInfo outputPath,
        DirectoryInfo logDirectory,
        string sceneId)
    {
        ScriptPath = scriptPath;
        VoiceConfigPath = voiceConfigPath;
        OutputPath = outputPath;
        LogDirectory = logDirectory;
        SceneId = sceneId;
    }

    public FileInfo ScriptPath { get; }

    public FileInfo VoiceConfigPath { get; }

    public FileInfo OutputPath { get; }

    public DirectoryInfo LogDirectory { get; }

    public string SceneId { get; }

    public DirectoryInfo SegmentDirectory => OutputPath.Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory());

    public string ExportFormat
    {
        get
        {
            var extension = OutputPath.Extension;
            if (!string.IsNullOrWhiteSpace(extension))
            {
                return extension.TrimStart('.').ToLowerInvariant();
            }

            var envValue = Environment.GetEnvironmentVariable("OUTPUT_AUDIO_FORMAT");
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.Trim().ToLowerInvariant();
            }

            return "mp3";
        }
    }

    public string GetLogFilePath() => Path.Combine(LogDirectory.FullName, $"synth_{SceneId}.jsonl");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(SegmentDirectory.FullName);
        Directory.CreateDirectory(LogDirectory.FullName);
    }

    public static CliOptions Parse(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected argument '{token}'. Expected --key value pairs.");
            }

            var key = token[2..];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Argument key must not be empty.");
            }

            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for argument '{token}'.");
            }

            var value = args[++i];
            map[key] = value;
        }

        var scriptPath = ResolveFile(map, "script", "data/script_scene1.json");
        var voiceConfigPath = ResolveFile(map, "voices", "config/voices.yaml");
        var outputPath = ResolveFile(map, "out", "output/scene1/scene1_final.mp3");
        var logDirectory = ResolveDirectory(map, "log-dir", "logs");

        var sceneId = map.TryGetValue("scene-id", out var providedSceneId) && !string.IsNullOrWhiteSpace(providedSceneId)
            ? providedSceneId
            : Path.GetFileNameWithoutExtension(scriptPath.Name);

        return new CliOptions(scriptPath, voiceConfigPath, outputPath, logDirectory, sceneId);
    }

    private static FileInfo ResolveFile(Dictionary<string, string> map, string key, string defaultRelativePath)
    {
        string rawValue;
        if (!map.TryGetValue(key, out rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            rawValue = defaultRelativePath;
        }

        var fullPath = Path.GetFullPath(rawValue);
        return new FileInfo(fullPath);
    }

    private static DirectoryInfo ResolveDirectory(Dictionary<string, string> map, string key, string defaultRelativePath)
    {
        string rawValue;
        if (!map.TryGetValue(key, out rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            rawValue = defaultRelativePath;
        }

        var fullPath = Path.GetFullPath(rawValue);
        return new DirectoryInfo(fullPath);
    }
}
