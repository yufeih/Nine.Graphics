namespace Nine.Graphics
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Explicit)]
    public struct Color : IEquatable<Color>
    {
        public static readonly Color Empty = new Color();
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);

        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color Blue = new Color(0, 0, 255);

        [FieldOffset(0)]
        public readonly byte B;
        [FieldOffset(1)]
        public readonly byte G;
        [FieldOffset(2)]
        public readonly byte R;
        [FieldOffset(3)]
        public readonly byte A;

        [FieldOffset(0)]
        public readonly int Bgra;
        public int Rgba => (R << 24) | (G << 16) | (B << 8) | A;
        public int Argb => (A << 24) | (R << 16) | (G << 8) | B;

        public bool IsTransparent => A < 255;

        public Color(int bgra) { R = G = B = A = 0; Bgra = bgra; }
        public Color(byte r, byte g, byte b, byte a = 255) { Bgra = 0; R = r; G = g; B = b; A = a; }
        public Color(float r, float g, float b, float a = 1.0f)
        {
            r *= 255; g *= 255; b *= 255; a *= 255;
            if (r > 255) r = 255; if (r < 0) r = 0;
            if (g > 255) g = 255; if (g < 0) g = 0;
            if (b > 255) b = 255; if (b < 0) b = 0;
            if (a > 255) a = 255; if (a < 0) a = 0;

            Bgra = 0;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder(32);
            sb.Append("#");

            sb.Append(A.ToString("X2"));
            sb.Append(R.ToString("X2"));
            sb.Append(G.ToString("X2"));
            sb.Append(B.ToString("X2"));

            return sb.ToString();
        }

        public static Color Parse(string value)
        {
            byte a = 255, r, g, b;

            if (value.Contains(","))
            {
                var argb = value.Split(',');
                var i = argb.Length > 3 ? 1 : 0;

                if (i == 1) a = (byte)int.Parse(argb[0]);
                r = (byte)int.Parse(argb[i + 0]);
                g = (byte)int.Parse(argb[i + 1]);
                b = (byte)int.Parse(argb[i + 2]);
            }
            else
            {
                var i = value.StartsWith("#") ? 1 : 0;

                if (value.Length - i == 3)
                {
                    r = g = b = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                }
                else if (value.Length - i == 6)
                {
                    r = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                    g = (byte)(Hex(value[i + 2]) * 16 + Hex(value[i + 3]));
                    b = (byte)(Hex(value[i + 4]) * 16 + Hex(value[i + 5]));
                }
                else
                {
                    a = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                    r = (byte)(Hex(value[i + 2]) * 16 + Hex(value[i + 3]));
                    g = (byte)(Hex(value[i + 4]) * 16 + Hex(value[i + 5]));
                    b = (byte)(Hex(value[i + 6]) * 16 + Hex(value[i + 7]));
                }
            }

            return new Color(r, g, b, a);
        }

        private static int Hex(char c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c >= 'a' && c <= 'z') return c - 'a' + 10;
            if (c >= '0' && c <= '9') return c - '0';

            throw new ArgumentOutOfRangeException($"{ c } is not a valid for hex number");
        }

        public static Color operator *(Color color, float alpha)
        {
            return new Color((byte)(color.R * alpha),
                             (byte)(color.G * alpha),
                             (byte)(color.B * alpha),
                             (byte)(color.A * alpha));
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.Bgra == b.Bgra;
        }

        public static bool operator !=(Color a, Color b)
        {
            return a.Bgra != b.Bgra;
        }

        public override int GetHashCode()
        {
            return this.Bgra.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        public bool Equals(Color other)
        {
            return Bgra == other.Bgra;
        }
    }
}
