using AltStreamLib.EventSub;
using AltStreamLib.EventSub.Chat.Models;
using AltStreamLib.EventSub.Chat.Models.Events;
using AltStreamLib.Logging;
using AltStreamBot.Modules;

namespace AltStreamBot;

public class Program
{
    private static EventSubClient? _client;
    private static SpeechHandler? _synth;
    private static SoundHandler? _player;

    private static System.Timers.Timer? _glassesTimer;

    public static async Task Main()
    {
        new Logger();
        Logger.Singleton.OnLog += Logger_OnLog;

        int i = 0;
        string? username = null;
        while (string.IsNullOrWhiteSpace(username) && i < 3)
        {
            Console.Clear();

            Console.Write(i > 0 ? "Invalid Username\n>" : "Enter Username\n>");
            username = Console.ReadLine();

            i++;

            if (i >= 3 && string.IsNullOrWhiteSpace(username))
                Environment.Exit(1);
        }

        _client = new EventSubClient(username.ToLower());

        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnRedemptionReceived += Client_OnRedemptionReceived;

        _synth = new SpeechHandler();
        _player = new SoundHandler();

        _glassesTimer = new(TimeSpan.FromSeconds(300));
        _glassesTimer.AutoReset = true;
        _glassesTimer.Elapsed += _glassesTimer_Elapsed;

        Console.WriteLine($"Welcome {username} <3\n");

        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
                continue;

            string[] parts = userInput.Split(' ');

            string cmd = parts[0];
            string[] args = parts[1..];

            switch (cmd)
            {
                case ("connect"):
                    _ = _client.ConnectAsync();
                    continue;
                case ("tts"):
                    if (_synth is null) continue;
                    _synth.TTS = !_synth.TTS;
                    Console.WriteLine($"TTS: [{(_synth.TTS ? "ENABLED" : "DISABLED")}]");
                    continue;
                case ("sayname"):
                    if (_synth is null) continue;
                    _synth.SayName = !_synth.SayName;
                    Console.WriteLine($"Say Name: [{(_synth.SayName ? "ENABLED" : "DISABLED")}]");
                    continue;
                case ("clear"):
                case ("cls"):
                    Console.Clear();
                    continue;
            }
        }
    }

    private static void Client_OnRedemptionReceived(EventMessage context)
    {
        if (context.EventType is EventType.Redeem && context.Event is Redemption redeem)
        {
            _player.PlaySound(SoundFile.Redeem);

            string username = redeem.From;
            string redeemName = redeem.RewardName;

            switch (redeemName.ToLower())
            {
                case "reroll voice":
                    _synth?.RerollUserVoice(username);
                    break;

                case "lose the glasses":
                    if (_synth is not null && !_synth.TTS)
                        _synth.TTS = true;
                    _glassesTimer?.Start();
                    break;
            }
        }
        else if (context.EventType is EventType.RaidReceived && context.Event is RaidIncoming raidIn)
        {

        }
    }

    private static void _glassesTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _glassesTimer?.Stop();
        if (_synth is not null)
        {
            _synth.TTS = false;
            _synth.SystemMessage("Lose the glasses redeem has ended.");
        }
    }

    private static void Client_OnMessageReceived(Message context)
    {
        Console.WriteLine($"{DateTimeOffset.FromUnixTimeMilliseconds(context.SentAt)} <{context.Author}> {context.Text}");

        if (context.AuthorUsername is not null && !context.AuthorUsername.Equals(_client?.ChannelName, StringComparison.OrdinalIgnoreCase))
            _synth?.Synthesize(context.Author, context.Text);
    }

    private static void Logger_OnLog(LogMessage log) =>
        Console.WriteLine($"{DateTime.Now} [SYSTEM] {log.LogLevel}: {log.Message}");
}