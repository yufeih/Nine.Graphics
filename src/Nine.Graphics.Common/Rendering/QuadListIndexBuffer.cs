#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using System;
    using System.Runtime.InteropServices;

    public sealed partial class QuadListIndexBuffer
    {
        private GCHandle pinnedIndex;
        private ushort[] indexData;
        private object indexDataLock = new object();

        public QuadListIndexBuffer(int initialQuadCapacity = 1024)
        {
            indexData = new ushort[initialQuadCapacity * 6];
            PopulateIndex(0, initialQuadCapacity);
            pinnedIndex = GCHandle.Alloc(indexData, GCHandleType.Pinned);
        }

        public void EnsureCapacity(int quadCount)
        {
            if (quadCount * 6 > indexData.Length)
            {
                lock (indexDataLock)
                {
                    var start = 0;

                    if (indexData != null)
                    {
                        start = indexData.Length / 6;
                    }

                    if (pinnedIndex.IsAllocated)
                    {
                        pinnedIndex.Free();
                    }

                    Array.Resize(ref indexData, quadCount * 6);

                    pinnedIndex = GCHandle.Alloc(indexData, GCHandleType.Pinned);

                    PopulateIndex(start, quadCount);
                }
            }
        }

        private void PopulateIndex(int start, int quadCount)
        {
            for (var i = start; i < quadCount; i++)
            {
                indexData[i * 6 + 0] = (ushort)(i * 4);
                indexData[i * 6 + 1] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 2] = (ushort)(i * 4 + 2);

                indexData[i * 6 + 3] = (ushort)(i * 4 + 1);
                indexData[i * 6 + 4] = (ushort)(i * 4 + 3);
                indexData[i * 6 + 5] = (ushort)(i * 4 + 2);
            }
        }
    }
}
