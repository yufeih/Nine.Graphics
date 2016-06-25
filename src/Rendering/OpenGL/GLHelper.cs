namespace Nine.Graphics.Rendering
{
    using OpenTK;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Text;
    using System.Threading;

    static class GLHelper
    {
        public static void ToMatrix4(ref Matrix4x4 m, out Matrix4 result)
        {
            result.Row0.X = m.M11;
            result.Row0.Y = m.M12;
            result.Row0.Z = m.M13;
            result.Row0.W = m.M14;

            result.Row1.X = m.M21;
            result.Row1.Y = m.M22;
            result.Row1.Z = m.M23;
            result.Row1.W = m.M24;

            result.Row2.X = m.M31;
            result.Row2.Y = m.M32;
            result.Row2.Z = m.M33;
            result.Row2.W = m.M34;

            result.Row3.X = m.M41;
            result.Row3.Y = m.M42;
            result.Row3.Z = m.M43;
            result.Row3.W = m.M44;
        }
    }
}
