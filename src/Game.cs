using Joonaxii.Engine;
using Joonaxii.Engine.Components;
using Joonaxii.Engine.Core;
using Joonaxii.Engine.Entities;
using Joonaxii.Engine.Rendering.TXT;
using Joonaxii.MathJX;
using System;
using System.Collections.Generic;

namespace Joonaxii.Physics.Demo
{
    public class Game
    {
        public static Game Instance { get; private set; }

        public bool IsStarted { get; private set; }

        public GameObjectManager GameObjectManager { get; private set; }

        private TXTRenderer _renderer;

        private static Dictionary<byte, TXTSprite> _radiusToSprite = new Dictionary<byte, TXTSprite>();
        private static char[] _circleFalloff = new char[] { '\0', '-', '+', 'o', '¤', '#', 'O', '0', '@' };

        private TimedThread _mainThread;
        private TimedThread _renderThread;

        public Game()
        {
            IsStarted = false;
            Instance = this;
            _renderer = new TXTRenderer();

            _mainThread = new TimedThread(MainLoop);
            _renderThread = new TimedThread((Time tt) => { _renderer.Render(); });

            _mainThread.SetSyncThread(_renderThread, true, true);

            GameObjectManager = new GameObjectManager();
            int count = 196;

            Console.WriteLine($"Generating Circles... 0/{count}");
            for (int i = 1; i <= count; i++)
            {
                TXTure.CreateCircle((byte)i, 5.0f, _circleFalloff, out TXTure txt, out TXTSprite sprt);
                _radiusToSprite.Add((byte)i, sprt);

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Generating Circles... {i + 1}/{count}".PadRight(64));
            }
        }

        public void Start()
        {
            if (IsStarted) { return; }
            IsStarted = true;

            _mainThread.Start(true);

            GameObject goTest = new GameObject("Test");
            var sprt = goTest.AddComponent<TXTSpriteRenderer>();
            var plr = goTest.AddComponent<PlayerController>();

            goTest.IsActive = true;
            sprt.sprite = _radiusToSprite[16];
            goTest.Transform.WorldPosition = new Vector2(0, 0);

            while (_mainThread.IsActive || _renderThread.IsActive) { }
            IsStarted = false;
        }

        private void MainLoop(Time time)
        {
            Input.Update();
            GameObjectManager.Update(time.DeltaTime);

            {
                _renderer.WriteReserved($"FPS (Main): {time.FrameRate}", 0, 0, 32);
                _renderer.WriteReserved($" -Time: {time.RunTime}", 0, 1, 32);
         
                _renderer.WriteReserved($"FPS (Render): {_renderThread.Time.FrameRate}", 0, 3, 32);
                _renderer.WriteReserved($" -Time: {_renderThread.Time.RunTime}", 0, 4, 32);
            }
        }
    }
}
