using System.Media;

namespace AltStreamBot.Modules;

public class SoundHandler
{
    private Dictionary<SoundFile, SoundPlayer> _sounds;

    public SoundHandler()
    {
        _sounds = new Dictionary<SoundFile, SoundPlayer>();
        foreach (SoundFile file in Enum.GetValues(typeof(SoundFile)))
            _sounds.Add(file, new SoundPlayer($".\\Sounds\\{file.ToString().ToLower()}.wav"));
    }

    public void PlaySound(SoundFile file) =>
        _sounds[file].PlaySync();
}

public enum SoundFile
{
    Redeem
}
