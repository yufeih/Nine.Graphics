namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using OpenTK.Graphics.OpenGL;

    partial class SpriteRenderer
    {
        Vertex[] vertexBuffer;
        GCHandle pinnedVertexBuffer;

        static ushort[] indexBuffer;

        // Pin indexBuffer to the memory for the whole lifetime of the app.
        static GCHandle pinnedIndexBuffer;
        static object indexBufferLock = new object();

        void CreateBuffers(int initialSpriteCapacity)
        {
            this.vertexBuffer = new Vertex[initialSpriteCapacity * 4];
            this.pinnedVertexBuffer = GCHandle.Alloc(vertexBuffer, GCHandleType.Pinned);
        }

        void EnsureBufferCapacity(int spriteCount)
        {
            if (spriteCount * 4 > vertexBuffer.Length)
            {
                pinnedVertexBuffer.Free();

                Array.Resize(ref vertexBuffer, spriteCount * 4);

                pinnedVertexBuffer = GCHandle.Alloc(vertexBuffer, GCHandleType.Pinned);
            }

            if (indexBuffer == null || spriteCount * 6 > indexBuffer.Length)
            {
                lock (indexBufferLock)
                {
                    var start = 0;

                    if (indexBuffer != null)
                    {
                        start = indexBuffer.Length / 6;
                        pinnedIndexBuffer.Free();
                    }

                    Array.Resize(ref indexBuffer, spriteCount * 6);

                    pinnedIndexBuffer = GCHandle.Alloc(indexBuffer, GCHandleType.Pinned);

                    PopulateIndex(start, spriteCount);
                }
            }
        }

        void DisposeBuffers()
        {
            if (vertexBuffer != null)
            {
                pinnedVertexBuffer.Free();
            }
        }
    }
}
