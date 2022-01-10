
using Joonaxii.MathJX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Joonaxii.Engine
{
    public static class Input
    {
        private static Dictionary<KeyCode, KeyState> _keyLut;
        private static KeyState[] _keysPrev;
        private static KeyState[] _keysNow;
        private static int[] _keys;

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        static Input()
        {
            KeyCode[] keys = Enum.GetValues(typeof(KeyCode)) as KeyCode[];
            _keys = new int[keys.Length];
            _keyLut = new Dictionary<KeyCode, KeyState>();

            _keysPrev = new KeyState[keys.Length];
            _keysNow = new KeyState[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                _keysPrev[i] = new KeyState();
                var now = _keysNow[i] = new KeyState();
                _keyLut.Add(keys[i], now);
                _keys[i] = (int)keys[i];
            }
        }

        public static bool IsKeyDown(KeyCode key) => _keyLut[key].down;
        public static bool IsKeyHeld(KeyCode key) => _keyLut[key].held;
        public static bool IsKeyUp(KeyCode key) => _keyLut[key].up;

        public static Vector2 GetAxis(KeyCode negX, KeyCode posX, KeyCode negY, KeyCode posY)
        {
            Vector2 axis = new Vector2();
            axis.x -= IsKeyHeld(negX) ? 1.0f : 0.0f;
            axis.x += IsKeyHeld(posX) ? 1.0f : 0.0f;

            axis.y -= IsKeyHeld(negY) ? 1.0f : 0.0f;
            axis.y += IsKeyHeld(posY) ? 1.0f : 0.0f;
            return axis;
        }

        public static void Update()
        {
            for (int i = 0; i < _keysNow.Length; i++)
            {
                var prev = _keysPrev[i];
                var now = _keysNow[i];
                prev.CopyFrom(now);

                bool cur = (GetKeyState(_keys[i]) & 0x8000) == 0x8000;

                now.down = !prev.pressed & cur;
                now.held = prev.pressed & cur;
                now.up = prev.pressed & !cur;
                now.pressed = now.down | now.held;
            }
        }

        private class KeyState
        {
            public bool down;
            public bool held;
            public bool up;
            public bool pressed;

            public void CopyFrom(KeyState state)
            {
                down = state.down;
                held = state.held;
                up = state.up;
                pressed = state.pressed;
            }
        }
    }
}
