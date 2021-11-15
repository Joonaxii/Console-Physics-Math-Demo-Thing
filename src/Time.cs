using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joonaxii.Physics.Demo
{
    public class Time
    {
        public float TimeScale = 1.0f;

        public float RunTime { get; private set; }
        public float DeltaTime { get; private set; }

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
            _prevTime = _curTime;
            _curTime = _sw.Elapsed.TotalSeconds;

            DeltaTime = (float)(_curTime - _prevTime) * TimeScale;
            RunTime += DeltaTime;

            FrameCount++;
            FrameRate = FrameCount / RunTime;
        }
    }
}
