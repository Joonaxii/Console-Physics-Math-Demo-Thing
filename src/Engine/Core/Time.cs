using System.Diagnostics;

namespace Joonaxii.Engine.Core
{
    public class Time
    {
        public float TimeScale = 1.0f;

        public float RunTime { get; private set; }
        public float DeltaTime { get; private set; }
        public float EstDeltaTime { get; private set; }

        public float FrameRate { get; private set; }
        public ulong FrameCount { get; private set; }

        private Stopwatch _sw;

        private double _curTime;
        private double _prevTime;

        public Time()
        {
            _sw = new Stopwatch();
            Reset();
        }

        public void Reset()
        {
            FrameRate = 0;

            _prevTime = 0;
            _curTime = 0;

            RunTime = 0;
            DeltaTime = 0;
            FrameCount = 0;
            _sw.Reset();
        }

        public void Start()
        {
            _sw.Start();
        }

        public WaitForSeconds WaitForSeconds(float dur) => new WaitForSeconds(this, dur);

        public void Tick()
        {
            const double MS_TO_SEC = (1.0 / 1000.0);
            //const double MAX_FPS = (1.0 / 144.0) * 1000.0;

            _prevTime = _curTime;
            _curTime = _sw.Elapsed.TotalMilliseconds;

            double deltaMS = _curTime - _prevTime;

            DeltaTime = (float)(deltaMS * MS_TO_SEC) * TimeScale;
            RunTime += DeltaTime;

            FrameCount++;
            FrameRate = FrameCount / RunTime;

            EstDeltaTime = 1.0f / FrameRate;

            //if(deltaMS < MAX_FPS)
            //{
            //    int ms = (int)(MAX_FPS - deltaMS);
            //    Thread.Sleep(ms);
            //}
        }
    }
}
