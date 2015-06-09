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

        readonly TextureFactory textureFactory;

        /// <summary>
        /// 
        /// </summary>
        public DynamicPrimitive(TextureFactory textureFactory, int initialBufferCapacity = 32, int maxBufferSizePerPrimitive = 32768)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.CreateBuffers(initialBufferCapacity, maxBufferSizePerPrimitive);
            this.CreateShaders();
            this.Clear();
        }

        public void Draw(IGraphicsHost host)
        {
            // TODO: Camera system
            OpenTK.Matrix4 projection = OpenTK.Matrix4.Identity;
            OpenTK.Matrix4.CreateOrthographicOffCenter(0, host.Width, host.Height, 0, 0, 1, out projection);

            this.Draw(projection);
        }

        public void Draw(OpenTK.Matrix4 wvp)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException("Begin cannot be called until End has been successfully called.");

            var count = batches.Count;
            if (count > 0)
            {
                // Apply shaders
                GL.UseProgram(shaderProgramHandle);

                // Set shader paramaters
                GL.UniformMatrix4(transformLocation, false, ref wvp);

                // Bind buffers
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

                // if dirty update buffers
                if (isDirty)
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * Vertex.SizeInBytes), vertexData, BufferUsageHint.StaticDraw);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexData.Length * sizeof(ushort)), indexData, BufferUsageHint.StaticDraw);
                    isDirty = false;
                }
                
                // Apply vertex layout
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 12);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 28);

                // Enable vertex layout
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);

                // Enable depth
                GL.Enable(EnableCap.DepthTest);
                
                // Draw batches
                for (int i = 0; i < count; ++i)
                    DrawBatch(batches[i]);

                // Reset features
                GL.LineWidth(1.0f);
                GL.Disable(EnableCap.DepthTest);

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

            // TODO: Add entry transform matrix

            GL.LineWidth(entry.LineWidth);

            // Apply texture
            if (entry.Texture != null)
            {
                var texture = textureFactory.GetTexture(entry.Texture.Value);
                GL.BindTexture(TextureTarget.Texture2D, (texture == null) ? blankTexture : texture.Texture);
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, blankTexture);
            }

            // Draw geometry
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
            this.DisposeBuffers();
            this.DisposeShaders();
        }
    }
}
