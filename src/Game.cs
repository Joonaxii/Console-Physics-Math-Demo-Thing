using Joonaxii.Engine.Components;
using Joonaxii.Engine.Core;
using Joonaxii.Math;
using Joonaxii.Physics.Demo.Physics;
using Joonaxii.Physics.Demo.Rendering;
using System;
using System.Collections.Generic;

namespace Joonaxii.Physics.Demo
{
    class Game
    {
        private static TXTRenderer _renderer;

        private static Dictionary<byte, Sprite> _radiusToSprite = new Dictionary<byte, Sprite>();
        private static char[] fall = new char[] { '\0', '-', '+', 'o', '¤', '#', 'O', '0', '@' };

        private static TimedThread _mainThread;
        private static TimedThread _renderThread;

        static void Main(string[] args)
        {
            _renderer = new TXTRenderer();
            _mainThread = new TimedThread(MainLoop);
            _renderThread = new TimedThread((Time tt) => { _renderer.Render(); });

            _mainThread.SetSyncThread(_renderThread, true, true);

            int count = 256;

            Console.WriteLine($"Generating Circles... 0/{count}");
            for (int i = 1; i <= count; i++)
            {
                TXTure.CreateCircle((byte)i, 5.0f, fall, out TXTure txt, out Sprite sprt);
                _radiusToSprite.Add((byte)i, sprt);

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Generating Circles... {i + 1}/{count}".PadRight(64));
            }

            rendererTest = new SpriteRenderer();
            rendererTest.IsActive = true;
            rendererTest.layer.SetLayer("Default");
            rendererTest.layer.SetOrder(10);

            SpriteRenderer rendererGround = new SpriteRenderer();
            SpriteRenderer rendererRoof = new SpriteRenderer();

            SpriteRenderer rendererWallL = new SpriteRenderer();
            SpriteRenderer rendererWallR = new SpriteRenderer();

            rendererTestCirc = new SpriteRenderer();
            rendererTestCirc.IsActive = true;
            rendererTestCirc.layer.SetLayer("Default");
            rendererTestCirc.layer.SetOrder(9);

            rendererGround.IsActive = true;
            rendererGround.layer.SetLayer("Foreground");
            rendererGround.layer.SetOrder(10);

            rendererRoof.IsActive = true;
            rendererRoof.layer.SetLayer("Foreground");
            rendererRoof.layer.SetOrder(9);

            rendererWallL.IsActive = true;
            rendererWallL.layer.SetLayer("Foreground");
            rendererWallL.layer.SetOrder(8);

            rendererWallR.IsActive = true;
            rendererWallR.layer.SetLayer("Foreground");
            rendererWallR.layer.SetOrder(8);

            TXTure.CreateFromChars(new string('=', TXTRenderer.BUFFER_W * 2).ToCharArray(), TXTRenderer.BUFFER_W * 2, 1, out var t, out var spr, new Vector2(0.5f, 0));
            TXTure.CreateFromChars(new string('#', TXTRenderer.GAME_AREA_HEIGHT * 2).ToCharArray(), 1, TXTRenderer.GAME_AREA_HEIGHT * 2, out var tW, out var sprW, new Vector2(0.0f, 0.5f));

            rendererGround.sprite = spr;
            rendererRoof.sprite = spr;

            rendererWallL.sprite = sprW;
            rendererWallR.sprite = sprW;

            radius = 6;
            radiusB = radius < 1 ? (byte)1 : (byte)radius;
            prev = radiusB;

            rendererTest.sprite = radiusB < 1 ? null : _radiusToSprite[radiusB];
            rb = new Rigidbody();

            rb.SetPosition(new Vector2(-0, 12.0f));
            rb.SetVelocity(new Vector2(1, -0.05f) * 45);

            rendererTest.SetPosition(rb.Position);
            rendererTestCirc.SetPosition(rb.Position);

            rendererGround.SetPosition(new Vector2(0, Rigidbody.FLOOR_HEIGHT));
            rendererRoof.SetPosition(new Vector2(0, Rigidbody.ROOF_HEIGHT));

            rendererWallL.SetPosition(new Vector2(-Rigidbody.WALL_POS, 0));
            rendererWallR.SetPosition(new Vector2(Rigidbody.WALL_POS, 0));

            TXTure.LoadLegacy("res/Player_R.sprt", out var txtPlrR, out sprtPlrR);
            TXTure.LoadLegacy("res/Player_U.sprt", out var txtPlrU, out sprtPlrU);
            TXTure.LoadLegacy("res/Player_UR.sprt", out var txtPlrUR, out sprtPlrUR);

            rendererTest.FlipY = true;
            rendererTest.FlipX = false;

            rendererTest.sprite = sprtPlrU;

            radius = 24;
            //rb.Radius = radius * 0.5f;
            radiusB = radius < 1 ? (byte)1 : (byte)radius;

            TXTure.CreateCircle((byte)radius, 1.5f, 0.75f, 0.005f, 0.005f, fall, out var txtC, out var sprtC);
            rendererTestCirc.sprite = _radiusToSprite[radiusB];

            //if (prev != radiusB)
            //{
            //    prev = radiusB;
            //    rendererTest.sprite = radiusB < 1 ? null : _radiusToSprite[radiusB];
            //}

            trPlr = new Transform();
            trCircle = new Transform();

            trsInner = new Transform[96];
            rendsInner = new SpriteRenderer[trsInner.Length];

            float ang = (360.0f / trsInner.Length) * MathJX.Deg2Rad;
            circ = _radiusToSprite[5];

            positions = new Vector2[trsInner.Length];
            float radiusS = 25;
            for (int i = 0; i < trsInner.Length; i++)
            {
                var tr = trsInner[i] = new Transform();
                var rend = rendsInner[i] = new SpriteRenderer();

                rend.IsActive = true;
                rend.layer.SetLayer("Default");
                rend.layer.SetOrder(8);

                rend.sprite = circ;

                float radI = ang * i;


                Vector2 point = positions[i] = new Vector2(MathJX.Cos(radI), MathJX.Sin(radI)) * radiusS;
                tr.LocalPosition = point;
                tr.Parent = trCircle;
            }

            trCircle.LocalPosition += Vector2.up * 30.0f;
            trPlr.AddChild(trCircle);

            trPlr.WorldPosition = rb.Position;

            _mainThread.Start(true);
            while (_mainThread.IsActive || _renderThread.IsActive) { }
        }



