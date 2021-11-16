namespace Joonaxii.Engine.Core
{
    public static unsafe class BufferEX
    {
        public static void Memset(byte[] data, byte val, int start, int size)
        {
            fixed (byte* buf = data) { Memset(buf, val, start, size); }
        }

        public static void Memset(byte* buffer, byte val, int start, int size)
        {
            for (int i = start; i < start + size; i++)
            {
                buffer[i] = val;
            }
        }

        public static void Memset(char[] data, char val, int start, int size)
        {
            fixed (char* buf = data) { Memset(buf, val, start, size); }
        }

        public static void Memset(char* buffer, char val, int start, int size)
        {
            for (int i = start; i < start + size; i++)
            {
                buffer[i] = val;
            }
        }

        public static void Memset(uint[] data, uint val, int start, int size)
        {
            fixed (uint* buf = data) { Memset(buf, val, start, size); }
        }

        public static void Memset(uint* buffer, uint val, int start, int size)
        {
            for (int i = start; i < start + size; i++)
            {
                buffer[i] = val;
            }
        }
    }
}
