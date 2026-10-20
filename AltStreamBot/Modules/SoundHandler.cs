using System.Media;

namespace AltStreamBot.Modules;

public class SoundHandler
{
    private Dictionary<SoundFile, SoundPlayer> _sounds;

    private int _delay = 30;
    private DateTime _lastMessage;

    public SoundHandler()
    {
        _sounds = new Dictionary<SoundFile, SoundPlayer>();
        foreach (SoundFile file in Enum.GetValues(typeof(SoundFile)))
            _sounds.Add(file, new SoundPlayer($".\\Sounds\\{file.ToString().ToLower()}.wav"));
    }

    public void PlaySound(SoundFile file, bool shouldDelay = false)
    {
        if (shouldDelay && (DateTime.Now - _lastMessage).TotalSeconds < _delay)
            return;

        _sounds[file].PlaySync();
    }

    public void SetDelay(int delay) =>
        _delay = delay;
}

public enum SoundFile
{
    Redeem,
    Raid
}
