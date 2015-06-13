namespace Nine.Graphics.OpenGL
{
    using System.Diagnostics;
    using OpenTK.Graphics.OpenGL;
    using System;

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

uniform sampler2D Texture;

void main(void)
{
    gl_FragColor = color * texture2D(Texture, uv);
}";

        private int shaderProgramHandle, transformLocation;
        private int vertexBufferId;
        private static int indexBufferId = GL.GenBuffer();
        
        private void PlatformCreateBuffers()
        {
            vertexBufferId = GL.GenBuffer();
        }

        private void PlatformCreateShaders()
        {
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

            Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
            GL.UseProgram(shaderProgramHandle);

            // Set uniforms
            transformLocation = GL.GetUniformLocation(shaderProgramHandle, "transform");
        }

        private unsafe void PlatformDraw(Vertex* pVertex, ushort* pIndex, int vertexCount, int indexCount, TextureSlice texture)
        {
            GL.UseProgram(shaderProgramHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexCount * Vertex.SizeInBytes), (IntPtr)pVertex, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexCount * sizeof(ushort)), (IntPtr)pIndex, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, 0, 0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, true, 0, 0);

            GL.BindTexture(TextureTarget.Texture2D, texture.Texture);

            GL.DrawElements(BeginMode.Triangles, indexCount, DrawElementsType.UnsignedShort, 0);
        }

        private void PlatformDispose()
        {
            GL.DeleteBuffer(vertexBufferId);
        }
    }
}
