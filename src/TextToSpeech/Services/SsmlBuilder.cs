using System.Globalization;
using System.Xml.Linq;
using TextToSpeech.Models;

namespace TextToSpeech.Services;

public sealed class SsmlBuilder
{
    private static readonly XNamespace SpeakNs = "http://www.w3.org/2001/10/synthesis";
    private static readonly XNamespace MsttsNs = "http://www.w3.org/2001/mstts";

    public string Build(string text, VoiceProfile profile, string? emotion)
    {
        var speak = new XElement(SpeakNs + "speak",
            new XAttribute(XNamespace.Xml + "lang", "ja-JP"),
            new XAttribute("version", "1.0"));

        var voice = new XElement(SpeakNs + "voice",
            new XAttribute("name", profile.VoiceName));
        speak.Add(voice);

        var isPersonalVoice = !string.IsNullOrWhiteSpace(profile.SpeakerProfileId);
        var style = !string.IsNullOrWhiteSpace(emotion) ? emotion : profile.Style;
        var useStyle = !isPersonalVoice && profile.EnableStyle && !string.IsNullOrWhiteSpace(style);

        XElement container;

        if (isPersonalVoice)
        {
            speak.SetAttributeValue(XNamespace.Xmlns + "mstts", MsttsNs);

            container = new XElement(MsttsNs + "ttsembedding",
                new XAttribute("speakerProfileId", profile.SpeakerProfileId!));

            voice.Add(container);
        }
        else if (useStyle)
        {
            speak.SetAttributeValue(XNamespace.Xmlns + "mstts", MsttsNs);

            container = new XElement(MsttsNs + "express-as",
                new XAttribute("style", style!));

            if (profile.StyleDegree.HasValue)
            {
                container.Add(new XAttribute(
                    "styledegree",
                    profile.StyleDegree.Value.ToString("0.0", CultureInfo.InvariantCulture)));
            }

            voice.Add(container);
        }
        else
        {
            container = voice;
        }

        var prosody = new XElement(SpeakNs + "prosody", new XText(text));

        var rate = string.IsNullOrWhiteSpace(profile.Rate) ? "0%" : profile.Rate!;
        prosody.SetAttributeValue("rate", rate);

        if (!isPersonalVoice)
        {
            var pitch = string.IsNullOrWhiteSpace(profile.Pitch) ? "0%" : profile.Pitch!;
            var volume = string.IsNullOrWhiteSpace(profile.Volume) ? "0%" : profile.Volume!;

            prosody.SetAttributeValue("pitch", pitch);
            prosody.SetAttributeValue("volume", volume);
        }

        container.Add(prosody);

        var ssml = speak.ToString(SaveOptions.DisableFormatting);
        // Console.WriteLine(ssml);

        return ssml;
    }
}
