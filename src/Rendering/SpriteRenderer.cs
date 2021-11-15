using Joonaxii.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joonaxii.Physics.Demo.Rendering
{
    public class SpriteRenderer : IComparable<SpriteRenderer>
    {
        public bool IsActive { get; set; }

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public SortingLayer layer;
        public Sprite sprite;

        private Vector2Int _position;

        public SpriteRenderer()
        {
            TXTRenderer.Instance.RegisterRenderer(this);
        }

        public int CompareTo(SpriteRenderer other)
        {
            return other.layer.CompareTo(layer);
        }

        public void Draw(char[] buffer, uint[] depthBuffer)
        {
            if (!IsActive || sprite == null) { return; }
            sprite.Draw(_position, buffer, depthBuffer, layer.Union, TXTRenderer.BUFFER_W, TXTRenderer.GAME_AREA_HEIGHT, TXTRenderer.GAME_AREA_START, FlipX, FlipY);
        }

        public void SetPosition(Vector2 position)
        {
            _position.x = (TXTRenderer.BUFFER_W / 2) + (int)position.x;
            _position.y = (TXTRenderer.GAME_AREA_HEIGHT / 2  - (int)position.y);
        }
    }
}
