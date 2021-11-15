using Joonaxii.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MathSys = System.Math;

namespace Joonaxii.Physics.Demo.Rendering
{
    public class TXTRenderer
    {
        public static TXTRenderer Instance;

        public const int BUFFER_W = 128;
        public const int BUFFER_H = BUFFER_W;

        public const int INFO_START_Y = 0;
        public const int INFO_HEIGHT = 10;

        public const int GAME_AREA_START = INFO_START_Y + INFO_HEIGHT;
        public const int GAME_AREA_HEIGHT = BUFFER_H - GAME_AREA_START;

        private char[] _buffer = new char[BUFFER_W * BUFFER_H];
        private uint[] _depthBuffer = new uint[BUFFER_W * BUFFER_H];
        private BitArray _reservedInfoArea;

        private const int CMD = 0x00000000;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private List<SpriteRenderer> _renderers = new List<SpriteRenderer>(512);
        private PriorityQueue<SpriteRenderer> _batch = new PriorityQueue<SpriteRenderer>();

        private Dictionary<ushort, ushort> _regionLengths = new Dictionary<ushort, ushort>();

        private Queue<(string data, int x, int y)> _infoTexts = new Queue<(string data, int x, int y)>();

        public TXTRenderer()
        {
            Instance = this;
            _reservedInfoArea = new BitArray(INFO_HEIGHT * BUFFER_W, false);

            ClearInfoArea(true);
            ConsoleHelper.SetCurrentFont("Terminal", 8, 8);
            SetSize(BUFFER_W, BUFFER_H + 1);
        }

        public static void SetSize(int width, int height)
        {
            int w = MathSys.Min(width, Console.LargestWindowWidth);
            int h = MathSys.Min(height, Console.LargestWindowHeight);

            Console.SetWindowSize(w, h);
            IntPtr consolePtr = GetConsoleWindow();

            DeleteMenu(GetSystemMenu(consolePtr, false), SC_MAXIMIZE, CMD);
            DeleteMenu(GetSystemMenu(consolePtr, false), SC_SIZE, CMD);
        }

        public void RegisterRenderer(SpriteRenderer rend)
        {
            lock(_renderers)
            {
                _renderers.Add(rend);
            }   
        }

        public ushort ReserveRegion(byte x, byte y, ushort length)
        {
            ushort id = (ushort)(x + (y << 8)); 

            x++;
            if (x >= BUFFER_W - 2)
            {
                x = 1;
                y++;
                if (y >= INFO_HEIGHT - 1) { return 0; }
            }

            y++;
            if (y >= INFO_HEIGHT - 1) { return 0; }

            _regionLengths.TryGetValue(id, out var cLen);

            if(length == cLen) { return cLen; }

            ushort finalLen = length > cLen ? length : cLen;

            _regionLengths.Remove(id);

            ushort len = 0;
            for (int i = 0; i < finalLen; i++)
            {
                int ii = y * BUFFER_W + x;

                if(i < length)
                {
                    _reservedInfoArea[ii] = true;
                    len++;
                }

                x++;
                if (x >= BUFFER_W - 2)
                {
                    x = 1;
                    y++;
                    if (y >= INFO_HEIGHT) { break; }
                }
            }

            _regionLengths.Add(id, len);
            return len;
        }


        public void ReleaseRegion(byte x, byte y, ushort length)
        {
            ushort id = (ushort)(x + (y << 8));

            x++;
            if(x >= BUFFER_W - 2)
            {
                x = 1;
                y++;
                if (y >= INFO_HEIGHT - 1) { return; }
            }

            y++;
            if(y >= INFO_HEIGHT - 1) { return; }

            for (int i = 0; i < length; i++)
            {
                int ii = y * BUFFER_W + x;
                _reservedInfoArea[ii] = false;

                x++;
                if(x >= BUFFER_W - 2)
                {
                    x = 1;
                    y++;
                    if(y >= INFO_HEIGHT) { break; }
                }
            }

            _regionLengths.Remove(id);
        }

        public void ReleaseReservedRegions()
        {
            _reservedInfoArea.SetAll(false);
            _regionLengths.Clear();
        }

