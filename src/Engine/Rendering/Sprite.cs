using Joonaxii.Math;

namespace Joonaxii.Physics.Demo.Rendering
{
    public class Sprite
    {
        public string Name { get; private set; }

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        private uint _start;
        private Vector2Int _offset;
        private TXTure _source;

        private Sprite(TXTure source, string name, uint start, ushort w, ushort h, Vector2 pivot)
        {
            Name = name;

            _source = source;
            _start = start;

            Width = w;
            Height = h;
 
            _offset = new Vector2Int((int)(-pivot.x * Width), (int)(-pivot.y * Height));
        }

        public void Draw(Vector2Int position, char[] buffer, uint[] depthBuffer, uint depth, int bufferW, int bufferH, int yOffset, bool flipX, bool flipY)
        {
            position += _offset;

            for (ushort y = 0; y < Height; y++)
            {
                int yy = position.y + (flipY ? Height - 1 - y : y);
                if(yy < 0 || yy >= bufferH) { continue; }
                yy += yOffset;

                for (ushort x = 0; x < Width; x++)
                {
                    int xx = position.x + (flipX ? Width - 1 - x : x);
                    if (xx < 0 || xx >= bufferW) { continue; }

                    int i = yy * bufferW + xx;
                    uint iS = (uint)(y * Width + x);

                    char c = _source.GetPixel(_start + iS);
                    if(c == '\0') { continue; }

                    ref uint dBuff = ref depthBuffer[i];
                    if(dBuff > depth) { continue; }
                    buffer[i] = c;
                    dBuff = depth;
                }
            }
        }

        public static Sprite Create(TXTure source, string name, uint start, ushort w, ushort h, Vector2 pivot) => 
            new Sprite(source, name, start, w, h, pivot);
    }
}