namespace AltStreamBot.Modules;

public class CallInHandler
{
    public bool CallIn;

    public bool IsInCall;

    private string _callInUsername;
    private TaskCompletionSource<Closer> _callTcs = new TaskCompletionSource<Closer>(TaskCreationOptions.RunContinuationsAsynchronously);

    public void UserHandgup() =>
        _callTcs.TrySetResult(Closer.User);
}

public enum Closer
{
    Owner,
    User
}