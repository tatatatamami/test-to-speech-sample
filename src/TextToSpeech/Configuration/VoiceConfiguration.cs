using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TextToSpeech.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TextToSpeech.Configuration;

public sealed class VoiceConfiguration
{
    public VoiceDefaults Defaults { get; set; } = new();

    public Dictionary<string, VoiceProfileConfig> Voices { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, string> Aliases { get; set; } = new(StringComparer.Ordinal);

    public VoiceProfile ResolveProfile(string speaker)
    {
        if (string.IsNullOrWhiteSpace(speaker))
        {
            throw new ArgumentException("Speaker name must be provided", nameof(speaker));
        }

        if (!Voices.TryGetValue(speaker, out var profileConfig) && Aliases.TryGetValue(speaker, out var alias))
        {
            Voices.TryGetValue(alias, out profileConfig);
        }

        if (profileConfig is null)
        {
            if (string.IsNullOrWhiteSpace(Defaults.FallbackVoice))
            {
                throw new KeyNotFoundException($"No voice mapping found for speaker '{speaker}' and no fallback voice configured.");
            }

            profileConfig = new VoiceProfileConfig
            {
                VoiceName = Defaults.FallbackVoice,
                Rate = Defaults.Rate,
                Pitch = Defaults.Pitch,
                Volume = Defaults.Volume,
                StyleDegree = Defaults.StyleDegree,
                EnableStyle = Defaults.EnableStyle,
            };
        }

        var fileTag = string.IsNullOrWhiteSpace(profileConfig.FileTag) ? "voice" : profileConfig.FileTag;
        var enableStyle = profileConfig.EnableStyle ?? Defaults.EnableStyle ?? true;

        return new VoiceProfile
        {
            VoiceName = profileConfig.VoiceName,
            Style = profileConfig.Style,
            StyleDegree = profileConfig.StyleDegree ?? Defaults.StyleDegree,
            Rate = profileConfig.Rate ?? Defaults.Rate ?? "0%",
            Pitch = profileConfig.Pitch ?? Defaults.Pitch ?? "0%",
            Volume = profileConfig.Volume ?? Defaults.Volume ?? "0%",
            FileTag = fileTag,
            EnableStyle = enableStyle,
            SpeakerProfileId = profileConfig.SpeakerProfileId,
        };
    }

    public int ResolvePause(int? pauseMs)
    {
        if (pauseMs.HasValue && pauseMs.Value > 0)
        {
            return pauseMs.Value;
        }

        return Defaults.PauseMs.HasValue && Defaults.PauseMs.Value > 0 ? Defaults.PauseMs.Value : 0;
    }

    public static VoiceConfiguration Load(FileInfo path)
    {
        if (!path.Exists)
        {
            throw new FileNotFoundException("Voice configuration file not found", path.FullName);
        }

        using var reader = new StreamReader(path.FullName, Encoding.UTF8);
        var yaml = reader.ReadToEnd();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var config = deserializer.Deserialize<VoiceConfiguration>(yaml) ?? new VoiceConfiguration();

        config.Defaults ??= new VoiceDefaults();
        config.Voices ??= new Dictionary<string, VoiceProfileConfig>(StringComparer.Ordinal);
        config.Aliases ??= new Dictionary<string, string>(StringComparer.Ordinal);

        return config;
    }
}

public sealed class VoiceDefaults
{
    [YamlMember(Alias = "output_format")]
    public string? OutputFormat { get; set; }

    [YamlMember(Alias = "pause_ms")]
    public int? PauseMs { get; set; }

    [YamlMember(Alias = "fallback_voice")]
    public string? FallbackVoice { get; set; }

    public string? Rate { get; set; }

    public string? Pitch { get; set; }

    public string? Volume { get; set; }

    [YamlMember(Alias = "style_degree")]
    public double? StyleDegree { get; set; }

    [YamlMember(Alias = "enable_style")]
    public bool? EnableStyle { get; set; }
}

public sealed class VoiceProfileConfig
{
    [YamlMember(Alias = "voice_name")]
    public string VoiceName { get; set; } = string.Empty;

    public string? Style { get; set; }

    [YamlMember(Alias = "style_degree")]
    public double? StyleDegree { get; set; }

    public string? Rate { get; set; }

    public string? Pitch { get; set; }

    public string? Volume { get; set; }

    [YamlMember(Alias = "file_tag")]
    public string? FileTag { get; set; }

    [YamlMember(Alias = "enable_style")]
    public bool? EnableStyle { get; set; }

    [YamlMember(Alias = "speaker_profile_id")]
    public string? SpeakerProfileId { get; set; }
}