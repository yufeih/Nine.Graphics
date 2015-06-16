namespace Nine.Graphics
{
    using System;
    using System.Numerics;

    static class MathHelper
    {
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)(Math.PI * 2);

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

        public static Matrix4x4 CreateRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            var result = new Matrix4x4();
            CreateRotation(ref fromDirection, ref toDirection, out result);
            return result;
        }

        public static void CreateRotation(ref Vector3 fromDirection, ref Vector3 toDirection, out Matrix4x4 matrix)
        {
            var axis = Vector3.Cross(fromDirection, toDirection);
            axis = Vector3.Normalize(axis);

            if (float.IsNaN(axis.X))
            {
                matrix = Matrix4x4.Identity;
                return;
            }

            var angle = Vector3.Dot(fromDirection, toDirection);
            matrix = Matrix4x4.CreateFromAxisAngle(axis, (float)Math.Acos(angle));
        }
    }
}
