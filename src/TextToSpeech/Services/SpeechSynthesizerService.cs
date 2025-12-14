using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TextToSpeech.Configuration;

namespace TextToSpeech.Services;

public sealed class SpeechSynthesizerService : IDisposable
{
    private readonly SpeechConfig _speechConfig;

    public SpeechSynthesizerService(VoiceDefaults defaults)
    {
        var key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
        var region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
        {
            throw new InvalidOperationException("AZURE_SPEECH_KEY and AZURE_SPEECH_REGION environment variables must be set.");
        }

        _speechConfig = SpeechConfig.FromSubscription(key, region);

        if (!string.IsNullOrWhiteSpace(defaults.OutputFormat))
        {
            if (!Enum.TryParse<SpeechSynthesisOutputFormat>(defaults.OutputFormat, ignoreCase: false, out var format))
            {
                throw new ArgumentException($"Unsupported speech output format '{defaults.OutputFormat}'.", nameof(defaults));
            }

            _speechConfig.SetSpeechSynthesisOutputFormat(format);
        }
    }

    public async Task<TimeSpan> SynthesizeAsync(string ssml, string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var audioConfig = AudioConfig.FromWavFileOutput(outputPath);
        using var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig);

        var result = await synthesizer.SpeakSsmlAsync(ssml).ConfigureAwait(false);
        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            var message = cancellation?.ErrorDetails;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = cancellation?.Reason.ToString();
            }

            throw new InvalidOperationException($"Speech synthesis canceled: {message ?? "Unknown reason"}.");
        }

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new InvalidOperationException($"Speech synthesis failed: {result.Reason}.");
        }

        return result.AudioDuration;
    }

    public void Dispose()
    {
        if (_speechConfig is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
