namespace Nine.Graphics.OpenGL
{
    using Nine.Graphics.OpenGL;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    partial class SpriteRenderer
    {
        private static readonly string vertexShaderSource = @"
#version 140

precision highp float;

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

in vec2 uv;
in vec4 color;

out vec4 out_color;

uniform sampler2D Texture;

void main(void)
{
    out_color = color.bgra * texture2D(Texture, uv);
}";

        private int shaderProgramHandle, transformLocation;
        private int vertexBufferId;
        
        public SpriteRenderer(TextureFactory textureFactory, QuadListIndexBuffer quadIndexBuffer, int initialSpriteCapacity = 1024)
        {
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));
            if (quadIndexBuffer == null) throw new ArgumentNullException(nameof(quadIndexBuffer));

            this.textureFactory = textureFactory;
            this.quadIndexBuffer = quadIndexBuffer;
            this.CreateBuffers(initialSpriteCapacity);
            this.PlatformCreateBuffers();
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers()
        {
            GLDebug.CheckAccess();

            vertexBufferId = GL.GenBuffer();
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

            //Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
            GL.UseProgram(shaderProgramHandle);

            // Set uniforms
            transformLocation = GL.GetUniformLocation(shaderProgramHandle, "transform");
        }

        private unsafe void PlatformBeginDraw(ref Matrix4x4 projection)
        {
            GLDebug.CheckAccess();

            GL.UseProgram(shaderProgramHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, Vertex.SizeInBytes, 8);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 8 + 4);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            fixed (float* ptr = &projection.M11)
            {
                GL.UniformMatrix4(transformLocation, 1, false, ptr);
            }
        }

        private unsafe void PlatformDraw(Vertex* pVertex, int vertexCount, int texture, bool isTransparent)
        {
            GLDebug.CheckAccess();

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexCount * Vertex.SizeInBytes), (IntPtr)pVertex, BufferUsageHint.StaticDraw);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            if (isTransparent)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            GL.DrawElements(BeginMode.Triangles, vertexCount / 4 * 6, DrawElementsType.UnsignedShort, 0);
        }

        private void PlatformEndDraw()
        {
            
        }

        private void PlatformDispose()
        {
            GLDebug.CheckAccess();

            GL.DeleteBuffer(vertexBufferId);
            GL.DeleteProgram(shaderProgramHandle);
        }
    }
}
