namespace Nine.Graphics
{
    static class MathHelpers
    {
        public static int NextPowerOfTwo(int num)
        {
            var n = num > 0 ? num - 1 : 0;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return ++n;
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }
    }
}
