#if DX
namespace Nine.Graphics.DirectX
{
#else
namespace Nine.Graphics.OpenGL
{
#endif
    using Rendering;
    using System;
    using System.ComponentModel;
    using System.Numerics;

    public partial class DynamicPrimitiveRenderer
    {
        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void BeginPrimitive(PrimitiveType primitiveType, TextureId? texture, Matrix4x4? world = null, float lineWidth = 1)
        {
            if (hasPrimitiveBegin) throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

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

            isDirty = true;
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

            //vertexSegments[vertexSegments.Count] = baseSegmentVertex + currentVertex;
            //indexSegments[indexSegments.Count] = baseSegmentIndex + currentIndex;

            batches.Add(currentPrimitive);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Color color)
            => this.AddVertex(position, color, Vector2.Zero);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Color color, Vector2 uv)
            => this.AddVertex(position, new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, 1.0f), uv);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Vector3 color)
            => this.AddVertex(position, new Vector4(color, 1.0f), Vector2.Zero);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Vector3 color, Vector2 uv)
            => this.AddVertex(position, new Vector4(color, 1.0f), uv);

        /// <summary>
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void AddVertex(Vector3 position, Vector4 color, Vector2 uv)
            => this.AddVertex(new Vertex { Position = position, Color = color, TextureCoordinate = uv });

        private void AddVertex(Vertex vertex)
        {
            if (!hasPrimitiveBegin) throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

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
            if (!hasPrimitiveBegin) throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");
            if (index > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(index));

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
    }
}
