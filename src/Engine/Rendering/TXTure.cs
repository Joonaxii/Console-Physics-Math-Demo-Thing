using Joonaxii.MathJX;
using System;
using System.Globalization;
using System.IO;

namespace Joonaxii.Engine.Rendering.TXT
{
    public class TXTure
    {
        public const char EMPTY_PIXEL = '½';
        public const char BLACK_PIXEL = '^';

        public uint Size { get; private set; }
        private char[] _pixels;

        private TXTure(char[] pixels)
        {
            _pixels = pixels;
            Size = (uint)_pixels.Length;
        }

        public char GetPixel(uint i) => _pixels[i];

        public static bool CreateFromChars(char[] chars, int w, int h, out TXTure texture, out TXTSprite sprite, Vector2 pivot)
        {
            texture = new TXTure(chars);
            sprite = TXTSprite.Create(texture, $"CUSTOM {w} x {h}", 0, (ushort)w, (ushort)h, pivot);
            return true;
        }

        public static bool CreateCircle(byte radius, float edgePower, char[] falloff, out TXTure texture, out TXTSprite sprite)
        {
            float radH = radius * 0.5f;

            uint dimX = (uint)(radius * 2);
            uint dimY = radius;
            uint reso = dimX * dimY;

            char[] pix = new char[reso];

            int len = falloff.Length - 1;

            Vector2 point = Vector2.zero;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < dimY; y++)
            {
                for (int x = 0; x < dimX; x++)
                {
                    point.x = x;
                    point.y = y + radH;

                    float dst = (point - center).Magnitude;

                    float n = 1.0f - Maths.Clamp((float)Math.Pow(dst / radH, edgePower), 0, 1);
                    int id = Maths.Lerp(0, len, n);
                    pix[y * dimX + x] = falloff[id];
                }
            }

            texture = new TXTure(pix);
            sprite = TXTSprite.Create(texture, $"Circle X{radius} {edgePower.ToString("F2")}", 0, (ushort)dimX, (ushort)dimY, new Vector2(0.5f, 0.4f));
            return true;
        }

        public static bool CreateCircle(byte radius, float edgePower, float edgeStart, float outerEdge, float innerEdge, char[] falloff, out TXTure texture, out TXTSprite sprite)
        {
            float radQrt = radius * 0.25f;
            float radH = radius * 0.5f;
            edgeStart = edgeStart < 0 ? 0 : edgeStart * radQrt;

            outerEdge = outerEdge < 0 ? 1 : outerEdge * radQrt;
            innerEdge = innerEdge < 0 ? 1 : innerEdge * radQrt;

            uint dimX = (uint)(radius * 2);
            uint dimY = radius;
            uint reso = dimX * dimY;

            char[] pix = new char[reso];

            int len = falloff.Length - 1;

            Vector2 point = Vector2.zero;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < dimY; y++)
            {
                for (int x = 0; x < dimX; x++)
                {
                    point.x = x;
                    point.y = y + radH;

                    float dst = (point - center).Magnitude;

                    float n = /*1.0f - MathJX.Clamp((float)MathSys.Pow(dst / radH, edgePower), 0, 1)*/0;
                    float d = 0;

                    if (dst >= edgeStart)
                    {
                        d = (dst - edgeStart) / edgeStart;
                        d = (float)Math.Pow(d, edgePower);

                        n = 1.0f - Maths.Clamp(d, 0, 1);
                    }
                    else
                    {
                        d = (edgeStart - dst) / innerEdge;
                        d = (float)Math.Pow(d, edgePower);
                        n = 1.0f - Maths.Clamp(d, 0, 1);
                    }

                    int id = Maths.Lerp(0, len, n);
                    pix[y * dimX + x] = falloff[id];
                }
            }

            texture = new TXTure(pix);
            sprite = TXTSprite.Create(texture, $"Circle X{radius} {edgePower.ToString("F2")} {edgeStart.ToString("F2")}", 0, (ushort)dimX, (ushort)dimY, new Vector2(0.5f, 0.5f));
            return true;
        }

        public static bool LoadLegacy(string path, out TXTure texture, out TXTSprite sprite)
        {
            texture = null;
            sprite = null;
            if (!File.Exists(path)) { return false; }

            string name = Path.GetFileNameWithoutExtension(path);

            char[] pixels = null;
            ushort width = 0;
            ushort height = 0;

            Vector2 pivot = Vector2.zero;

            using (FileStream stream = new FileStream(path, FileMode.Open))
            using (StreamReader read = new StreamReader(stream))
            {
                string[] init = new string[5];
                for (int i = 0; i < init.Length; i++)
                {
                    string rd = read.ReadLine().Trim();
                    switch (i)
                    {
                        case 0:
                            width = ushort.TryParse(rd, out ushort w) ? w : (ushort)1;
                            break;
                        case 1:
                            height = ushort.TryParse(rd, out ushort h) ? h : (ushort)1;
                            break;
                        case 2:
                            pivot.x = float.TryParse(rd, NumberStyles.Any, CultureInfo.InvariantCulture, out float pX) ? pX : 0;
                            break;
                        case 3:
                            pivot.x = float.TryParse(rd, NumberStyles.Any, CultureInfo.InvariantCulture, out float pY) ? pY : 0;
                            break;
                    }
                }

                int reso = width * height;
                pixels = new char[reso];

                for (int y = 0; y < height; y++)
                {
                    string str = read.ReadLine().Trim();
                    for (int x = 0; x < width; x++)
                    {
                        char c = str[x];
                        int i = y * width + x;
                        pixels[i] = c == EMPTY_PIXEL ? '\0' : c == BLACK_PIXEL ? ' ' : c;
                    }
                }
                texture = new TXTure(pixels);
                sprite = TXTSprite.Create(texture, name, 0, width, height, pivot);
            }
            return true;
        }
    }
}