
public struct LoadADEvent : IGameEvent
{
    public readonly bool StartLoading;
    public LoadADEvent(bool startLoading) => StartLoading = startLoading;
}
