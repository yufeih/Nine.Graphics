namespace Nine.Graphics.OpenGL
{
    using OpenTK.Graphics.OpenGL;
    using System;

    partial class DynamicPrimitive
    {
        static readonly string vertexShaderSource = @"
#version 140

uniform mat4 transform;

in vec3 in_position;
in vec4 in_color;
in vec2 in_uv;

out vec2 uv;
out vec4 out_color;

void main(void)
{
    uv = in_uv;
    out_color = in_color;
    gl_Position = transform * vec4(in_position, 1);
}";

        static readonly string fragmentShaderSource = @"
#version 140

precision highp float;

uniform sampler2D Texture;

in vec2 uv;
in vec4 out_color;

out vec4 color;

void main(void)
{
    color = out_color;// * texture2D(Texture, uv);
}";

        int shaderProgramHandle, transformLocation;

        void CreateShaders()
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

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            OpenGLExtensions.PrintProgramInfo(shaderProgramHandle);
            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

            GL.UseProgram(shaderProgramHandle);

            // Set uniforms
            transformLocation = GL.GetUniformLocation(shaderProgramHandle, "transform");
        }
    }
}
