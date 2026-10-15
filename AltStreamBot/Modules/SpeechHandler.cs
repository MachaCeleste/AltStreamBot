using System.Speech.Synthesis;

namespace AltStreamBot.Modules;

public class SpeechHandler
{
    public bool TTS;
    public bool SayName;

    private SpeechSynthesizer _synth;
    private Dictionary<string, string> _userVoices;

    public SpeechHandler()
    {
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();
        _userVoices = new Dictionary<string, string>();
    }

    public async void Synthesize(string username, string input)
    {
        if (!TTS) return;

        if (!_userVoices.ContainsKey(username.ToLower()))
        {
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            var voices = _synth.GetInstalledVoices();
            _userVoices.Add(username.ToLower(), voices[rng.Next(voices.Count - 1)].VoiceInfo.Name);
        }

        _synth.SelectVoice(_userVoices[username.ToLower()]);

        string output = SayName ? $"{username} said {input}" : input;

        SynthAudioPlayer(output);
    }

    public async void SystemMessage(string input)
    {
        _synth.SelectVoice("Microsoft David Desktop");
        SynthAudioPlayer(input);
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
