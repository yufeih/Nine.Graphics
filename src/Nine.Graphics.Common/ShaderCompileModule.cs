#if DX
namespace Nine.Graphics.DirectX
#else
namespace Nine.Graphics.OpenGL
#endif
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.Framework.Runtime.Roslyn;

    public partial class ShaderCompileModule : ICompileModule
    {
        public static byte[] GetByteCode(string shaderPath)
        {
            throw new InvalidOperationException($"This method should have been replaced by { nameof(ShaderCompileModule) }");
        }

        public void AfterCompile(IAfterCompileContext context) { }
        public void BeforeCompile(IBeforeCompileContext context)
        {
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {

            }

            context.Compilation = context.Compilation;
        }
    }
}
