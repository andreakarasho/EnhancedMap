namespace EnhancedMap.GUI
{
    public enum MouseState
    {
        HOVER,
        DOWN,
        OUT
    }

    public interface ICustomControl
    {
        MouseState MouseState { get; set; }
    }
}