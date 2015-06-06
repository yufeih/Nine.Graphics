namespace Nine.Graphics.OpenGL
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Text;

    static class OpenGLExtensions
    {
        public static void PrintProgramInfo(int program)
        {
            PrintProgramAttributes(program);
            PrintProgramUniforms(program);
        }

        public static void PrintProgramAttributes(int program)
        {
            int count = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveAttributes, out count);

            Console.WriteLine("\nAttributes(program: " + program + ", count: " + count + "):");

            for (int i = 0; i < count; i++)
            {
                int length = 0, size = 0;
                ActiveAttribType type;
                var name = new StringBuilder();

                GL.GetActiveAttrib(program, i, 256, out length, out size, out type, name);

                var location = GL.GetAttribLocation(program, name.ToString());
                var typeName = Enum.GetName(typeof(ActiveAttribType), type);

                Console.WriteLine(string.Format("\tLocation: {1}, Type: {2}, Name: {0}", name.ToString(), location, typeName));
            }
        }

        public static void PrintProgramUniforms(int program)
        {
            int count = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out count);

            Console.WriteLine("\nUniforms(program: " + program + ", count: " + count + "):");

            for (int i = 0; i < count; i++)
            {
                int length = 0, size = 0;
                ActiveUniformType type;
                var name = new StringBuilder();

                GL.GetActiveUniform(program, i, 256, out length, out size, out type, name);

                var location = GL.GetUniformLocation(program, name.ToString());
                var typeName = Enum.GetName(typeof(ActiveUniformType), type);

                Console.WriteLine(string.Format("\tLocation: {1}, Type: {2}, Name: {0}", name.ToString(), location, typeName));
            }
        }
    }
}
