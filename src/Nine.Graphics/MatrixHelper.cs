namespace Nine.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Numerics;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MatrixHelper
    {
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
