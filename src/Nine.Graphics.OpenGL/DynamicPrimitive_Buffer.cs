namespace Nine.Graphics.OpenGL
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    partial class DynamicPrimitive
    {
        private bool hasPrimitiveBegin = false;
        private Vertex[] vertexData;
        private ushort[] indexData;

        private List<PrimitiveGroupEntry> batches = new List<PrimitiveGroupEntry>();
        private List<int> vertexSegments = new List<int>();
        private List<int> indexSegments = new List<int>();

        private PrimitiveGroupEntry currentPrimitive;
        private int currentSegment;
        private int currentVertex;
        private int currentIndex;
        private int currentBaseVertex;
        private int currentBaseIndex;
        private int baseSegmentVertex;
        private int baseSegmentIndex;
        private int beginSegment;

        private int initialBufferCapacity;
        private int maxBufferSizePerPrimitive;
        
        private uint[] VBOid = new uint[2];

        void CreateBuffers(int initialBufferCapacity, int maxBufferSizePerPrimitive)
        {
            this.initialBufferCapacity = initialBufferCapacity;
            this.maxBufferSizePerPrimitive = maxBufferSizePerPrimitive;

            this.vertexData = new Vertex[512];
            this.indexData = new ushort[6];

            GL.GenBuffers(2, VBOid);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void BeginPrimitive(PrimitiveType primitiveType, TextureId? texture, Matrix4x4? world = null, float lineWidth = 1)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

            hasPrimitiveBegin = true;

            currentPrimitive = new PrimitiveGroupEntry();
            currentPrimitive.LineWidth = lineWidth;
            currentPrimitive.World = world;
            currentPrimitive.PrimitiveType = primitiveType;
            currentPrimitive.Texture = texture;
            currentPrimitive.StartVertex = currentVertex;
            currentPrimitive.StartIndex = currentIndex;

            currentBaseVertex = currentVertex;
            currentBaseIndex = currentIndex;

            beginSegment = currentSegment;
        }

        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void EndPrimitive()
        {
            hasPrimitiveBegin = false;
            currentPrimitive.Segment = currentSegment;
            currentPrimitive.VertexCount = currentVertex - currentPrimitive.StartVertex;
            currentPrimitive.IndexCount = currentIndex - currentPrimitive.StartIndex;

            vertexSegments[vertexSegments.Count - 1] = baseSegmentVertex + currentVertex;
            indexSegments[indexSegments.Count - 1] = baseSegmentIndex + currentIndex;
            
            batches.Add(currentPrimitive);
        }

        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Vector3 color, Vector2 uv) 
            => this.AddVertex(new Vertex { Position = position, Color = color, TextureCoordinate = uv });

        private void AddVertex(Vertex vertex)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

            var index = baseSegmentVertex + currentVertex;
            if (index >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);
            
            vertexData[index] = vertex;
            currentVertex++;
        }

        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddIndex(int index)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            if (baseSegmentIndex + currentIndex >= indexData.Length)
                Array.Resize(ref indexData, indexData.Length * 2);
            
            indexData[baseSegmentIndex + currentIndex++] = (ushort)(currentBaseVertex + index);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.batches.Clear();
            this.vertexSegments.Clear();
            this.indexSegments.Clear();

            this.vertexSegments.Add(0);
            this.indexSegments.Add(0);

            this.vertexSegments.Add(0);
            this.indexSegments.Add(0);

            this.currentSegment = 0;
            this.currentIndex = 0;
            this.currentVertex = 0;
            this.baseSegmentIndex = 0;
            this.baseSegmentVertex = 0;
        }

        void DisposeBuffers()
        {
            GL.DeleteBuffers(2, VBOid);
        }
    }
}
