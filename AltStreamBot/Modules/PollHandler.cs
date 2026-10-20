namespace AltStreamBot.Modules;

public class PollHandler(SpeechHandler synth, string description, string[] options, int timeInSeconds = 120, int readbackInterval = 30)
{
    public event Action<string>? PollCompleteCallback;

    public string Description { get; private set; } = description;
    private SpeechHandler _synth = synth;
    private string[] _options = options;

    private List<string> _voters = [];
    private Dictionary<string, int> _results = [];
    private System.Timers.Timer _timer = new(TimeSpan.FromSeconds(timeInSeconds));
    private System.Timers.Timer _readout = new(TimeSpan.FromSeconds(readbackInterval));

    public async Task StartPoll()
    {
        bool state = _synth.TTS;

        if (state)
            _synth.TTS = false;

        foreach (var option in _options)
            _results.Add(option, 0);

        _synth.SystemMessage($"" +
            $"A new poll has been started.");

        _synth.TTS = state;

        await Readout();

        _readout.AutoReset = true;
        _readout.Elapsed += _readout_Elapsed;

        _timer.Elapsed += _timer_Elapsed;

        _timer.Start();
        _readout.Start();
    }

    public void AddVote(string username, int choice)
    {
        if (_voters.Contains(username) || _results.Count < choice || choice <= 0) return;

        _voters.Add(username);
        _results[_results.ElementAt(choice).Key]++;
    }

    private void _readout_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) =>
        _ = Readout();

    private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        bool state = _synth.TTS;

        if (state)
            _synth.TTS = false;

        _readout.Stop();
        _readout.Dispose();

        _timer.Dispose();

        var winner = _results.MaxBy(x => x.Value);

        PollCompleteCallback?.Invoke($"The poll for {Description} has ended, the winner is {winner.Key} with {winner.Value} votes.");

        _synth.TTS = state;
    }

    private async Task Readout()
    {
        bool state = _synth.TTS;

        if (state)
            _synth.TTS = false;

        _synth.SystemMessage($"{Description}\n" +
            $"Please select from these options:\n");

        int i = 1;
        foreach (var result in _results)
        {
            _synth.SystemMessage($"{i} {result.Key}");
            i++;
        }

        _synth.TTS = state;
    }
}