        public void WriteInfo(string info, int x, int y)
        {
            lock (_infoTexts)
            {
                _infoTexts.Enqueue((info, x, y));
            }
        }

        public void WriteReserved(string data, byte x, byte y, ushort lengthPadding)
        {
            if(!_regionLengths.TryGetValue((ushort)(x + (y << 8)), out ushort len))
            {
                len = ReserveRegion(x, y, lengthPadding);
            }

            x++;
            if (x >= BUFFER_W - 2)
            {
                x = 1;
                y++;
                if (y >= INFO_HEIGHT - 1) { return; }
            }

            y++;
            if (y >= INFO_HEIGHT - 1) { return; }

            for (int i = 0; i < len; i++)
            {
                int ii = y * BUFFER_W + x;
                _buffer[ii] = i < data.Length ? data[i] : ' ';

                x++;
                if (x >= BUFFER_W - 2)
                {
                    x = 1;
                    y++;
                    if (y >= INFO_HEIGHT) { break; }
                }
            }
        }

        private void WriteInfoToBuffer(string info, int x, int y)
        {
            int l = info.Length;

            int curX = 1 + x;
            int curY = y + 1;

            for (int i = 0; i < l; i++)
            {
                int ii = curY * BUFFER_W + curX;
                _buffer[ii] = info[i];

                curX++;
                if (curX >= BUFFER_W - 1)
                {
                    curX = 1;
                    curY++;
                    if (curY >= INFO_HEIGHT - 1) { break; }
                }
            }
        }

        public void ClearInfoArea(bool generateEdge, bool ignoreReserved = true)
        {
            int start = generateEdge ? 0 : 1;
            int endX = generateEdge ? BUFFER_W : BUFFER_W - 1;
            int endY = generateEdge ? INFO_HEIGHT : INFO_HEIGHT - 1;

            for (int y = start; y < endY; y++)
            {
                bool yAtEdge = y == 0 || y == INFO_HEIGHT - 1;
                byte yVal = 0;

                //00 - 01 - 10
                yVal = (byte)(yAtEdge ? 1 << (y == 0 ? 0 : 1) : 0);
                for (int x = start; x < endX; x++)
                {
                    //00 - 01 - 10
                    bool xAtEdge = x == 0 || x == BUFFER_W - 1;
                    byte xVal = (byte)(xAtEdge ? 1 << (x == 0 ? 2 : 3) : 0);

                    //00 00
                    int state = yVal + xVal;

                    int i = y * BUFFER_W + x;

                    if (ignoreReserved & _reservedInfoArea[i]) { continue; }
                    switch (state)
                    {
                        case 0:
                            _buffer[i] = ' ';
                            break;
                        case 1:
                        case 2:
                            _buffer[i] = '-';
                            break;
                
                        case 4:
                        case 8:
                            _buffer[i] = '|';
                            break;
                
                        case 5:
                        case 10:
                            _buffer[i] = '/';
                            break;
                   
                        case 6:
                        case 9:
                            _buffer[i] = '\\';
                            break;
                    }
                }
            }
        }

        public void Render()
        {
            BufferEX.Memset(_depthBuffer, 0, GAME_AREA_START * BUFFER_W, BUFFER_W * GAME_AREA_HEIGHT);
            BufferEX.Memset(_buffer, ' ', GAME_AREA_START * BUFFER_W, BUFFER_W * GAME_AREA_HEIGHT);

            ClearInfoArea(true);

            lock (_infoTexts)
            {
                while (_infoTexts.Count > 0)
                {
                    var (data, x, y) = _infoTexts.Dequeue();
                    WriteInfoToBuffer(data, x, y);
                }
            }

            lock (_renderers)
            {
                for (int i = 0; i < _renderers.Count; i++)
                {
                    var rend = _renderers[i];
                    if (rend.IsActive)
                    {
                        _batch.Enqueue(rend);
                    }
                }
            }

            while(_batch.Count > 0)
            {
                var rend = _batch.Dequeue();
                rend.Draw(_buffer, _depthBuffer);
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(_buffer);
        }
    }
}
