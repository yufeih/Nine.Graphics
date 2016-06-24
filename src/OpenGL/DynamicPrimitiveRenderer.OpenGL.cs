namespace Nine.Graphics.OpenGL
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Numerics;

    partial class DynamicPrimitiveRenderer
    {
        static readonly string vertexShaderSource = @"
#version 140

uniform mat4 transform;

in vec3 in_position;
in vec4 in_color;
in vec2 in_uv;

out vec2 uv;
out vec4 color;

void main(void)
{
    uv = in_uv;
    color = in_color;
    gl_Position = transform * vec4(in_position, 1);
}";

        static readonly string fragmentShaderSource = @"
#version 140

precision highp float;

uniform sampler2D Texture;

in vec2 uv;
in vec4 color;

out vec4 out_color;

void main(void)
{
    out_color = color * texture2D(Texture, uv);
}";

        private int shaderProgramHandle, transformLocation;
        private int[] VBOid;

        public DynamicPrimitiveRenderer(TextureFactory textureFactory, int initialBufferCapacity = 32)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.textureFactory = textureFactory;
            this.PlatformCreateBuffers(initialBufferCapacity);
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers(int initialBufferCapacity)
        {
            GLDebug.CheckAccess();

            this.vertexData = new Vertex[512];
            this.indexData = new ushort[6];
            this.VBOid = new int[2];

            GL.GenBuffers(2, VBOid);
        }

        private void PlatformCreateShaders()
        {
            GLDebug.CheckAccess();

            var vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            var fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            // Create program
            shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

            GL.BindAttribLocation(shaderProgramHandle, 0, "in_position");
            GL.BindAttribLocation(shaderProgramHandle, 1, "in_color");
            GL.BindAttribLocation(shaderProgramHandle, 2, "in_uv");

            GL.LinkProgram(shaderProgramHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            GL.UseProgram(shaderProgramHandle);

            // Set uniforms
            transformLocation = GL.GetUniformLocation(shaderProgramHandle, "transform");
        }

        private void PlatformUpdateBuffers()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * Vertex.SizeInBytes), vertexData, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexData.Length * sizeof(ushort)), indexData, BufferUsageHint.StaticDraw);
        }

        private unsafe void PlatformBeginDraw(ref Matrix4x4 wvp)
        {
            GLDebug.CheckAccess();

            // Apply shaders
            GL.UseProgram(shaderProgramHandle);

            // Set shader paramaters
            fixed (float* ptr = &wvp.M11)
            {
                GL.UniformMatrix4(transformLocation, 4*4, false, ptr);
            }

            // Bind buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

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
        }

        private void PlatformDrawBatch(PrimitiveGroupEntry entry)
        {
            if (entry.VertexCount <= 0 && entry.IndexCount <= 0)
                return;
            
            // TODO: Add entry transform matrix
            
            GL.LineWidth(entry.LineWidth);
            
            // Apply texture
            var texture = textureFactory.GetTexture(entry.Texture ?? TextureId.White);
            if (texture == null)
                return;
            
            GL.BindTexture(TextureTarget.Texture2D, texture.PlatformTexture);
            
            // Draw geometry
            if (entry.IndexCount > 0)
            {
                GL.DrawElements((OpenTK.Graphics.OpenGL.PrimitiveType)entry.PrimitiveType, entry.IndexCount, DrawElementsType.UnsignedShort, entry.StartIndex * sizeof(ushort));
            }
            else
            {
                GL.DrawArrays((OpenTK.Graphics.OpenGL.PrimitiveType)entry.PrimitiveType, entry.StartVertex, entry.VertexCount);
            }
        }

        private void PlatformEndDraw()
        {
            GLDebug.CheckAccess();

            // Reset features
            GL.LineWidth(1.0f);
            GL.Disable(EnableCap.DepthTest);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
        }
        
        private void PlatformDispose()
        {
            GLDebug.CheckAccess();

            GL.DeleteBuffers(2, VBOid);
            GL.DeleteProgram(shaderProgramHandle);
        }
    }
}
