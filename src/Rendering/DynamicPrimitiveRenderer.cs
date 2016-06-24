#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using Rendering;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    
    public sealed partial class DynamicPrimitiveRenderer : IDynamicPrimitiveRenderer, IDisposable
    {
        struct Vertex
        {
            public Vector3 Position;
            public Vector4 Color;
            public Vector2 TextureCoordinate;

            public const int SizeInBytes = 12 + 16 + 8;
        }

        struct PrimitiveGroupEntry
        {
            public Matrix4x4? World;
            public TextureId? Texture;
            public float LineWidth;
            public PrimitiveType PrimitiveType;
            public int StartVertex;
            public int VertexCount;
            public int StartIndex;
            public int IndexCount;
            public int Segment;
        }

        private bool hasPrimitiveBegin = false;
        private Vertex[] vertexData;
        private ushort[] indexData;

        private PrimitiveGroupEntry currentPrimitive;
        private int currentSegment;
        private int currentVertex;
        private int currentIndex;
        private int currentBaseVertex;
        private int currentBaseIndex;
        private int baseSegmentVertex;
        private int baseSegmentIndex;
        private int beginSegment;
        private bool isDirty = true;

        private List<PrimitiveGroupEntry> batches = new List<PrimitiveGroupEntry>();
        private List<int> vertexSegments = new List<int>();
        private List<int> indexSegments = new List<int>();

        private readonly TextureFactory textureFactory;

        public void Draw(Matrix4x4 projection, Matrix4x4 view)
        {
            if (hasPrimitiveBegin) throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

            if (batches.Count <= 0)
            {
                return;
            }

            if (isDirty)
            {
                PlatformUpdateBuffers();
                isDirty = false;
            }

            Matrix4x4 wvp = view * projection;
            PlatformBeginDraw(ref wvp);
            
            for (int i = 0; i < batches.Count; ++i)
                PlatformDrawBatch(batches[i]);

            PlatformEndDraw();
        }
        
        public void Dispose()
        {
            PlatformDispose();
        }

        public override string ToString()
        {
            return $"{ nameof(DynamicPrimitiveRenderer) }: { batches.Count } primitives";
        }
    }
}