        private static Rigidbody rb;
        private static SpriteRenderer rendererTestCirc;
        private static SpriteRenderer rendererTest;
        private static Transform trPlr = new Transform();
        private static Transform trCircle = new Transform();
        private static Vector2[] positions;
        private static Transform[] trsInner = new Transform[96];
        private static SpriteRenderer[] rendsInner;

        private static Sprite circ;
        private static Sprite sprtPlrR;
        private static Sprite sprtPlrU;
        private static Sprite sprtPlrUR;

        private static byte radiusB;
        private static byte prev;

        private static float spd = 90;
        private static float an = 0;
        private static float radius = 6;
        private static float anInner = 0;
        private static float anSpeed = 90.0f;

        private static float frequency = 8.0f;
        private static float timeSin = 0;

        private static int sprt = 1;
        private static void MainLoop(Time time)
        {
            //while (true)
            {
                float rad = an * MathJX.Deg2Rad;

                timeSin += 180 * time.DeltaTime;
                an += spd * time.DeltaTime;
                anInner += anSpeed * time.DeltaTime;
                rb.Update(time.DeltaTime);

                var n = rb.Velocity.Normalized;

                float vX = n.x;
                float vY = n.y;

                float absvX = System.Math.Abs(vX);
                float absvY = System.Math.Abs(vY);

                bool isUp = vY >= 0;
                bool isRight = vX >= 0;

                bool isXT = absvX <= 0.15f;
                bool isYT = absvY <= 0.15f;

                //000 001 010 100 101 110 111
                byte b = 0;

                Vector2 aa = new Vector2(absvX, absvY);
                float aaA = System.Math.Abs(MathJX.Angle(aa, Vector2.one));

                b = (byte)(aaA < 90 ? (absvX < absvY ? 1 : 0) : 2);

                rendererTest.FlipX = !isRight;
                rendererTest.FlipY = !isUp;

                if (sprt != b)
                {
                    sprt = b;
                    switch (b)
                    {
                        case 0:
                            rendererTest.sprite = sprtPlrR;
                            break;
                        case 1:
                            rendererTest.sprite = sprtPlrU;
                            break;
                        case 2:
                            rendererTest.sprite = sprtPlrUR;
                            break;
                    }
                }

                trCircle.LocalRotation = anInner;
                trPlr.WorldRotation = an;
                trPlr.WorldPosition = rb.Position;


                float offset = (MathJX.Sin(time.RunTime * 0.5f * MathJX.TWO_PI) + 1.0f) * 0.5f;
                for (int i = 0; i < trsInner.Length; i++)
                {
                    float nN = i / (trsInner.Length - 1.0f);
                    trsInner[i].LocalPosition = Vector2.Lerp(positions[i] * 0.75f, positions[i] * 1.25f, (MathJX.Sin(((timeSin * MathJX.Deg2Rad) + (frequency * nN) * MathJX.TWO_PI)) * offset + 1.0f) * 0.5f);
                    rendsInner[i].SetPosition(trsInner[i].WorldPosition);
                }

                rendererTest.SetPosition(trPlr.LocalPosition);
                rendererTestCirc.SetPosition(trCircle.WorldPosition);

                _renderer.WriteReserved($"FPS (Main): {time.FrameRate}", 0, 0, 32);
                _renderer.WriteReserved($" -Time: {time.RunTime}", 0, 1, 32);
         
                _renderer.WriteReserved($"FPS (Render): {_renderThread.Time.FrameRate}", 0, 3, 32);
                _renderer.WriteReserved($" -Time: {_renderThread.Time.RunTime}", 0, 4, 32);
            }
        }
    }
}
