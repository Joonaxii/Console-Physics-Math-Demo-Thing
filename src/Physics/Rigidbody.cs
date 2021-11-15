using Joonaxii.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joonaxii.Physics.Demo.Physics
{
    public class Rigidbody
    {
        public const float GRAVITY = -9.81f;

        public const float ROOF_HEIGHT = 59.0f;
        public const float FLOOR_HEIGHT = -58.0f;
        public const float WALL_POS = 63.0f;
        public const float BOUNCE_THRESHOLD = 0.333f;

        public const float TERMINAL_VELOCITY = -20.0f;

        public Transform Transform { get; private set; }

        public float Radius { get; set; }
        public bool IsActive { get; set; } = true;

        public float Mass { get; private set; } = 1f;
        public Vector2 Velocity { get => _velocity; }
        public Vector2 Position { get => _position; }

        private float _bounciness = 1f;
        private float _drag = 0.05f;
        private float _gravityScale = 0f;
        private Vector2 _velocity;

        public void SetVelocity(Vector2 vel) => _velocity = vel;
        public void SetPosition(Vector2 pos) => _position = pos;

        private bool _isGrounded;
        private float _acc;

        public void Update(float delta)
        {
            if (!IsActive) { return; }
            if (!_isGrounded)
            {
                _acc += GRAVITY * delta * Mass * _gravityScale;
                _velocity.y += _acc * delta;

                _velocity.y = System.Math.Max(TERMINAL_VELOCITY, _velocity.y);
            }

            _position += _velocity;
            bool left = _velocity.x < 0;
            bool down = _velocity.y < 0;

            float yBot = _position.y - Radius;
            float yTop = _position.y + Radius;

            float xLeft = _position.x - Radius;
            float xRight = _position.x + Radius;

            float b = (float)System.Math.Pow(_bounciness, 1.05f);

            if (xLeft <= -WALL_POS)
            {
                _position.x = -WALL_POS + Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.right);
                _velocity.x *= b;
                _velocity.x = _velocity.x < BOUNCE_THRESHOLD ? 0 : _velocity.x;
            }

            if (xRight >= WALL_POS)
            {
                _position.x = WALL_POS - Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.left);
                _velocity.x *= b;
                _velocity.x = _velocity.x > -BOUNCE_THRESHOLD ? 0 : _velocity.x;
            }

            if (yTop >= ROOF_HEIGHT)
            {
                _position.y = ROOF_HEIGHT - Radius;
                _velocity = Vector2.Reflect(_velocity, Vector2.down);
                _velocity.y *= b;
                _velocity.y = _velocity.y > -BOUNCE_THRESHOLD ? 0 : _velocity.y;
            }

            if (yBot <= FLOOR_HEIGHT)
            {
                _position.y = FLOOR_HEIGHT + Radius;
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
        }
    }
}
