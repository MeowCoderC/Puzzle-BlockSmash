namespace BlockSmash
{
    using UnityEngine;

    public class MouseInput : MonoBehaviour, IInput
    {
        public bool    PointerDown     => Input.GetMouseButtonDown(0);
        public bool    PointerHold     => Input.GetMouseButton(0);
        public bool    PointerUp       => Input.GetMouseButtonUp(0);
        public Vector3 PointerPosition => Input.mousePosition;
    }
}