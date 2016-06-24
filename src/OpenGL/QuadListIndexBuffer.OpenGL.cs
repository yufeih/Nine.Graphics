namespace Nine.Graphics.OpenGL
{
    using System;
    using OpenTK.Graphics.OpenGL;

    partial class QuadListIndexBuffer
    {
        private int indexBufferId = GL.GenBuffer();

        public void Apply()
        {
            GLDebug.CheckAccess();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexData.Length * sizeof(ushort)), pinnedIndex.AddrOfPinnedObject(), BufferUsageHint.StaticDraw);
        }
    }
}
