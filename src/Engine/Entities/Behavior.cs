using Joonaxii.Engine.Components;
using Joonaxii.Engine.Core;
using Joonaxii.Physics.Demo.Physics;
using Joonaxii.Physics.Demo.Rendering;

namespace Joonaxii.Engine.Entities
{
    public abstract class Behavior : Component
    {
        protected float _time;

        public void Update(float delta)
        {
            if (!_enabled) { return; }

            EarlyUpdate(delta);
            _time += delta;         
            LateUpdate(delta);
        }

        protected abstract void EarlyUpdate(float delta);
        protected abstract void LateUpdate(float delta);
    }
}
