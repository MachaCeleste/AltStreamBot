using AltStreamEventSub;
using AltStreamEventSub.Api.Chat.Models;
using AltStreamEventSub.Api.Chat.Models.Events;
using AltStreamBot.Modules;

namespace AltStreamBot;

public class Program
{
    private static SpeechHandler? _synth;

    public static async Task Main()
    {
        var client = new EventSubClient("machaceleste");
        client.OnLog += Client_OnLog;

        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnRedemptionReceived += Client_OnRedemptionReceived;

        _synth = new SpeechHandler();

        while (true)
        {
            string? userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
                continue;

            string[] parts = userInput.Split(' ');

            string cmd = parts[0];
            string[] args = parts[1..];

            switch (cmd)
            {
                case ("connect"):
                    _ = client.ConnectAsync();
                    continue;
                case ("tts"):
                    if (_synth is null) continue;
                    _synth.TTS = !_synth.TTS;
                    continue;
            }
        }
    }

    private static void Client_OnRedemptionReceived(EventMessage context)
    {
        if (context.EventType is EventType.Redeem && context.Event is Redemption redeem)
        {
            string username = redeem.From;
            string redeemName = redeem.RewardName;

            switch (redeemName)
            {
                case "Reroll Voice":
                    _synth?.RerollUserVoice(username);
                    break;
            }

        }
    }

    private static void Client_OnMessageReceived(Message context)
    {
        Console.WriteLine($"{DateTimeOffset.FromUnixTimeMilliseconds(context.SentAt)} <{context.Author}> {context.Text}");

        _synth?.Synthesize(context.Author, context.Text);
    }


    private static void Client_OnLog(string message) =>
        Console.WriteLine($"{DateTime.Now} [SYSTEM] {message}");
}