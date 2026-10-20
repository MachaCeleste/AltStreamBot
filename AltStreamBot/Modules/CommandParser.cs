using System.Text.RegularExpressions;

namespace AltStreamBot.Modules;

public class CommandParser
{
    public string Command { get; set; }
    public List<string> Arguments { get; set; }

    public static CommandParser Parse(string input)
    {
        var parts = Regex.Matches(input, @"""[^""]*""|[^\s]+")
            .Cast<Match>().Select(m => m.Value.Trim('"')).ToList();

        return new CommandParser
        {
            Command = parts[0],
            Arguments = [..parts.Skip(1)]
        };
    }
}
