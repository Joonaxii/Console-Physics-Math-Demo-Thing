using System.Collections;

namespace Joonaxii.Engine.Core
{
    public abstract class YieldInstruction : IEnumerator
    {
        protected Time _timeSpace;
        public abstract bool KeepWaiting { get; }

        public object Current => null;
        public bool MoveNext() => KeepWaiting;

        public void Setup(Time timeSpace) { _timeSpace = timeSpace; }
        public void Reset() { }
    }
}