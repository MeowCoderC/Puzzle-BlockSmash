namespace BlockSmash
{
    using UnityEngine;

    public interface IInput
    {
        bool    PointerDown     { get; }
        bool    PointerHold     { get; }
        bool    PointerUp       { get; }
        Vector3 PointerPosition { get; }
    }
}