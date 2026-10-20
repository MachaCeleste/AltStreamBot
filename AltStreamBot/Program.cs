using AltStreamBot.Modules;
using AltStreamLib.Api;
using AltStreamLib.EventSub;
using AltStreamLib.EventSub.Chat.Models;
using AltStreamLib.EventSub.Chat.Models.Events;
using AltStreamLib.Logging;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace AltStreamBot;

public class Program
{
    private static ApiClient _api;
    private static EventSubClient? _client;
    private static SpeechHandler? _synth;
    private static SoundHandler? _player;

    private static PollHandler? _currentPoll;

    private static System.Timers.Timer? _glassesTimer;

    private static Dictionary<string, VoiceGender> _userGenders = [];

    private static readonly Regex AnsiEscapeRegex = new Regex(
    @"\x1B(?:[@-Z\\-_]|\\[[0-?]*[ -/]*[@-~])",
    RegexOptions.Compiled);

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

        _api = new ApiClient();

        _client = new EventSubClient(username.ToLower());

        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnRedemptionReceived += Client_OnRedemptionReceived;
        _client.OnRaidReceived += Client_OnRaidReceived;

        _synth = new SpeechHandler();
        _player = new SoundHandler();

        _glassesTimer = new(TimeSpan.FromSeconds(300));
        _glassesTimer.AutoReset = true;
        _glassesTimer.Elapsed += GlassesTimer_OnElapsed;

        Console.WriteLine($"Welcome {username} <3\n");

        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
                continue;

            var cmd = CommandParser.Parse(userInput);

