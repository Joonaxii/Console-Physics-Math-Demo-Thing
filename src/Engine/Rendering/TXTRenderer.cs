using Joonaxii.Collections.ListQueue;
using Joonaxii.Engine.Core;
using Joonaxii.MathJX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Joonaxii.Engine.Rendering.TXT
{
    public class TXTRenderer
    {
        public static TXTRenderer Instance;

        public const int SCREEN_W = 960;
        public const int SCREEN_H = 960;
        
        public const int RES_X = SCREEN_W + 36;
        public const int RES_Y = SCREEN_H + 48;
        
        public const int OFFSET_X = (RES_X / 2);
        public const int OFFSET_Y = (RES_Y / 2);

        public const int BUFFER_W = (int)(SCREEN_W / 8);
        public const int BUFFER_H = (int)(SCREEN_H / 8);

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
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, [In, Out] ConsoleCursorInfo lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, [In] ConsoleCursorInfo lpConsoleCursorInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, [Out] ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, Coord dwSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out SmallRect lpRect);
        
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        private List<TXTSpriteRenderer> _renderers = new List<TXTSpriteRenderer>(512);
        private ListQueue<TXTSpriteRenderer> _batch = new ListQueue<TXTSpriteRenderer>();

        private Dictionary<ushort, ushort> _regionLengths = new Dictionary<ushort, ushort>();
        private Queue<(string data, int x, int y)> _infoTexts = new Queue<(string data, int x, int y)>();

        public Rect Bounds { get => _bounds; }
        private Rect _bounds;

        public TXTRenderer()
        {
            Instance = this;
            _reservedInfoArea = new BitArray(INFO_HEIGHT * BUFFER_W, false);

            _bounds = new Rect(Vector2.zero, new Vector2(BUFFER_W, GAME_AREA_HEIGHT));

            Console.OutputEncoding = System.Text.Encoding.ASCII;

            ClearInfoArea(true);
            ConsoleHelper.SetCurrentFont("Terminal", 8, 8);
            SetSize(OFFSET_X, OFFSET_Y, RES_X, RES_Y, BUFFER_W, BUFFER_H);

            Setup();
        }

        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_SIZEBOX = 0x00040000;

        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_LINE_INPUT = 0x0002;
        private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        private static void Setup()
        {
            var console = GetConsoleWindow();
            SetWindowLong(console, -16, GetWindowLong(console, -16) & ~WS_MAXIMIZEBOX & ~WS_SIZEBOX);

            var hInput = GetStdHandle(-10);
            GetConsoleMode(hInput, out var prev);
            SetConsoleMode(hInput, ENABLE_EXTENDED_FLAGS | (prev & ~ENABLE_MOUSE_INPUT & ~ENABLE_LINE_INPUT & ~ENABLE_QUICK_EDIT_MODE));

            ToggleCursor(false);
        }

        [StructLayout(LayoutKind.Sequential)]
        private class ConsoleCursorInfo
        {
            public uint dwSize;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bVisible;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class Coord
        {
            public short x;
            public short y;

            public Coord(short x, short y)
            {
                this.x = x;
                this.y = y;
            }
        }
              
        [StructLayout(LayoutKind.Sequential)]
        private class SmallRect
        {
            public short left;
            public short top;

            public short right;
            public short bottom;     
        }
            
        [StructLayout(LayoutKind.Sequential)]
        private class ConsoleScreenBufferInfo
        {
            public Coord dwSize;
            public Coord dwCursorPosition;

            public short wAttributes;
            public SmallRect srWindow;

            public Coord dwMaximumWindowSize;
        }

        public static void ToggleCursor(bool toggle)
        {
            var hndl = GetStdHandle(-11);

            ConsoleCursorInfo info = new ConsoleCursorInfo();
            GetConsoleCursorInfo(hndl, info);
            info.dwSize = 1;
            info.bVisible = toggle;

            SetConsoleCursorInfo(hndl, info);
        }

        public static void SetSize(int x, int y, int width, int height, int cW, int cH)
        {
            var console = GetConsoleWindow();

            var scrn = GetDesktopWindow();
            GetWindowRect(scrn, out var screenRect);

            if(screenRect == null) { screenRect = new SmallRect() { bottom = 0, left = 0, right = 1920, top = 1080 }; }

            int w = screenRect.right - screenRect.left;
            int h = screenRect.bottom - screenRect.top;

            Coord coord = new Coord((short)cW, (short)cH);

            var hConsoleOutput = GetStdHandle(-11);
            SetConsoleScreenBufferSize(hConsoleOutput, coord);
            MoveWindow(console, 0, 0, width, height, true);
        }

        public void RegisterRenderer(TXTSpriteRenderer rend)
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
                    if (rend.Enabled)
                    {
                        _batch.Enqueue(rend);
                    }
                }
                _batch.Sort();
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
