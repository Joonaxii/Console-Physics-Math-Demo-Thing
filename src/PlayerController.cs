using Joonaxii.Engine;
using Joonaxii.Engine.Entities;
using Joonaxii.MathJX;

namespace Joonaxii.Physics.Demo
{
    public class PlayerController : Behavior
    {
        public float xSpeed = 24.0f;
        public float ySpeed = 24.0f;

        protected override void EarlyUpdate(float delta)
        {
            Vector2 pos = Transform.WorldPosition;

            Vector2 dir = Input.GetAxis(KeyCode.ARROW_L, KeyCode.ARROW_R, KeyCode.ARROW_D, KeyCode.ARROW_U);
            dir.x *= xSpeed * delta;
            dir.y *= ySpeed * delta;
            pos += dir;

            Transform.WorldPosition = pos;
        }

        protected override void LateUpdate(float delta)
        {
            
        }
    }
}