            switch (cmd.Command)
            {
                case ("connect"):
                    {
                        _ = _client.ConnectAsync();
                        continue;
                    }

                case ("disconnect"):
                    {
                        _ = _client.DisconnectAsync();
                        continue;
                    }

                case ("setdelay"):
                    {
                        if (_player is null || cmd.Arguments.Count < 1
                            || int.TryParse(cmd.Arguments[0], out int res))
                        {
                            Console.WriteLine("Usage: setdelay [timeInSeconds]");
                            return;
                        }

                        _player.SetDelay(res);
                        continue;
                    }

                case ("tts"):
                    {
                        if (_synth is null) continue;
                        _synth.TTS = !_synth.TTS;
                        Console.WriteLine($"TTS: [{(_synth.TTS ? "ENABLED" : "DISABLED")}]");
                        continue;
                    }

                case ("sayname"):
                    {
                        if (_synth is null) continue;
                        _synth.SayName = !_synth.SayName;
                        Console.WriteLine($"Say Name: [{(_synth.SayName ? "ENABLED" : "DISABLED")}]");
                        continue;
                    }

                case ("getvoices"):
                    {
                        if (_synth is null) continue;
                        string output = string.Empty;
                        foreach (var voice in _synth.GetAllVoicesInstalled())
                            output += $"{voice.VoiceInfo.Name} | {voice.VoiceInfo.Culture} | {voice.VoiceInfo.Gender}\n";

                        Console.WriteLine(output);
                        continue;
                    }

                case ("onevoice"):
                    {
                        if (_synth is null) continue;
                        _synth.OneVoice = !_synth.OneVoice;
                        Console.WriteLine($"One Voice: [{(_synth.OneVoice ? "ENABLED" : "DISABLED")}]");
                        continue;
                    }

                case ("setvoice"):
                    {
                        if (_synth is null) continue;
                        if (cmd.Arguments.Count < 2)
                        {
                            Console.WriteLine($"Usage: setvoice \"[voiceName]\" [(1 = M, 2 = F, 3 = NB)]");
                            continue;
                        }

                        string voiceName = cmd.Arguments[0];

                        if (!Enum.TryParse<VoiceGender>(cmd.Arguments[1], out VoiceGender gender))
                        {
                            Console.WriteLine("Failed to parse option.");
                            continue;
                        }

                        if (!_synth.SelectVoice(voiceName, gender))
                            Console.WriteLine($"Failed to set voice, no voice contain: {voiceName}");
                        else
                            Console.WriteLine($"Voice {gender} set to {voiceName}");
                        continue;
                    }
                case ("usepronoun"):
                    {
                        if (_synth is null) return;
                        _synth.UsePronoun = !_synth.UsePronoun;
                        Console.WriteLine($"Use Pronoun: [{(_synth.UsePronoun ? "ENABLED" : "DISABLED")}]");
                        continue;
                    }

                case ("stop"):
                    {
                        if (_synth is null) continue;
                        _synth.StopSynth();
                        continue;
                    }

                //case ("newpoll"): // This is fucking broken
                //    {
                //        if (_currentPoll is not null)
                //        {
                //            Console.WriteLine($"Poll already running: {_currentPoll.Description}");
                //            continue;
                //        }

                //        Console.WriteLine("Enter Description:");
                //        string? description = Console.ReadLine();
                //        if (string.IsNullOrEmpty(description)) continue;

                //        List<string> options = [];
                //        while (true)
                //        {
                //            Console.WriteLine("Enter Option: (Empty ends entry)");
                //            string? option = Console.ReadLine();
                //            if (string.IsNullOrEmpty(option)) break;
                //            options.Add(option);
                //        }
                //        if (options.Count < 2) continue;

                //        _currentPoll = new(_synth!, description, [.. options]);
                //        _currentPoll.PollCompleteCallback += Poll_PollCompleteCallback;
                //        _currentPoll.StartPoll();
                //        continue;
                //    }

                case ("clear"):
                case ("cls"):
                    {
                        Console.Clear();
                        continue;
                    }
            }
        }
    }

    //private static void Poll_PollCompleteCallback(string response)
    //{
    //    _currentPoll = null;
    //    _synth?.SystemMessage(response);
    //}

    private static void Client_OnRaidReceived(EventMessage context)
    {
        var raid = context.Event as RaidIncoming;
        if (raid == null)
        {
            Console.WriteLine("Raid null");
            return;
        }
        Console.WriteLine($"{DateTimeOffset.FromUnixTimeMilliseconds(context.Context.SentAt)} <{raid.SourceUsername}> is raiding with a party of {raid.Viewers}!");

        _player?.PlaySound(SoundFile.Raid);
    }

    private static void Client_OnRedemptionReceived(EventMessage context)
    {
        if (context.EventType is EventType.Redeem && context.Event is Redemption redeem)
        {
            _player?.PlaySound(SoundFile.Redeem, true);

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
    }

    private static void GlassesTimer_OnElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _glassesTimer?.Stop();
        if (_synth is not null)
        {
            _synth.TTS = false;
            _synth.SystemMessage("Lose the glasses redeem has ended.");
        }
    }

    private static async void Client_OnMessageReceived(Message context)
    {
        if (context.AuthorUsername is not null && _currentPoll is not null
            && int.TryParse(context.Text, out int choice))
            _currentPoll.AddVote(context.AuthorUsername, choice);

        if (context.AuthorUsername is not null && !context.AuthorUsername.Equals(_client?.ChannelName, StringComparison.OrdinalIgnoreCase))
        {
            VoiceGender voicePronoun = VoiceGender.Neutral;

            if (_synth is not null && _synth.UsePronoun)
            {
                if (!_userGenders.ContainsKey(context.AuthorUsername))
                {
                    var user = await _api.GetUserSettingsInfoAsync(context.AuthorUsername);
                    if (user is not null && user.Pronouns is not null)
                    {
                        string[] parts = user.Pronouns.Split('/');
                        if (parts[0].Equals("He") || parts[1].Equals("Him"))
                            voicePronoun = VoiceGender.Male;
                        else if (parts[0].Equals("She") || parts[1].Equals("Her"))
                                voicePronoun = VoiceGender.Female;
                        else
                            voicePronoun= VoiceGender.Neutral;
                    }

                    _userGenders.Add(context.AuthorUsername, voicePronoun);
                }
                else
                    voicePronoun = _userGenders[context.AuthorUsername];
            }

            _synth?.Synthesize(context.Author, context.Text, voicePronoun);

            Console.WriteLine(
                $"{DateTimeOffset.FromUnixTimeMilliseconds(context.SentAt)} " +
                $"<{context.Author}>" +
                $"{(_synth is not null && _synth.UsePronoun && _userGenders.ContainsKey(context.AuthorUsername)
                ? $" [{_userGenders[context.AuthorUsername]}]" : "")} " +
                $"{AnsiEscapeRegex.Replace(context.Text ?? string.Empty, string.Empty)}");
        }

    }

    private static void Logger_OnLog(LogMessage log)
    {
        if (!log.LogLevel.Equals(LogLevel.Ping))
            Console.WriteLine($"{DateTime.Now} [SYSTEM] {log.LogLevel}: {log.Message}");
    }
}