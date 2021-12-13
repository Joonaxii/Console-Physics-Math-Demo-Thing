using Joonaxii.Engine.Components;
using Joonaxii.MathJX;
using System;

namespace Joonaxii.Physics.Demo.Rendering
{
    public class Sprite
    {
        public string Name { get; private set; }

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public Rect Bounds { get => _bounds; }

        private uint _start;

        private Vector2 _pivot;
        private Vector2Int _pivotInt;
        private TXTure _source;
        private Rect _bounds;

        private Sprite(TXTure source, string name, uint start, ushort w, ushort h, Vector2 pivot)
        {
            Name = name;

            _source = source;
            _start = start;

            Width = w;
            Height = h;

            _pivot = new Vector2((-pivot.x * Width), (-pivot.y * Height));
            _pivotInt = new Vector2Int((int)_pivot.x, (int)_pivot.y);

            Vector2 extents = new Vector2(w * 0.5f, h * 0.5f);
            Vector2 diff = extents + _pivot;

            _bounds.SetMinMax(new Vector2(-extents.x + diff.x, -extents.x + diff.y), new Vector2(extents.x + diff.x, extents.x + diff.y));
        }

        public void Draw(Transform transform, TransformConstraints constraints, char[] buffer, uint[] depthBuffer, uint depth, bool flipX, bool flipY)
        {
            if(constraints == TransformConstraints.RotScale) 
            {
                Vector2Int posI = new Vector2Int
                {
                    x = (TXTRenderer.BUFFER_W / 2) + (int)transform.WorldPosition.x,
                    y = (TXTRenderer.GAME_AREA_HEIGHT / 2) - (int)transform.WorldPosition.y
                };

                Draw(posI, buffer, depthBuffer, depth, flipX, flipY); 
                return; 
            }

            Vector2 scal = transform.WorldScale;
            Vector2 posT = transform.WorldPosition;
            Matrix3x3 mat = transform.WorldMatrix;

            int w;
            int h;

            int wMin;
            int hMin;

            int oW;
            int oH;

            float lW;
            float lH;

            int offX;
            int offY;

            Vector2 piv;

            float xWrld;
            float yWrld;

            Vector2 pos;

            switch (constraints)
            {
                case TransformConstraints.None:
                    w = (int)(Width * scal.x);
                    h = (int)(Height * scal.y);

                    oW = Width - 1;
                    oH = Height - 1;

                    lW = w < 2 ? 1.0f : w - 1.0f;
                    lH = h < 2 ? 1.0f : h - 1.0f;

                    offX = TXTRenderer.BUFFER_W / 2;
                    offY = TXTRenderer.GAME_AREA_HEIGHT / 2;

                    piv = mat.ScaleVector(_pivot);

                    xWrld = offX + posT.x; 
                    yWrld = offY + posT.y; 

                    for (ushort y = 0; y < h; y++)
                    {
                        int ogY = (int)Math.Round(oH * (flipY ?  1.0f - (y / lH) : (y / lH)));
                        pos.y = (y + piv.y);
         
                        for (ushort x = 0; x < Width; x++)
                        {
                            int ogX = (int)Math.Round(oW * (flipX ? 1.0f - (x / lW) : (x / lW)));

                            pos.x = (x + piv.x);

                            Vector2 point = mat.RotatePoint(pos);

                            int xX = (int)(point.x + xWrld);
                            int yY = /*(TXTRenderer.GAME_AREA_HEIGHT - 1) -*/ (int)(point.y + yWrld);
                            if (xX < 0 || xX >= TXTRenderer.BUFFER_W || yY < 0 || yY >= TXTRenderer.GAME_AREA_HEIGHT) { continue; }
                            yY += TXTRenderer.GAME_AREA_HEIGHT;

                            uint iO = (uint)(ogY * Width + ogX);
                            int iB = yY * TXTRenderer.BUFFER_W + xX;

                            ref uint dBuff = ref depthBuffer[iB];
                            if (dBuff > depth) { continue; }

                            char c = _source.GetPixel(_start + iO);
                            if (c == '\0') { continue; }

                            buffer[iB] = c;
                            dBuff = depth;
                        }
                    }
                    break;
            }


            //position += _offset;

          
        }

        public void Draw(Vector2Int position, char[] buffer, uint[] depthBuffer, uint depth, bool flipX, bool flipY)
        {
            position += _pivotInt;

            for (ushort y = 0; y < Height; y++)
            {
                int yy = position.y + (flipY ? Height - 1 - y : y);
                if(yy < 0 || yy >= TXTRenderer.GAME_AREA_HEIGHT) { continue; }
                yy += TXTRenderer.GAME_AREA_HEIGHT;

                for (ushort x = 0; x < Width; x++)
                {
                    int xx = position.x + (flipX ? Width - 1 - x : x);
                    if (xx < 0 || xx >= TXTRenderer.BUFFER_W) { continue; }

                    int i = yy * TXTRenderer.BUFFER_W + xx;
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