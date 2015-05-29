namespace Nine.Graphics.OpenGL
{
    partial class SpriteRenderer
    {
        private static readonly string vertexShaderSource = @"
#version 140

precision highp float;

uniform mat4 projection;
uniform mat4 modelview;

in vec3 in_position;
in vec4 in_color;
in vec2 in_uv;

out vec2 uv;
out vec2 color;

void main(void)
{
    uv = in_uv;
    color = in_color;
    gl_Position = projection * modelview * vec4(in_position, 1);
}";

        private static readonly string fragmentShaderSource = @"
#version 140

precision highp float;

in vec2 uv;
in vec4 color;

uniform sampler2D Texture;

void main(void)
{
    gl_FragColor = color * texture2D(Texture, uv);
}";
    }
}
