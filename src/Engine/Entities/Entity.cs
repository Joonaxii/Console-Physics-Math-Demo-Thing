using Joonaxii.Physics.Demo.Physics;
using Joonaxii.Physics.Demo.Rendering;

namespace Joonaxii.Engine.Entities
{
    public abstract class Entity
    {
        public bool IsActive { get; private set; }

        protected SpriteRenderer _renderer;
        protected Rigidbody _rb;

        protected float _time;

        public void Update(float delta)
        {
            if (!IsActive) { return; }
            EarlyUpdate(delta);

            _time += delta;
            _rb.Update(delta);

            LateUpdate(delta);
        }

        protected abstract void EarlyUpdate(float delta);
        protected abstract void LateUpdate(float delta);
    }
}
