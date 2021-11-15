using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Joonaxii.Physics.Demo
{
    public class TimedThread
    {
        private static uint THREAD_ID_CUR = 0;
        public string Name { get; private set; }
        public uint ID { get; private set; }

        public Time Time { get => _time; }
        public bool IsActive { get => _isActive; }
        private bool _isActive;

        private volatile bool _isStopping;
        private Thread _thread;
        private Time _time;
        private Action<Time> _runnable;

        private bool _controlSync;
        private TimedThread _syncThread;
        private bool _hasSyncThread;
  
        public TimedThread(Action<Time> runnable) : this($"Thread #{THREAD_ID_CUR}", runnable) { }

        public TimedThread(string name, Action<Time> runnable)
        {
            Name = name;
            ID = THREAD_ID_CUR++;
            _runnable = runnable;
            _time = new Time();
        }

        public void SetSyncThread(TimedThread thread, bool controlSync, bool biDirectional)
        {
            if (thread != null && (thread == this || (thread._syncThread == this && thread._controlSync && controlSync))) { return; }

            _controlSync = controlSync;
            _syncThread = thread;

            _hasSyncThread = thread != null;
            if (biDirectional && thread != null) { thread.SetSyncThread(this, false, false); }
        }

        public bool Start(bool wait, Action<Time> runnable)
        {
            if (_isActive) { return false; }
            _runnable = runnable;

            return Start(wait);
        }

        public bool Start(bool wait)
        {
            if (_isActive) { return false; }

            while (_isStopping)
            {
                if (!wait) { return false; }
            }

            _isActive = true;

            _thread = new Thread(Run);
            _thread.Start();

            if (_controlSync) { _syncThread?.Start(wait); }
            return true;
        }

        public bool Stop(bool wait)
        {
            if (_isStopping | !_isActive) { return false; }
            _isStopping = true;

            if (!wait) { return false; }
            _thread.Join();
            return true;
        }

        private void Run()
        {
            _time.Reset();
            _time.Start();

            System.Diagnostics.Debug.WriteLine($"Starting Thread! ({Name}, {ID}) [{(_syncThread != null ? "with sync" : "no sync")}]");

            ulong frameBefore = 0;
            while (true)
            {
                if (_hasSyncThread)
                {
                    frameBefore = _syncThread._time.FrameCount;
                }

                if (_isStopping) { _isStopping = false; break; }
                _runnable.Invoke(_time);
                _time.Tick();

                if (_hasSyncThread)
                {
                    while (_syncThread._time.FrameCount <= frameBefore) { }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Stopping Thread!");
            if (_controlSync) { _syncThread?.Stop(true); }
            _isActive = false;
        }
    }
}
