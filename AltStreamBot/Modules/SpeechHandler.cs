using System.Collections.ObjectModel;
using System.Speech.Synthesis;

namespace AltStreamBot.Modules;

public class SpeechHandler
{
    public bool TTS;
    public bool SayName;
    public bool OneVoice;
    public bool UsePronoun;

    private SpeechSynthesizer _synth;
    private Dictionary<string, string> _userVoices;
    private string _setVoiceMale = "Microsoft David Desktop";
    private string _setVoiceFemale = "Microsoft Zira Desktop";
    private string _setVoiceEnby = "Microsoft Zira Desktop";

    public SpeechHandler()
    {
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();
        _userVoices = new Dictionary<string, string>();
    }

    public async void Synthesize(string username, string input, VoiceGender gender)
    {
        if (!TTS) return;

        string voice = string.Empty;
        if (!OneVoice)
        {
            if (!_userVoices.ContainsKey(username.ToLower()))
            {
                Random rng = new Random(Guid.NewGuid().GetHashCode());
                ReadOnlyCollection<InstalledVoice>? voices = UsePronoun
                    ? _synth.GetInstalledVoices().Where(x => x.VoiceInfo.Gender.Equals(gender)) as ReadOnlyCollection<InstalledVoice>
                    : _synth.GetInstalledVoices();
                if (voices is null || voices.Count < 1) voices = _synth.GetInstalledVoices();

                _userVoices.Add(username.ToLower(), voices[rng.Next(voices.Count - 1)].VoiceInfo.Name);
            }

            voice = _userVoices[username.ToLower()];
        }
        else
            voice = gender switch
            {
                VoiceGender.Male => _setVoiceMale,
                VoiceGender.Female => _setVoiceFemale,
                _ => _setVoiceEnby
            };

        _synth.SelectVoice(voice);

        string output = SayName ? $"{username} said {input}" : input;

        SynthAudioPlayer(output);
    }

    public async void SystemMessage(string input)
    {
        _synth.SelectVoice("Microsoft David Desktop");
        SynthAudioPlayer(input);
    }

    public bool SelectVoice(string voiceName, VoiceGender gender)
    {
        var voices = _synth.GetInstalledVoices();

        InstalledVoice? voice = voices.FirstOrDefault(x => x.VoiceInfo.Name.Contains(voiceName, StringComparison.OrdinalIgnoreCase));
        if (voice == null)
            return false;

        switch (gender)
        {
            case VoiceGender.Female:
                _setVoiceFemale = voice.VoiceInfo.Name;
                break;
            case VoiceGender.Male:
                _setVoiceMale = voice.VoiceInfo.Name;
                break;
            default:
                _setVoiceEnby = voice.VoiceInfo.Name;
                break;
        }

        return true;
    }

    public void RerollUserVoice(string username)
    {
        if (!_userVoices.ContainsKey(username.ToLower()))
        {
            Console.WriteLine("Error: Could not find user voice");
            return;
        }
        _userVoices.Remove(username.ToLower());
    }

    public ReadOnlyCollection<InstalledVoice> GetAllVoicesInstalled() =>
        _synth.GetInstalledVoices();

    public void StopSynth()
    {
        _synth.Pause();
        _synth.Dispose();
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();
    }

    private void SynthAudioPlayer(string message)
    {
        var msg = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                        <voice name='" + _synth.Voice.Name + @"'>
                            " + message + @"
                        </voice>
                    </speak>";
        _synth.SpeakSsmlAsync(msg);
    }
}