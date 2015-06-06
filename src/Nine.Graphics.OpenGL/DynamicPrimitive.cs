namespace Nine.Graphics.OpenGL
{
    using System;
    using System.Numerics;
    using OpenTK.Graphics.OpenGL;

    public partial class DynamicPrimitive : IDisposable
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

        /// <summary>
        /// 
        /// </summary>
        public DynamicPrimitive(int initialBufferCapacity = 32, int maxBufferSizePerPrimitive = 32768)
        {
            this.CreateBuffers(initialBufferCapacity, maxBufferSizePerPrimitive);
            this.CreateShaders();
            this.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddRectangle(Vector2 min, Vector2 max, Vector3 color)
        {
            this.BeginPrimitive(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, null); // Lines
            {
                this.AddVertex(new Vector3(min.X, min.Y, 0), color, new Vector2());
                this.AddVertex(new Vector3(min.X, max.Y, 0), color, new Vector2());
                this.AddVertex(new Vector3(max.X, max.Y, 0), color, new Vector2());

                this.AddVertex(new Vector3(min.X, min.Y, 0), color, new Vector2());
                this.AddVertex(new Vector3(max.X, min.Y, 0), color, new Vector2());
                this.AddVertex(new Vector3(max.X, max.Y, 0), color, new Vector2());


                //this.AddVertex(new Vector3(min.X, min.Y, 0), color, new Vector2());
                //this.AddVertex(new Vector3(min.X, max.Y, 0), color, new Vector2());
                //this.AddVertex(new Vector3(max.X, max.Y, 0), color, new Vector2());
                //this.AddVertex(new Vector3(max.X, min.Y, 0), color, new Vector2());

                //this.AddIndex(0);
                //this.AddIndex(1);
                //this.AddIndex(1);
                //this.AddIndex(2);
                //this.AddIndex(2);
                //this.AddIndex(3);
                //this.AddIndex(3);
                //this.AddIndex(0);
            }
            this.EndPrimitive();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw(IGraphicsHost host)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

            var count = batches.Count;
            if (count > 0)
            {
                GL.UseProgram(shaderProgramHandle);

                // TODO: Move transform to batches
                OpenTK.Matrix4 projection = OpenTK.Matrix4.Identity;
                OpenTK.Matrix4.CreateOrthographicOffCenter(0, host.Width, host.Height, 0, 0, 1, out projection);
                GL.UniformMatrix4(transformLocation, false, ref projection);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

                if (isDirty)
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * Vertex.SizeInBytes), vertexData, BufferUsageHint.StaticDraw);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexData.Length * sizeof(ushort)), indexData, BufferUsageHint.StaticDraw);
                    isDirty = false;
                }
                
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 12);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 28);

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                for (int i = 0; i < count; ++i)
                    DrawBatch(batches[i]);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.DisableVertexAttribArray(2);
            }
        }

        private void DrawBatch(PrimitiveGroupEntry entry)
        {
            if (entry.VertexCount <= 0 && entry.IndexCount <= 0)
                return;
            
            if (entry.IndexCount > 0)
            {
                GL.DrawElements(entry.PrimitiveType, entry.IndexCount, DrawElementsType.UnsignedShort, entry.StartIndex * sizeof(ushort));
            }
            else
            {
                GL.DrawArrays(entry.PrimitiveType, entry.StartVertex, entry.VertexCount);
            }
        }

        public void Dispose()
        {
            DisposeBuffers();
        }
    }
}
