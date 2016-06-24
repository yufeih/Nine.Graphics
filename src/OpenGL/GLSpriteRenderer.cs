namespace Nine.Graphics.Rendering
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Numerics;

    public class GLSpriteRenderer : SpriteRenderer<int>, IDisposable
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
        private int indexBufferId;
        
        public GLSpriteRenderer(GLTextureFactory textureFactory, int initialSpriteCapacity = 1024)
            : base(textureFactory, initialSpriteCapacity)
        {
            CreateBuffers();
            CreateShaders();
        }

        private void CreateBuffers()
        {
            GLDebug.CheckAccess();

            vertexBufferId = GL.GenBuffer();
            indexBufferId = GL.GenBuffer();
        }

        private void CreateShaders()
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

        protected override unsafe void BeginDraw(ref Matrix4x4 projection, ushort* pIndex, int indexCount)
        {
            GLDebug.CheckAccess();

            GL.UseProgram(shaderProgramHandle);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexCount * sizeof(ushort)), (IntPtr)pIndex, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex2D.SizeInBytes, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, Vertex2D.SizeInBytes, 8);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex2D.SizeInBytes, 8 + 4);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            fixed (float* ptr = &projection.M11)
            {
                GL.UniformMatrix4(transformLocation, 1, false, ptr);
            }
        }

        protected override unsafe void Draw(Vertex2D* pVertex, int vertexCount, int texture, bool isTransparent)
        {
            GLDebug.CheckAccess();

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexCount * Vertex2D.SizeInBytes), (IntPtr)pVertex, BufferUsageHint.StaticDraw);
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

        protected override void EndDraw()
        {
            
        }

        public void Dispose()
        {
            GLDebug.CheckAccess();

            GL.DeleteBuffer(vertexBufferId);
            GL.DeleteProgram(shaderProgramHandle);
        }
    }
}
