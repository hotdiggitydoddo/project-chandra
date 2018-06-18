namespace ProjectChandra.Shared.Helpers
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,
        UpperRight,
        LowerRight,
        LowerLeft,
        UpperLeft
    }

    public enum LayerMask
    {
        All = -1,
        Player = 0,
        Obstacles = 1
    }
}
