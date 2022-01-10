using Joonaxii.Engine.Components;
using Joonaxii.Engine.Entities;
using Joonaxii.MathJX;

namespace Joonaxii.Engine.Physics
{
    public class Rigidbody : Behavior
    {
        public const float GRAVITY = -9.81f;

        public const float ROOF_HEIGHT = 59.0f;
        public const float FLOOR_HEIGHT = -58.0f;
        public const float WALL_POS = 63.0f;
        public const float BOUNCE_THRESHOLD = 0.333f;

        public const float TERMINAL_VELOCITY = -20.0f;

        public float Radius { get; set; }
        public bool IsActive { get; set; } = true;

        public float Mass { get; private set; } = 1f;
        public Vector2 Velocity { get => _velocity; }
        public Vector2 Position { get => Transform.WorldPosition; }

        private float _bounciness = 1f;
        private float _drag = 0.05f;
        private float _gravityScale = 0f;
        private Vector2 _velocity;

        public void SetVelocity(Vector2 vel) => _velocity = vel;

        private bool _isGrounded;
        private float _acc;

        protected override void LateUpdate(float delta) { }
        protected override void EarlyUpdate(float delta)
        {
            if (!IsActive) { return; }
            if (!_isGrounded)
            {
                _acc += GRAVITY * delta * Mass * _gravityScale;
                _velocity.y += _acc * delta;

                _velocity.y = System.Math.Max(TERMINAL_VELOCITY, _velocity.y);
            }

            Vector2 position = Transform.WorldPosition;
            position += _velocity * delta;
    
            float yBot = position.y - Radius;
            float yTop = position.y + Radius;

            float xLeft = position.x - Radius;
            float xRight = position.x + Radius;

            float b = (float)System.Math.Pow(_bounciness, 1.05f);

            if (xLeft <= -WALL_POS)
            {
                position.x = -WALL_POS + Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.right);
                _velocity.x *= b;
                _velocity.x = _velocity.x < BOUNCE_THRESHOLD ? 0 : _velocity.x;
            }

            if (xRight >= WALL_POS)
            {
                position.x = WALL_POS - Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.left);
                _velocity.x *= b;
                _velocity.x = _velocity.x > -BOUNCE_THRESHOLD ? 0 : _velocity.x;
            }

            if (yTop >= ROOF_HEIGHT)
            {
                position.y = ROOF_HEIGHT - Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.down);
                _velocity.y *= b;
                _velocity.y = _velocity.y > -BOUNCE_THRESHOLD ? 0 : _velocity.y;
            }

            if (yBot <= FLOOR_HEIGHT)
            {
                position.y = FLOOR_HEIGHT + Radius;
                if (!_isGrounded)
                {
                    _acc = 0;
                    _isGrounded = true;
                    _velocity = Vector2.Reflect(_velocity, Vector2.up);
                    _velocity.y *= b;
                    _velocity.y = _velocity.y < BOUNCE_THRESHOLD ? 0 : _velocity.y;
                }
            }
            else
            {
                if (_isGrounded)
                {
                    _isGrounded = false;
                }
            }
            Transform.WorldPosition = position;
        }
    }
}
