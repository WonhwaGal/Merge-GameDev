
public struct KeyEvent : IGameEvent
{
    public readonly KeyBubble KeyBubble;

    public KeyEvent(KeyBubble keyView)
    {
        KeyBubble = keyView;
    }
}

